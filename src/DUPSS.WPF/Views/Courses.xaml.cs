using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging; // Required for BitmapImage
using System.IO; // Required for Path.GetExtension

namespace DUPSS.WPF.Views
{
    public partial class Courses : Page
    {
        private readonly CourseApiService _courseService;
        private List<CourseDTO> allCourses = new List<CourseDTO>();
        private int displayLimit = 3;
        private bool isShowingAll = false;

        // Define common image extensions to check
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        // Define a placeholder image URI for when a course image is not found
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/Courses/placeholder.png");

        public Courses()
        {
            InitializeComponent();

            if (App.HttpClient == null)
            {
                Debug.WriteLine("❌ App.HttpClient is null!");
                MessageBox.Show("App.HttpClient is not initialized. Cannot load courses.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _courseService = new CourseApiService(App.HttpClient);
            LoadCoursesAsync();
        }

        private async void LoadCoursesAsync()
        {
            try
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusText.Text = "Loading courses...";

                var courses = await _courseService.GetAllAsync();

                if (courses != null && courses.Count > 0)
                {
                    allCourses = courses;
                    SetCourseImageUris(); // Call method to set local image URIs
                    Debug.WriteLine($"✅ Loaded {courses.Count} courses. Setting image URIs.");
                    FilterCourses();
                }
                else
                {
                    allCourses = new List<CourseDTO>(); // Ensure it's not null
                    StatusPanel.Visibility = Visibility.Visible;
                    StatusText.Text = "No courses available.";
                    Debug.WriteLine("⚠️ No courses returned from API.");
                }
            }
            catch (Exception ex)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusText.Text = $"Error loading courses: {ex.Message}";
                Debug.WriteLine($"❌ Error loading courses: {ex.Message}");
                // In a real application, you might log the full exception details
            }
        }

        // Method to set local image URIs for each course
        private void SetCourseImageUris()
        {
            foreach (var course in allCourses)
            {
                Uri imageUri = null;
                // Try to find an image with any of the specified extensions
                foreach (var ext in _imageExtensions)
                {
                    // Construct the pack URI for the image
                    var potentialUriString = $"pack://application:,,,/DUPSS.WPF;component/Images/Courses/{course.CourseId}{ext}";
                    try
                    {
                        var uri = new Uri(potentialUriString);
                        // Check if the resource stream exists for the URI
                        if (Application.GetResourceStream(uri) != null)
                        {
                            imageUri = uri;
                            break; // Found a valid image, stop checking extensions
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

                if (imageUri != null)
                {
                    course.ImageUrl = imageUri.ToString();
                }
                else
                {
                    course.ImageUrl = _placeholderImageUri.ToString();
                    Debug.WriteLine($"No specific image found for CourseId: {course.CourseId}. Using placeholder.");
                }
            }
        }

        private void FilterCourses()
        {
            var searchText = SearchBox.Text.ToLowerInvariant();
            IEnumerable<CourseDTO> filtered;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filtered = allCourses;
            }
            else
            {
                filtered = allCourses.Where(c =>
                    c.CourseName.ToLowerInvariant().Contains(searchText) ||
                    (c.Description != null && c.Description.ToLowerInvariant().Contains(searchText)) ||
                    (c.Topic?.TopicName != null && c.Topic.TopicName.ToLowerInvariant().Contains(searchText))
                ).ToList();
            }

            // Apply display limit if not showing all
            if (!isShowingAll && filtered.Count() > displayLimit)
            {
                CoursesGrid.ItemsSource = filtered.Take(displayLimit).ToList();
                ExploreMoreButton.Visibility = Visibility.Visible;
                ShowLessButton.Visibility = Visibility.Collapsed; // Only show Explore More
            }
            else
            {
                CoursesGrid.ItemsSource = filtered.ToList();
                ExploreMoreButton.Visibility = Visibility.Collapsed;
                // Show "Show Less" only if there are more courses than the initial display limit AND we are currently showing all
                ShowLessButton.Visibility = (allCourses.Count > displayLimit && isShowingAll) ? Visibility.Visible : Visibility.Collapsed;
            }

            // Show message if no results
            if (!filtered.Any())
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusText.Text = "No matching courses found.";
            }
            else
            {
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("🔍 Search changed: " + SearchBox.Text);
            FilterCourses();
        }

        private void ExploreMoreButton_Click(object sender, RoutedEventArgs e)
        {
            isShowingAll = true;
            FilterCourses();
            // Scroll to the top of the ScrollViewer after expanding
            MainScrollViewer.ScrollToHome(); // <--- NEW: Scroll to top
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            isShowingAll = false;
            FilterCourses();
            // Scroll to the top of the ScrollViewer after collapsing
            MainScrollViewer.ScrollToHome(); // <--- NEW: Scroll to top
        }

        private void CourseCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var course = border?.DataContext as CourseDTO;
            if (course != null)
            {
                Debug.WriteLine($"📌 Clicked course: {course.CourseName}");
                // TODO: Navigate to detail page if needed
            }
        }
    }
}
