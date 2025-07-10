using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DUPSS.WPF.Views
{
    public partial class Courses : Page
    {
        private readonly CourseApiService _courseService;
        private List<CourseDTO> allCourses = new List<CourseDTO>();
        private int displayLimit = 3;
        private bool isShowingAll = false;

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
                    Debug.WriteLine($"✅ Loaded {courses.Count} courses.");
                    FilterCourses();
                    StatusPanel.Visibility = Visibility.Collapsed;

                    // Hiện nút explore nếu số lượng > displayLimit
                    if (allCourses.Count > displayLimit)
                        ExploreMoreButton.Visibility = Visibility.Visible;
                }
                else
                {
                    StatusText.Text = "No courses found.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Exception: {ex}");
                StatusText.Text = "Failed to load courses.";
                MessageBox.Show($"Failed to load courses.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterCourses()
        {
            string keyword = SearchBox.Text?.Trim().ToLower();
            IEnumerable<CourseDTO> filtered = allCourses;

            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = filtered.Where(c =>
                    (c.CourseName != null && c.CourseName.ToLower().Contains(keyword)) ||
                    (c.Topic?.TopicName != null && c.Topic.TopicName.ToLower().Contains(keyword)) ||
                    (c.Description != null && c.Description.ToLower().Contains(keyword))
                );
            }

            if (!isShowingAll)
            {
                filtered = filtered.Take(displayLimit);
            }

            CoursesGrid.ItemsSource = filtered.ToList();

            // Hiện nút explore / show less
            if (string.IsNullOrEmpty(keyword) && allCourses.Count > displayLimit)
            {
                ExploreMoreButton.Visibility = isShowingAll ? Visibility.Collapsed : Visibility.Visible;
                ShowLessButton.Visibility = isShowingAll ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                ExploreMoreButton.Visibility = Visibility.Collapsed;
                ShowLessButton.Visibility = Visibility.Collapsed;
            }

            // Hiện thông báo nếu không có kết quả
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
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            isShowingAll = false;
            FilterCourses();
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
