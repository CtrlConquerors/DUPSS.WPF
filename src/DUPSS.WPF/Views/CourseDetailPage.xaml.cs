using DUPSS.ApiClients;
using DUPSS.Common;
using DUPSS.DTO.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input; // For ICommand
// Removed using DUPSS.WPF.Views; as converters are now global

namespace DUPSS.WPF.Views
{
    // Removed BooleanToVisibilityConverter, BooleanToVisibilityConverterInverted, and HalfWidthConverter classes from here.
    // They are now defined in CommonConverters.cs under the DUPSS.WPF namespace and declared globally in App.xaml.

    public partial class CourseDetailPage : Page, INotifyPropertyChanged
    {
        // Injected Services
        private readonly CourseApiService _courseApiService;
        private readonly CourseEnrollApiService _courseEnrollApiService;
        private readonly UserApiService _userApiService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        // Data-bound properties
        private CoursePageData? _courseData;
        public CoursePageData? CourseData
        {
            get => _courseData;
            set
            {
                _courseData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowCourseContent));
            }
        }

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private bool _hasError = false;
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        private bool _noCourseFound = false;
        public bool NoCourseFound
        {
            get => _noCourseFound;
            set
            {
                _noCourseFound = value;
                OnPropertyChanged();
            }
        }

        public bool ShowCourseContent => CourseData?.Course != null && !IsLoading && !HasError && !NoCourseFound;

        private bool _isEnrolling = false;
        public bool IsEnrolling
        {
            get => _isEnrolling;
            set
            {
                _isEnrolling = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnrollButtonEnabled));
                OnPropertyChanged(nameof(EnrollButtonText));
            }
        }

        private bool _isEnrolledSuccessfully = false;
        public bool IsEnrolledSuccessfully
        {
            get => _isEnrolledSuccessfully;
            set
            {
                _isEnrolledSuccessfully = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnrollButtonEnabled));
            }
        }

        public bool IsEnrollButtonEnabled => !IsEnrolling && !IsEnrolledSuccessfully;
        public string EnrollButtonText => IsEnrolling ? "Loading..." : "Enroll Now";

        private string _enrollmentMessage = string.Empty;
        public string EnrollmentMessage
        {
            get => _enrollmentMessage;
            set
            {
                _enrollmentMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEnrollmentMessage));
            }
        }

        private string _enrollmentMessageClass = string.Empty;
        public string EnrollmentMessageClass
        {
            get => _enrollmentMessageClass;
            set
            {
                _enrollmentMessageClass = value;
                OnPropertyChanged();
            }
        }

        public bool HasEnrollmentMessage => !string.IsNullOrEmpty(EnrollmentMessage);

        private string? _currentLoggedInUserId;
        private List<CourseEnrollDTO>? _allEnrollments; // To check existing enrollments

        // Commands for buttons and hyperlinks
        public ICommand EnrollCommand { get; private set; }
        public ICommand GoToCourseContentCommand { get; private set; }
        public ICommand GoBackToCoursesCommand { get; private set; }
        public ICommand NavigateToInstructorCommand { get; private set; }


        // Constructor with Dependency Injection
        public CourseDetailPage(
            CourseApiService courseApiService,
            CourseEnrollApiService courseEnrollApiService,
            UserApiService userApiService,
            JwtAuthenticationStateProvider authStateProvider)
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext for bindings

            // Converters are now global, no need to add to page resources here.
            // Removed: this.Resources.Add("BooleanToVisibilityConverterInverted", new BooleanToVisibilityConverterInverted());
            // Removed: this.Resources.Add("HalfWidthConverter", new HalfWidthConverter());

            _courseApiService = courseApiService;
            _courseEnrollApiService = courseEnrollApiService;
            _userApiService = userApiService;
            _authStateProvider = authStateProvider;

            // Initialize Commands
            EnrollCommand = new RelayCommand(async () => await EnrollNow(), () => IsEnrollButtonEnabled);
            GoToCourseContentCommand = new RelayCommand(() => GoToCourseContent());
            GoBackToCoursesCommand = new RelayCommand(() => GoBackToCourses());
            NavigateToInstructorCommand = new RelayCommand<UserDTO>(async (instructor) => await NavigateToInstructor(instructor));
        }

        // Parameter for CourseId (set by navigation)
        public string CourseId { get; set; } = string.Empty;

        // Page Loaded event handler (similar to OnParametersSetAsync in Blazor)
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve CourseId from NavigationContext if available
            // This assumes the page is navigated to via a URI like /CourseDetailPage/C0001
            // You might need to adjust how CourseId is passed based on your navigation setup.
            // For example, if you navigate using MainFrame.Navigate(new CourseDetailPage("C0001")),
            // then you would pass the ID in the constructor.
            // For now, we assume it's set via a public property before Loaded.
            // If navigating via URI, you might need to parse it from NavigationService.Source.OriginalString

            // For simplicity, let's assume CourseId is set before Page_Loaded by the navigation system
            // (e.g., if you're using a custom navigation service that sets properties).
            // If not, you'd need to parse it from the URI.
            // Example:
            // if (NavigationService.Source != null && NavigationService.Source.OriginalString.Contains("/CourseDetailPage/"))
            // {
            //     CourseId = NavigationService.Source.OriginalString.Split('/').Last();
            // }

            await LoadCourseDataAsync();
        }

        private async Task LoadCourseDataAsync()
        {
            IsLoading = true;
            HasError = false;
            NoCourseFound = false;
            EnrollmentMessage = string.Empty;
            IsEnrolledSuccessfully = false;
            CourseData = null; // Clear previous data

            if (string.IsNullOrWhiteSpace(CourseId))
            {
                NoCourseFound = true;
                IsLoading = false;
                return;
            }

            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                _currentLoggedInUserId = null;
                if (user.Identity?.IsAuthenticated == true)
                {
                    _currentLoggedInUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(_currentLoggedInUserId))
                    {
                        Console.WriteLine("Warning: Authenticated user has no NameIdentifier claim for UserId.");
                    }
                }

                var fetchedCourseTask = _courseApiService.GetByIdAsync(CourseId);
                var allEnrollmentsTask = _courseEnrollApiService.GetAllAsync();
                // UserApiService.GetAllAsync() is removed as it's not used in this component.
                // If you need all members for other purposes, re-add it.

                await Task.WhenAll(fetchedCourseTask, allEnrollmentsTask);

                var fetchedCourse = fetchedCourseTask.Result;
                _allEnrollments = allEnrollmentsTask.Result;

                if (fetchedCourse != null && fetchedCourse.Consultant != null)
                {
                    // Image URL resolution (simplified for WPF, assuming images are in wwwroot/images/Courses and wwwroot/images/Users)
                    // WPF does not use IWebHostEnvironment for client-side image paths.
                    // The API should ideally provide the full relative path, or we construct it here.
                    // Assuming images are named CourseId.jpg and UserId.jpg
                    if (!string.IsNullOrEmpty(fetchedCourse.CourseId))
                    {
                        // Check if ImageUrl is already provided by DTO (from API)
                        if (string.IsNullOrEmpty(fetchedCourse.ImageUrl))
                        {
                            // Fallback if API doesn't provide it, assume default extension
                            fetchedCourse.ImageUrl = $"/images/Courses/{fetchedCourse.CourseId}.jpg";
                        }
                    }

                    if (!string.IsNullOrEmpty(fetchedCourse.Consultant.UserId))
                    {
                        if (string.IsNullOrEmpty(fetchedCourse.Consultant.ImageUrl))
                        {
                            // Fallback if API doesn't provide it, assume default extension
                            fetchedCourse.Consultant.ImageUrl = $"/images/Users/{fetchedCourse.Consultant.UserId}.jpg";
                        }
                    }

                    CourseData = new CoursePageData
                    {
                        Course = fetchedCourse,
                        Description = fetchedCourse.Description ?? "A detailed description for this course will be added soon.",
                        Instructor = fetchedCourse.Consultant
                    };

                    if (!string.IsNullOrEmpty(_currentLoggedInUserId))
                    {
                        var existingEnrollment = _allEnrollments?.FirstOrDefault(e =>
                            e.MemberId == _currentLoggedInUserId && e.CourseId == fetchedCourse.CourseId);
                        if (existingEnrollment != null)
                        {
                            IsEnrolledSuccessfully = true;
                            EnrollmentMessage = "You are already enrolled in this course.";
                            EnrollmentMessageClass = "alert-info";
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Course with ID {CourseId} not found or Consultant data missing.");
                    NoCourseFound = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading course details for ID {CourseId}: {ex.Message}");
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EnrollNow()
        {
            if (CourseData?.Course == null)
            {
                await ShowMessage("Error", "Cannot enroll. Invalid course information.", "alert-danger");
                return;
            }

            if (string.IsNullOrEmpty(_currentLoggedInUserId))
            {
                await ShowMessage("Authorization Required", "Please login to enroll in courses.", "alert-danger");
                // Optionally navigate to login page
                // NavigationService.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
                return;
            }

            IsEnrolling = true;
            EnrollmentMessage = string.Empty;
            // StateHasChanged() is not needed here as properties are bound and notify changes.

            try
            {
                // In WPF, we rely on the API to handle unique ID generation.
                // We'll create a new DTO instance for the request.
                var newEnrollment = new CourseEnrollDTO
                {
                    // EnrollId should ideally be generated by the API.
                    // If the DTO requires it, provide a new GUID, but API should handle uniqueness.
                    EnrollId = Guid.NewGuid().ToString(), // Placeholder, API should ideally generate this
                    MemberId = _currentLoggedInUserId,
                    CourseId = CourseData.Course.CourseId,
                    Status = "Enrolled",
                    EnrollDate = DateOnly.FromDateTime(DateTime.Today),
                    CompleteDate = null
                };

                // The API call to create the enrollment
                await _courseEnrollApiService.CreateAsync(newEnrollment);

                IsEnrolledSuccessfully = true;
                await ShowMessage("Enrollment Success", $"You have successfully enrolled in '{CourseData.Course.CourseName}'!", "alert-success");
                Console.WriteLine($"Successful enrollment in course {newEnrollment.CourseId} for member {newEnrollment.MemberId} with Enroll ID {newEnrollment.EnrollId}");

                await Task.Delay(2000); // Wait for 2 seconds
                GoToCourseContent(); // Navigate after successful enrollment
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enrolling in course: {ex.Message}");
                await ShowMessage("Enrollment Error", $"Failed to enroll in course: {ex.Message}", "alert-danger");
            }
            finally
            {
                IsEnrolling = false;
                // StateHasChanged() not needed, bindings will update.
            }
        }

        // Removed GenerateNextEnrollmentId() as it should be handled by the API.

        private async Task ShowMessage(string title, string message, string cssClass)
        {
            EnrollmentMessage = message;
            EnrollmentMessageClass = cssClass;
            Console.WriteLine($"{title}: {message}"); // Use Console.WriteLine instead of JSRuntime.InvokeVoidAsync("console.log")
            // StateHasChanged() not needed.
            await Task.Delay(5000); // Display message for 5 seconds
            EnrollmentMessage = string.Empty;
            // StateHasChanged() not needed.
        }

        private void GoBackToCourses()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack(); // Go back in navigation history
            }
            else
            {
                // Fallback if no history, navigate to a default courses page
                NavigationService.Navigate(new Uri("/Views/Courses.xaml", UriKind.Relative));
            }
            Console.WriteLine("Navigating back to Courses page.");
        }

        private void GoToCourseContent()
        {
            if (CourseData?.Course?.CourseId != null)
            {
                // Navigate to a hypothetical CourseContent page
                // Ensure you have a CourseContent.xaml page or similar
                NavigationService.Navigate(new Uri($"/Views/CourseContent.xaml?courseId={CourseData.Course.CourseId}", UriKind.Relative));
                Console.WriteLine($"Navigating to course content for Course ID: {CourseData.Course.CourseId}");
            }
            else
            {
                Console.WriteLine("Cannot navigate to course content: Course ID is missing.");
                // Log error, no MessageBox as per previous instructions
            }
        }

        private async Task NavigateToInstructor(UserDTO instructor)
        {
            if (instructor?.UserId != null)
            {
                // Example: Navigate to an InstructorProfile page
                NavigationService.Navigate(new Uri($"/Views/InstructorProfile.xaml?userId={instructor.UserId}", UriKind.Relative));
                Console.WriteLine($"Navigating to instructor profile for User ID: {instructor.UserId}");
            }
            else
            {
                Console.WriteLine("Cannot navigate to instructor profile: Instructor User ID is missing.");
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Inner class for data structure (matches Blazor's CoursePageData)
        public class CoursePageData : INotifyPropertyChanged
        {
            public CourseDTO? Course { get; set; }
            public string Description { get; set; } = string.Empty;
            public UserDTO? Instructor { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    // Helper class for ICommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;

        public void Execute(object? parameter) => _execute((T)parameter!);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
