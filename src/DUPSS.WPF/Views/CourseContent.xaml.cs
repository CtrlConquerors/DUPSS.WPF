using DUPSS.ApiClients;
using DUPSS.Common; // For IProtectedLocalStorage, etc.
using DUPSS.DTO.DTOs;
using Microsoft.AspNetCore.Components.Authorization; // For AuthenticationStateProvider
using System;
using System.Collections.Generic;
using System.ComponentModel; // For INotifyPropertyChanged
using System.Linq;
using System.Runtime.CompilerServices; // For CallerMemberName
using System.Security.Claims; // For ClaimTypes.NameIdentifier
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // For IValueConverter
using System.Windows.Input; // For ICommand
using System.Windows.Navigation; // For NavigationService
using System.IO; // For Path.Combine, File.Exists
using System.Diagnostics; // For Debug.WriteLine
using System.Windows.Threading; // For DispatcherTimer
using System.Windows.Media; // Required for VisualTreeHelper

namespace DUPSS.WPF.Views
{
    public partial class CourseContent : Page, INotifyPropertyChanged
    {
        // Injected Services (from App.xaml.cs static properties)
        private readonly CourseApiService _courseApiService;
        private readonly CourseEnrollApiService _courseEnrollApiService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        // Data-bound properties for UI state
        private CourseDTO? _course;
        public CourseDTO? Course
        {
            get => _course;
            set
            {
                _course = value;
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
                OnPropertyChanged(nameof(ShowCourseContent));
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
                OnPropertyChanged(nameof(ShowCourseContent));
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
                OnPropertyChanged(nameof(ShowCourseContent));
            }
        }

        // Calculated property to control visibility of main content
        public bool ShowCourseContent => Course != null && !IsLoading && !HasError && !NoCourseFound;

        private List<CourseModule> _modules = new List<CourseModule>();
        public List<CourseModule> Modules
        {
            get => _modules;
            set
            {
                _modules = value;
                OnPropertyChanged();
            }
        }

        private string? _currentLoggedInUserId;
        private CourseEnrollDTO? _currentEnrollment;
        public CourseEnrollDTO? CurrentEnrollment
        {
            get => _currentEnrollment;
            set
            {
                _currentEnrollment = value;
                OnPropertyChanged();
            }
        }

        private bool _isCompletingCourse = false;
        public bool IsCompletingCourse
        {
            get => _isCompletingCourse;
            set
            {
                _isCompletingCourse = value;
                OnPropertyChanged();
            }
        }

