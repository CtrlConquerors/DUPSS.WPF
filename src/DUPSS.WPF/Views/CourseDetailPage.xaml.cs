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
using System.Windows.Media.Imaging; // Added for BitmapImage
using System.IO; // Added for Path.GetExtension
using System.Diagnostics; // Added for Debug.WriteLine
using DUPSS.WPF; // Added to access App.static properties

namespace DUPSS.WPF.Views
{
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
                OnPropertyChanged(nameof(ShowCourseContent)); // Ensure ShowCourseContent updates
                Debug.WriteLine($"CourseData SET. Course is null: {(_courseData?.Course == null)}. Instructor is null: {(_courseData?.Instructor == null)}");
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
                OnPropertyChanged(nameof(ShowCourseContent)); // <--- ADDED: Notify ShowCourseContent
                Debug.WriteLine($"IsLoading SET to: {_isLoading}");
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
                OnPropertyChanged(nameof(ShowCourseContent)); // <--- ADDED: Notify ShowCourseContent
                Debug.WriteLine($"HasError SET to: {_hasError}");
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
                OnPropertyChanged(nameof(ShowCourseContent)); // <--- ADDED: Notify ShowCourseContent
                Debug.WriteLine($"NoCourseFound SET to: {_noCourseFound}");
            }
        }

        public bool ShowCourseContent
        {
            get
            {
                var show = CourseData?.Course != null && !IsLoading && !HasError && !NoCourseFound;
                Debug.WriteLine($"ShowCourseContent GET: CourseData.Course null={CourseData?.Course == null}, !IsLoading={!IsLoading}, !HasError={!HasError}, !NoCourseFound={!NoCourseFound} => Result={show}");
                return show;
            }
        }

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

        // Define common image extensions to check (reused from Courses.xaml.cs)
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        // Define a placeholder image URI for when an image is not found
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/Courses/placeholder.png"); // Assuming a generic placeholder

        // Constructor with Dependency Injection
        public CourseDetailPage(
            CourseApiService courseApiService,
            CourseEnrollApiService courseEnrollApiService,
            UserApiService userApiService,
            JwtAuthenticationStateProvider authStateProvider)
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext for bindings

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
            Debug.WriteLine($"Page_Loaded fired. CourseId: {CourseId}");
            await LoadCourseDataAsync();
        }

        private async Task LoadCourseDataAsync()
        {
            IsLoading = true; // This will trigger Debug.WriteLine for IsLoading
            HasError = false;
            NoCourseFound = false;
            EnrollmentMessage = string.Empty;
            IsEnrolledSuccessfully = false;
            CourseData = null; // Clear previous data, this will trigger Debug.WriteLine for CourseData

            if (string.IsNullOrWhiteSpace(CourseId))
            {
                NoCourseFound = true; // This will trigger Debug.WriteLine for NoCourseFound
                IsLoading = false;    // This will trigger Debug.WriteLine for IsLoading
                Debug.WriteLine("CourseId is null or empty. Cannot load course details. Displaying NoCourseFound state.");
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
                        Debug.WriteLine("Warning: Authenticated user has no NameIdentifier claim for UserId.");
                    }
                }

                var fetchedCourseTask = _courseApiService.GetByIdAsync(CourseId);
                var allEnrollmentsTask = _courseEnrollApiService.GetAllAsync();

                await Task.WhenAll(fetchedCourseTask, allEnrollmentsTask);

                var fetchedCourse = fetchedCourseTask.Result;
                _allEnrollments = allEnrollmentsTask.Result;


                if (fetchedCourse != null && fetchedCourse.Consultant != null)
                {
                    // Construct pack URIs for embedded resources
                    fetchedCourse.ImageUrl = GetPackUriForImage(
                        $"Images/Courses/{fetchedCourse.CourseId}",
                        _imageExtensions,
                        _placeholderImageUri
                    ).ToString();
                    Debug.WriteLine($"Course Image URL set to: {fetchedCourse.ImageUrl}");


                    // For Instructor Image
                    fetchedCourse.Consultant.ImageUrl = GetPackUriForImage(
                        $"Images/Users/{fetchedCourse.Consultant.UserId}",
                        _imageExtensions,
                        _placeholderImageUri
                    ).ToString();
                    Debug.WriteLine($"Instructor Image URL set to: {fetchedCourse.Consultant.ImageUrl}");

                    CourseData = new CoursePageData // This will trigger Debug.WriteLine for CourseData
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
                    Debug.WriteLine($"Course with ID {CourseId} not found or Consultant data missing. Setting NoCourseFound to true.");
                    NoCourseFound = true; // This will trigger Debug.WriteLine for NoCourseFound
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading course details for ID {CourseId}: {ex.Message}");
                HasError = true; // This will trigger Debug.WriteLine for HasError
            }
            finally
            {
                IsLoading = false; // This will trigger Debug.WriteLine for IsLoading
                Debug.WriteLine($"--- LoadCourseDataAsync FINISHED ---");
                Debug.WriteLine($"Final state: IsLoading={IsLoading}, HasError={HasError}, NoCourseFound={NoCourseFound}, CourseData.Course is null={CourseData?.Course == null}, ShowCourseContent={ShowCourseContent}");
                Debug.WriteLine($"------------------------------------");
            }
        }

        /// <summary>
        /// Helper method to construct a pack URI for an embedded image resource.
        /// </summary>
        /// <param name="basePath">The base path within the component (e.g., "Images/Courses/C0001").</param>
        /// <param name="extensions">Array of possible file extensions (e.g., ".jpg", ".png").</param>
        /// <param name="placeholderUri">The URI to use if no specific image is found.</param>
        /// <returns>A Uri object for the image resource.</returns>
        private Uri GetPackUriForImage(string basePath, string[] extensions, Uri placeholderUri)
        {
            foreach (var ext in extensions)
            {
                var potentialUriString = $"pack://application:,,,/DUPSS.WPF;component/{basePath}{ext}";
                try
                {
                    var uri = new Uri(potentialUriString);
                    if (Application.GetResourceStream(uri) != null)
                    {
                        return uri; // Found a valid image
                    }
                }
                catch (UriFormatException)
                {
                    Debug.WriteLine($"Invalid URI format for: {potentialUriString}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking resource stream for {potentialUriString}: {ex.Message}");
                }
            }
            Debug.WriteLine($"No specific image found for base path: {basePath}. Using placeholder.");
            return placeholderUri; // Return placeholder if no image found
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

            try
            {
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

                await _courseEnrollApiService.CreateAsync(newEnrollment);

                IsEnrolledSuccessfully = true;
                await ShowMessage("Enrollment Success", $"You have successfully enrolled in '{CourseData.Course.CourseName}'!", "alert-success");
                Debug.WriteLine($"Successful enrollment in course {newEnrollment.CourseId} for member {newEnrollment.MemberId} with Enroll ID {newEnrollment.EnrollId}");

                await Task.Delay(2000); // Wait for 2 seconds
                GoToCourseContent(); // Navigate after successful enrollment
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error enrolling in course: {ex.Message}");
                await ShowMessage("Enrollment Error", $"Failed to enroll in course: {ex.Message}", "alert-danger");
            }
            finally
            {
                IsEnrolling = false;
            }
        }

        private async Task ShowMessage(string title, string message, string cssClass)
        {
            EnrollmentMessage = message;
            EnrollmentMessageClass = cssClass;
            Debug.WriteLine($"{title}: {message}");
            await Task.Delay(5000); // Display message for 5 seconds
            EnrollmentMessage = string.Empty;
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
            Debug.WriteLine("Navigating back to Courses page.");
        }

        private void GoToCourseContent()
        {
            if (CourseData?.Course?.CourseId != null)
            {
                // Navigate to the new CourseContent page, passing necessary services and CourseId
                var courseContentPage = new CourseContent(
                    App.CourseApiService,
                    App.CourseEnrollApiService,
                    App.JwtAuthenticationStateProvider
                )
                {
                    CourseId = CourseData.Course.CourseId
                };
                NavigationService.Navigate(courseContentPage);
                Debug.WriteLine($"Navigating to course content for Course ID: {CourseData.Course.CourseId}");
            }
            else
            {
                Debug.WriteLine("Cannot navigate to course content: Course ID is missing.");
            }
        }

        private async Task NavigateToInstructor(UserDTO instructor)
        {
            if (instructor?.UserId != null)
            {
                // Example: Navigate to an InstructorProfile page
                NavigationService.Navigate(new Uri($"/Views/InstructorProfile.xaml?userId={instructor.UserId}", UriKind.Relative));
                Debug.WriteLine($"Navigating to instructor profile for User ID: {instructor.UserId}");
            }
            else
            {
                Debug.WriteLine("Cannot navigate to instructor profile: Instructor User ID is missing.");
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
