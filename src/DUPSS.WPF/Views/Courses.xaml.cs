using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DUPSS.DTO.DTOs;

namespace DUPSS.WPF.Views
{
    /// <summary>
    /// Interaction logic for Courses.xaml
    /// </summary>
    public partial class Courses : Page
    {
        private List<CourseDTO> _allCourses = new();
        private List<CourseDTO> _filteredCourses = new();
        private const int DefaultCourseDisplayLimit = 3;
        private bool _showAllCourses = false;

        public Courses()
        {
            InitializeComponent();
            Loaded += Courses_Loaded;
        }

        private async void Courses_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                ShowStatus("Loading courses...");
                // Replace with your actual API endpoint or service
                using var http = new HttpClient();
                var result = await http.GetFromJsonAsync<List<CourseDTO>>("https://localhost:5001/api/Courses/GetAll");
                _allCourses = result ?? new List<CourseDTO>();
                HideStatus();
                ApplySearchAndDisplay();
            }
            catch (Exception ex)
            {
                ShowStatus("Oops! Something went wrong while loading courses. Please try again later.");
            }
        }

        private void ApplySearchAndDisplay()
        {
            string searchTerm = SearchBox?.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(searchTerm))
                _filteredCourses = _allCourses.ToList();
            else
                _filteredCourses = _allCourses.Where(c =>
                    (c.CourseName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Topic?.TopicName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Description?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.CourseId?.ToLower().Contains(searchTerm) ?? false)
                ).ToList();

            if (!_filteredCourses.Any())
            {
                ShowStatus("No courses found matching your criteria. Please try a different search term or filters.");
                CoursesGrid.ItemsSource = null;
                ExploreMoreButton.Visibility = ShowLessButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                HideStatus();
                var displayList = _showAllCourses ? _filteredCourses : _filteredCourses.Take(DefaultCourseDisplayLimit).ToList();
                CoursesGrid.ItemsSource = displayList;
                ExploreMoreButton.Visibility = (_filteredCourses.Count > DefaultCourseDisplayLimit && !_showAllCourses) ? Visibility.Visible : Visibility.Collapsed;
                ShowLessButton.Visibility = (_showAllCourses && _filteredCourses.Count > DefaultCourseDisplayLimit) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _showAllCourses = false;
            ApplySearchAndDisplay();
        }

        private void ExploreMoreButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllCourses = true;
            ApplySearchAndDisplay();
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllCourses = false;
            ApplySearchAndDisplay();
        }

        private void CourseCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is CourseDTO course)
            {
                // Replace with your navigation logic
                MessageBox.Show($"Navigate to detail for course: {course.CourseId}");
            }
        }

        private void ShowStatus(string message)
        {
            StatusPanel.Visibility = Visibility.Visible;
            StatusText.Text = message;
        }

        private void HideStatus()
        {
            StatusPanel.Visibility = Visibility.Collapsed;
        }
    }
}