        private string _completionMessage = string.Empty;
        public string CompletionMessage
        {
            get => _completionMessage;
            set
            {
                _completionMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCompletionMessage));
            }
        }

        private string _completionMessageClass = string.Empty;
        public string CompletionMessageClass
        {
            get => _completionMessageClass;
            set
            {
                _completionMessageClass = value;
                OnPropertyChanged();
            }
        }

        public bool HasCompletionMessage => !string.IsNullOrEmpty(CompletionMessage);

        // Commands
        public ICommand ToggleModuleCommand { get; private set; }
        public ICommand ToggleLearningObjectivesCommand { get; private set; }
        public ICommand ToggleLessonContentCommand { get; private set; }
        public ICommand CompleteCourseCommand { get; private set; }
        public ICommand GoBackToCoursesCommand { get; private set; }

        // Parameter for CourseId (set by navigation from Courses.xaml.cs)
        public string CourseId { get; set; } = string.Empty;

        // Supported image extensions for local resources
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/Courses/placeholder.png");

        // MediaElement related properties
        // Removed _mediaTimer as it's no longer needed for updating slider/time
        // Removed CurrentTimeText and TotalTimeText properties

        // Constructor with Dependency Injection
        public CourseContent(
            CourseApiService courseApiService,
            CourseEnrollApiService courseEnrollApiService,
            JwtAuthenticationStateProvider authStateProvider)
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext for bindings

            _courseApiService = courseApiService;
            _courseEnrollApiService = courseEnrollApiService;
            _authStateProvider = authStateProvider;

            // Initialize Commands
            ToggleModuleCommand = new RelayCommand<CourseModule>(module => ToggleModule(module));
            ToggleLearningObjectivesCommand = new RelayCommand<CourseModule>(module => ToggleLearningObjectives(module));
            ToggleLessonContentCommand = new RelayCommand<CourseLesson>(lesson => ToggleLessonContent(lesson));
            CompleteCourseCommand = new RelayCommand(async () => await CompleteCourse(), () => !IsCompletingCourse && CurrentEnrollment?.Status != "Completed");
            GoBackToCoursesCommand = new RelayCommand(() => GoBackToCourses());

            // Removed _mediaTimer initialization as it's no longer needed
        }

        // Page Loaded event handler (equivalent to Blazor's OnInitializedAsync or OnParametersSetAsync)
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCourseContent();
        }

        private async Task LoadCourseContent()
        {
            IsLoading = true;
            HasError = false;
            NoCourseFound = false;
            CompletionMessage = string.Empty;
            IsCompletingCourse = false;
            Course = null;
            Modules = new List<CourseModule>(); // Clear modules
            CurrentEnrollment = null;

            if (string.IsNullOrWhiteSpace(CourseId))
            {
                NoCourseFound = true;
                IsLoading = false;
                Debug.WriteLine("CourseId is null or empty. Cannot load course content.");
                return;
            }

            try
            {
                // Get current logged-in user ID
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                _currentLoggedInUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Fetch course details
                var fetchedCourse = await _courseApiService.GetByIdAsync(CourseId);

                if (fetchedCourse == null)
                {
                    NoCourseFound = true;
                    Debug.WriteLine($"Course with ID {CourseId} not found.");
                }
                else
                {
                    Course = fetchedCourse;

                    // Resolve Course Image URL for WPF (pack URI)
                    // Ensure ImageUrl is always set to a valid URI, even if it's the placeholder
                    Course.ImageUrl = GetPackUriForImage(
                        $"Images/Courses/{Course.CourseId}",
                        _imageExtensions,
                        _placeholderImageUri
                    ).ToString();
                    Debug.WriteLine($"Course Image URL set to: {Course.ImageUrl}");

                    // Check if the current user is enrolled in this course
                    if (!string.IsNullOrEmpty(_currentLoggedInUserId))
                    {
                        var enrollments = await _courseEnrollApiService.GetEnrollmentsByMemberAndCourse(_currentLoggedInUserId, CourseId);
                        CurrentEnrollment = enrollments?.FirstOrDefault(); // Get the specific enrollment if it exists
                    }

                    PopulateMockModules(Course.CourseId); // Populate modules based on courseId

                    // Expand the first module by default
                    if (Modules.Any())
                    {
                        Modules.First().IsExpanded = true;
                        OnPropertyChanged(nameof(Modules)); // Notify UI if first module expanded
                    }

                    // If the course is completed, mark all modules and lessons as complete
                    if (CurrentEnrollment != null && CurrentEnrollment.Status == "Completed")
                    {
                        foreach (var module in Modules)
                        {
                            module.IsComplete = true;
                            foreach (var lesson in module.Lessons)
                            {
                                lesson.IsComplete = true;
                            }
                        }
                        OnPropertyChanged(nameof(Modules)); // Notify UI of changes
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading course content for ID {CourseId}: {ex.Message}");
                HasError = true;
            }
            finally
            {
                IsLoading = false;
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


        private void ToggleModule(CourseModule module)
        {
            module.IsExpanded = !module.IsExpanded;
            OnPropertyChanged(nameof(Modules)); // Notify UI of change
        }

        private void ToggleLearningObjectives(CourseModule module)
        {
            module.AreObjectivesExpanded = !module.AreObjectivesExpanded;
            OnPropertyChanged(nameof(Modules)); // Notify UI of change
        }

        // Method to toggle the visibility of lesson content for "Reading" types and video players for "Video" types
        private void ToggleLessonContent(CourseLesson lesson)
        {
            // Only toggle if it's a Reading with content or a Video with a URL
            if ((lesson.Type == "Reading" && !string.IsNullOrEmpty(lesson.Content)) ||
                (lesson.Type == "Video" && !string.IsNullOrEmpty(lesson.VideoUrl)))
            {
                // Collapse any currently expanded lesson content first
                foreach (var module in Modules)
                {
                    foreach (var l in module.Lessons)
                    {
                        if (l != lesson && l.IsContentExpanded)
                        {
                            l.IsContentExpanded = false;
                            // Stop any playing video when collapsing other lessons
                            // Access the MediaElement via the AssociatedMediaElement property of the lesson
                            if (l.Type == "Video" && l.AssociatedMediaElement != null && l.AssociatedMediaElement.Source != null)
                            {
                                l.AssociatedMediaElement.Stop();
                            }
                        }
                    }
                }

                lesson.IsContentExpanded = !lesson.IsContentExpanded;

                // Handle MediaElement specific actions for the currently toggled lesson
                if (lesson.Type == "Video" && lesson.AssociatedMediaElement != null)
                {
                    if (lesson.IsContentExpanded)
                    {
                        // Set source and play when expanded
                        // The StringToUriConverter in XAML will handle the null check for VideoUrl
                        lesson.AssociatedMediaElement.Source = new Uri(lesson.VideoUrl!, UriKind.RelativeOrAbsolute);
                        lesson.AssociatedMediaElement.Play();
                        // Removed _mediaTimer.Start()
                    }
                    else
                    {
                        // Stop and clear source when collapsed
                        lesson.AssociatedMediaElement.Stop();
                        lesson.AssociatedMediaElement.Source = null;
                        // Removed _mediaTimer.Stop()
                        // Removed slider and time text resets
                    }
                }
                OnPropertyChanged(nameof(Modules)); // Notify UI of change
            }
        }

        // Event handlers for MediaElement
        // These handlers need to determine which MediaElement triggered the event.
        private void LessonMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            MediaElement? mediaElement = sender as MediaElement;
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                // Removed logic to update position slider and time text
            }
        }

        private void LessonMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement? mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                mediaElement.Stop();
                // Removed _mediaTimer.Stop()
                // Removed logic to reset position slider and time text
            }
        }

        private void LessonMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MediaElement? mediaElement = sender as MediaElement;
            Debug.WriteLine($"Media failed for {mediaElement?.Source}: {e.ErrorException.Message}");
            ShowCompletionMessage("Error", $"Failed to play video: {e.ErrorException.Message}", "alert-danger");
        }

        // Removed MediaTimer_Tick as it's no longer needed for updating slider/time
        /*
        private void MediaTimer_Tick(object? sender, EventArgs e)
        {
            // This timer is global. It needs to update the *currently playing* video's controls.
            foreach (var module in Modules)
            {
                foreach (var lesson in module.Lessons)
                {
                    if (lesson.IsContentExpanded && lesson.Type == "Video" && lesson.AssociatedMediaElement != null && lesson.AssociatedMediaElement.Source != null)
                    {
                        MediaElement currentMediaElement = lesson.AssociatedMediaElement;
                        if (currentMediaElement.NaturalDuration.HasTimeSpan)
                        {
                            DependencyObject? parentStackPanel = VisualTreeHelper.GetParent(currentMediaElement);
                            if (parentStackPanel != null)
                            {
                                Slider? positionSlider = this.FindChild<Slider>(parentStackPanel, "PositionSlider");
                                TextBlock? currentTimeText = this.FindChild<TextBlock>(parentStackPanel, "CurrentTimeText");

                                if (positionSlider != null)
                                {
                                    // Prevent infinite loop by checking if the slider value is already close to the media position
                                    if (Math.Abs(positionSlider.Value - currentMediaElement.Position.TotalSeconds) > 0.5)
                                    {
                                        positionSlider.Value = currentMediaElement.Position.TotalSeconds;
                                    }
                                }
                                if (currentTimeText != null)
                                {
                                    currentTimeText.Text = currentMediaElement.Position.ToString(@"mm\:ss");
                                }
                            }
                        }
                        break; // Only one video should be expanded and playing at a time
                    }
                }
            }
        }
        */

        // Playback control button clicks
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            CourseLesson? lesson = button?.DataContext as CourseLesson;
            if (lesson?.AssociatedMediaElement != null)
            {
                lesson.AssociatedMediaElement.Play();
                // Removed _mediaTimer.Start()
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            CourseLesson? lesson = button?.DataContext as CourseLesson;
            if (lesson?.AssociatedMediaElement != null)
            {
                lesson.AssociatedMediaElement.Pause();
                // Removed _mediaTimer.Stop()
            }
        }

        // Removed StopButton_Click
        /*
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            CourseLesson? lesson = button?.DataContext as CourseLesson;
            if (lesson?.AssociatedMediaElement != null)
            {
                lesson.AssociatedMediaElement.Stop();
                _mediaTimer.Stop();
                // Reset controls for this specific media element
                DependencyObject? parentStackPanel = VisualTreeHelper.GetParent(lesson.AssociatedMediaElement);
                if (parentStackPanel != null)
                {
                    Slider? positionSlider = this.FindChild<Slider>(parentStackPanel, "PositionSlider");
                    TextBlock? currentTimeText = this.FindChild<TextBlock>(parentStackPanel, "CurrentTimeText");
                    if (positionSlider != null) positionSlider.Value = 0;
                    if (currentTimeText != null) currentTimeText.Text = "00:00";
                }
            }
        }
        */

        // Removed PositionSlider_ValueChanged
        /*
        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider? slider = sender as Slider;
            CourseLesson? lesson = slider?.DataContext as CourseLesson;
            if (lesson?.AssociatedMediaElement != null && lesson.AssociatedMediaElement.NaturalDuration.HasTimeSpan)
            {
                // Only seek if the change was user-initiated (not by the timer)
                if (slider != null && !slider.IsMouseOver && Math.Abs(slider.Value - lesson.AssociatedMediaElement.Position.TotalSeconds) > 0.5)
                {
                    lesson.AssociatedMediaElement.Position = TimeSpan.FromSeconds(slider.Value);
                }
            }
        }
        */

        // Volume slider changed
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider? slider = sender as Slider;
            CourseLesson? lesson = slider?.DataContext as CourseLesson;
            if (lesson?.AssociatedMediaElement != null)
            {
                lesson.AssociatedMediaElement.Volume = slider?.Value ?? 0.5; // Default to 0.5 if slider is null
            }
        }

        // New: Loaded event for MediaElement to associate it with the lesson
        private void LessonMediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            MediaElement? mediaElement = sender as MediaElement;
            CourseLesson? lesson = mediaElement?.DataContext as CourseLesson;
            if (lesson != null && mediaElement != null)
            {
                lesson.AssociatedMediaElement = mediaElement;
            }
        }

        // New: Unloaded event for MediaElement to clear the association
        private void LessonMediaElement_Unloaded(object sender, RoutedEventArgs e)
        {
            MediaElement? mediaElement = sender as MediaElement;
            CourseLesson? lesson = mediaElement?.DataContext as CourseLesson;
            if (lesson != null)
            {
                lesson.AssociatedMediaElement = null;
            }
        }


        private async Task CompleteCourse()
        {
            if (CurrentEnrollment == null || string.IsNullOrEmpty(_currentLoggedInUserId))
            {
                await ShowCompletionMessage("Error", "You are not enrolled in this course or not logged in.", "alert-danger");
                return;
            }

            if (CurrentEnrollment.Status == "Completed")
            {
                await ShowCompletionMessage("Info", "This course is already marked as completed.", "alert-info");
                return;
            }

            IsCompletingCourse = true;
            CompletionMessage = string.Empty;

            try
            {
                // Update the enrollment DTO
                CurrentEnrollment.Status = "Completed";
                CurrentEnrollment.CompleteDate = DateOnly.FromDateTime(DateTime.Today);
                Debug.WriteLine("Complete Date: " + CurrentEnrollment.CompleteDate);

                // Call the API to update the enrollment
                await _courseEnrollApiService.UpdateAsync(CurrentEnrollment);

                // Mark all modules and lessons as complete in the UI
                foreach (var module in Modules)
                {
                    module.IsComplete = true;
                    // Optionally collapse objectives and modules after completion
                    module.AreObjectivesExpanded = false;
                    module.IsExpanded = false; // Collapse modules if desired after completion
                    foreach (var lesson in module.Lessons)
                    {
                        lesson.IsComplete = true;
                        lesson.IsContentExpanded = false; // Collapse reading/video content on course completion
                        // Stop any playing video when course is completed
                        if (lesson.Type == "Video" && lesson.AssociatedMediaElement != null && lesson.AssociatedMediaElement.Source != null)
                        {
                            lesson.AssociatedMediaElement.Stop();
                        }
                    }
                }
                OnPropertyChanged(nameof(Modules)); // Notify UI of changes

                await ShowCompletionMessage("Success", $"Course '{Course?.CourseName}' successfully marked as completed!", "alert-success");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error completing course: {ex.Message}");
                await ShowCompletionMessage("Error", $"Failed to mark course as complete: {ex.Message}", "alert-danger");
            }
            finally
            {
                IsCompletingCourse = false;
            }
        }

        private async Task ShowCompletionMessage(string title, string message, string cssClass)
        {
            CompletionMessage = message;
            CompletionMessageClass = cssClass;
            Debug.WriteLine($"{title}: {message}");
            await Task.Delay(5000); // Display message for 5 seconds
            CompletionMessage = string.Empty;
        }

        private void GoBackToCourses()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Fallback if no history, navigate to a default courses page
                NavigationService.Navigate(new Uri("/Views/Courses.xaml", UriKind.Relative));
            }
            Debug.WriteLine("Navigating back to Courses page.");
        }

        // Helper method to find a child control by name within a DependencyObject's visual tree
        private T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and child name are valid.
            if (parent == null) return null;

            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the desired type, skip it.
                if (child is not T typedChild) continue;

                if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = typedChild;
                        break;
                    }
                }
                else
                {
                    // If no name is specified, return the first found child of the correct type.
                    foundChild = typedChild;
                    break;
                }

                // Recurse into children of this child in the visual tree.
                foundChild = this.FindChild<T>(child, childName);
                if (foundChild != null) break;
            }
            return foundChild;
        }


        // Mock data population (copied directly from your Blazor component)
        private void PopulateMockModules(string courseId)
        {
            if (courseId == "C0001")
            {
                Modules.Add(new CourseModule
                {
                    Title = "Module 1: Embracing a Sober & Social Life", // Changed title
                    Description = "In this module, you will be introduced to the course's program and how to stay sober. You are expected to spend 2 to 3 hours on this module's workload.",
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Understand the core principles of sober living.",
                        "Identify personal motivations for a substance-free lifestyle.",
                        "Set foundational goals for social engagement without substances."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        new CourseLesson { Title = "Your Journey to Sober Fun", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "Welcome to the exciting world of drug rehabilitation! This course is designed to guide you through a journey of recovery and self-discovery. We believe in your potential to overcome challenges and build a fulfilling life free from addiction. Throughout the modules, you will find resources, insights, and exercises to support your path to sobriety. Remember, you are not alone in this journey, and support is always available." }, // Changed title
                        new CourseLesson { Title = "Navigating Your Sober Path", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "The course syllabus provides a detailed overview of the curriculum, learning objectives, and assessment methods. It outlines the topics covered in each module, recommended readings, and important dates. Familiarizing yourself with the syllabus will help you plan your studies effectively and stay on track with your progress. If you have any questions, feel free to reach out to your instructor or support team." }, // Changed title
                        new CourseLesson { Title = "Share Your Sober & Social Goals", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "To provide you with the best possible learning experience, we encourage you to share more about yourself. Your insights and experiences can help us tailor the content to better meet your needs. Please take a moment to complete the brief survey, which will allow us to understand your background, learning preferences, and any specific areas where you might need additional support. Your responses are confidential and will only be used to enhance your learning journey." } // Changed title
                    }
                });
                Modules.Add(new CourseModule
                {
                    Title = "Module 2: Building Healthy Social Connections", // Changed title to fit theme
                    Description = "This module explores the dynamics of social interactions, offering insights into fostering meaningful relationships and navigating social environments without the need for substances.", // Changed description
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Recognize the impact of past substance use on social dynamics.",
                        "Identify healthy vs. unhealthy social relationships.",
                        "Develop strategies for setting boundaries in social settings.",
                        "Explore the benefits of a supportive sober community."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        new CourseLesson { Title = "Understanding Social Dynamics & Triggers", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "In this lesson, we delve into the fundamental aspects of social dynamics and how they can be impacted by a history of substance use. You will learn to identify common social triggers that might lead to cravings or discomfort, and begin to understand how to approach these situations with a sober mindset. This foundational knowledge is crucial for building resilient social connections." }, // Changed title and content
                        new CourseLesson { Title = "Cultivating Positive Relationships", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "This lesson focuses on practical strategies for building and maintaining positive, supportive relationships that align with your sober lifestyle. We'll explore techniques for clear communication, active listening, and setting healthy boundaries to ensure your social circle contributes positively to your recovery journey. Learn how to identify and nurture connections that uplift you." }, // Changed title and content
                        new CourseLesson { Title = "Navigating Social Events Soberly", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "This lesson provides actionable advice for attending social gatherings and events without feeling pressured to use substances. You'll learn coping mechanisms for uncomfortable situations, how to politely decline offers, and strategies for finding enjoyment in social settings purely on your own terms. Prepare to confidently embrace sober social fun!" } // Changed title and content
                    }
                });
                Modules.Add(new CourseModule
                {
                    Title = "Module 3: Practicing New Social Skills", // Changed title
                    Description = "This module focuses on practical application, guiding you through exercises and real-world scenarios to build confidence and competence in sober social interactions.",
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Practice effective communication in sober social settings.",
                        "Learn techniques for declining substances gracefully.",
                        "Develop coping mechanisms for social anxiety without relying on substances."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        // MODIFIED: Changed VideoUrl to a local path
                        new CourseLesson { Title = "Lesson 3.1: Communicating Confidently", Type = "Video", DurationText = "12 min", IsComplete = false, VideoUrl = "Videos/Communication.mp4" },
                        new CourseLesson { Title = "Lesson 3.2: Role-Playing Social Situations", Type = "Reading", DurationText = "8 min", IsComplete = false, Content = "In this lesson, you'll engage in various role-playing exercises designed to simulate real-life social scenarios. This hands-on approach will help you practice responding to peer pressure, initiating conversations, and handling awkward moments, all while maintaining your sobriety. The goal is to build muscle memory for positive social interactions, making it easier to navigate social events with confidence and ease." },
                    }
                });
                Modules.Add(new CourseModule
                {
                    Title = "Module 4: Evaluating Your Social Progress", // Changed title
                    Description = "This module focuses on evaluating your progress in navigating social situations without substances. You'll learn to track your successes, identify challenges, and adjust your strategies for continued growth.",
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Assess personal growth in sober social interactions.",
                        "Identify triggers and high-risk social situations.",
                        "Develop strategies for continuous self-assessment and improvement."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        new CourseLesson { Title = "Lesson 4.1: Setting Social Goals", Type = "Reading", DurationText = "7 min", IsComplete = false, Content = "This lesson delves into the importance of setting clear and achievable social goals in your sober journey. You'll learn how to define what 'fun' and 'social' mean to you without substances, create actionable steps, and develop a roadmap for expanding your sober social circle." },
                        // MODIFIED: Changed VideoUrl to a local path
                        new CourseLesson { Title = "Lesson 4.2: Reflecting on Social Encounters", Type = "Video", DurationText = "18 min", IsComplete = false, VideoUrl = "Videos/Reflect.mp4" },
                    }
                });
                Modules.Add(new CourseModule
                {
                    Title = "Module 5: Sustaining Your Sober & Social Life",
                    Description = "This module equips you with essential strategies and resources for long-term sobriety, helping you build and maintain a thriving social life free from substance dependence.",
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Build and maintain a strong sober support network.",
                        "Discover new, enjoyable sober activities and hobbies.",
                        "Plan for long-term sobriety and social well-being."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        new CourseLesson { Title = "Lesson 5.1: Building a Support Network", Type = "Reading", DurationText = "10 min", IsComplete = false, Content = "This lesson will guide you through the process of building and nurturing a strong sober support network. You'll learn how to identify individuals who genuinely support your recovery, communicate your boundaries effectively, and leverage these connections to enhance your social well-being." },
                        // MODIFIED: Changed VideoUrl to a local path
                        new CourseLesson { Title = "Lesson 5.2: Finding Sober Activities & Hobbies", Type = "Video", DurationText = "15 min", IsComplete = false, VideoUrl = "Videos/Sober.mp4" },
                    }
                });
            }
            else // Generic content for other courses
            {
                Modules.Add(new CourseModule
                {
                    Title = "Module 1: Laying the Foundations for Sober Fun",
                    Description = "This module provides the foundational knowledge required for the course.",
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Define key concepts of sobriety and social well-being.",
                        "Understand the importance of a substance-free lifestyle for social health."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        // MODIFIED: Changed VideoUrl to a local path
                        new CourseLesson { Title = "Introduction to Sober Living", Type = "Video", DurationText = "8 min", IsComplete = false, VideoUrl = "Videos/Intro.mp4" },
                        new CourseLesson { Title = "Understanding the Benefits of Sobriety", Type = "Reading", DurationText = "20 min", IsComplete = false, Content = "This core reading delves into the fundamental principles of the subject matter. It's designed to give you a solid understanding of the essential concepts that will be built upon in subsequent modules. Take your time to absorb the information and feel free to revisit sections as needed. A strong foundation here will greatly benefit your learning journey." }
                    }
                });
                Modules.Add(new CourseModule
                {
                    Title = "Module 2: Applying Sober Strategies",
                    Description = "Apply the concepts learned in Module 1 to practical scenarios.",
                    IsComplete = false,
                    LearningObjectives = new List<string> {
                        "Apply practical strategies for navigating social events soberly.",
                        "Evaluate personal progress in maintaining a sober and social life."
                    },
                    Lessons = new List<CourseLesson>
                    {
                        // MODIFIED: Changed VideoUrl to a local path
                        new CourseLesson { Title = "Navigating Social Scenarios", Type = "Video", DurationText = "12 min", IsComplete = false, VideoUrl = "Videos/Walkthrough.mp4" }
                    }
                });
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Helper classes for data binding and commands
        public class CourseModule : INotifyPropertyChanged
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            private bool _isExpanded = false;
            public bool IsExpanded
            {
                get => _isExpanded;
                set
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }

            private bool _isComplete = false;
            public bool IsComplete
            {
                get => _isComplete;
                set
                {
                    _isComplete = value;
                    OnPropertyChanged();
                }
            }

            public List<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
            public List<string> LearningObjectives { get; set; } = new List<string>();

            private bool _areObjectivesExpanded = false;
            public bool AreObjectivesExpanded
            {
                get => _areObjectivesExpanded;
                set
                {
                    _areObjectivesExpanded = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public class CourseLesson : INotifyPropertyChanged
        {
            public string Title { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string DurationText { get; set; } = string.Empty;

            private bool _isComplete = false;
            public bool IsComplete
            {
                get => _isComplete;
                set
                {
                    _isComplete = value;
                    OnPropertyChanged();
                }
            }

            public string? Content { get; set; }
            public string? VideoUrl { get; set; } // Now stores local path

            private bool _isContentExpanded = false;
            public bool IsContentExpanded
            {
                get => _isContentExpanded;
                set
                {
                    _isContentExpanded = value;
                    OnPropertyChanged();
                }
            }

            // New property to hold the reference to the MediaElement in the UI
            public MediaElement? AssociatedMediaElement { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        // Helper class for ICommand implementation (copied from CourseDetailPage.xaml.cs)
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

        // Custom Converters
        // Note: StringToUriConverter is now assumed to be in CommonConverters.cs or a similar shared file.
        public class StringToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is string s && !string.IsNullOrEmpty(s))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class BooleanToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool b && b)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class InverseBooleanToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool b && !b)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class BooleanToChevronConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool b && b)
                {
                    return "▼"; // Down arrow for expanded
                }
                return "▶"; // Right arrow for collapsed
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
