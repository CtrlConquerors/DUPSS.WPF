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
    public partial class Blog : Page
    {
        private List<BlogDTO> _allBlogs = new();
        private List<BlogDTO> _filteredBlogs = new();
        private List<BlogTopicDTO> _blogTopics = new();
        private const int DefaultBlogDisplayLimit = 3;
        private bool _showAllBlogs = false;

        public Blog()
        {
            InitializeComponent();
            Loaded += Blog_Loaded;
        }

        private async void Blog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBlogTopicsAsync();
            await LoadBlogsAsync();
            // Do NOT call ApplySearchAndDisplay here!
        }

        private async Task LoadBlogTopicsAsync()
        {
            try
            {
                using var http = new HttpClient();
                var result = await http.GetFromJsonAsync<List<BlogTopicDTO>>("https://localhost:7026/api/Blogs/GetAll");
                _blogTopics = result ?? new List<BlogTopicDTO>();
                TopicFilter.ItemsSource = _blogTopics;
            }
            catch
            {
                ShowStatus("Failed to load blog topics.");
            }
        }

        private async Task LoadBlogsAsync()
        {
            try
            {
                ShowStatus("Loading blogs...");
                using var http = new HttpClient();
                var result = await http.GetFromJsonAsync<List<BlogDTO>>("https://localhost:7026/api/Blogs/GetAll");
                _allBlogs = (result ?? new List<BlogDTO>()).Where(b => b.Status == "Published").ToList();
                HideStatus();
                ApplySearchAndDisplay(); // Only call here, after blogs are loaded
            }
            catch
            {
                ShowStatus("Oops! Something went wrong while loading blogs. Please try again later.");
            }
        }

        private void ApplySearchAndDisplay()
        {
            string searchTerm = SearchBox?.Text?.Trim().ToLower() ?? "";
            IEnumerable<BlogDTO> query = _allBlogs;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b =>
                    (b.Title?.ToLower().Contains(searchTerm) ?? false) ||
                    (b.BlogTopic?.BlogTopicName?.ToLower().Contains(searchTerm) ?? false) ||
                    (b.Content?.ToLower().Contains(searchTerm) ?? false) ||
                    (b.BlogId?.ToLower().Contains(searchTerm) ?? false)
                );
            }

            if (TopicFilter.SelectedItem is BlogTopicDTO selectedTopic)
            {
                query = query.Where(b => b.BlogTopic?.BlogTopicId == selectedTopic.BlogTopicId);
            }

            _filteredBlogs = query.ToList();

            if (!_filteredBlogs.Any())
            {
                ShowStatus("No blogs found matching your criteria. Please try a different search term or filters.");
                BlogGrid.ItemsSource = null;
                ExploreMoreButton.Visibility = ShowLessButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                HideStatus();
                var displayList = _showAllBlogs ? _filteredBlogs : _filteredBlogs.Take(DefaultBlogDisplayLimit).ToList();
                BlogGrid.ItemsSource = displayList;
                ExploreMoreButton.Visibility = (_filteredBlogs.Count > DefaultBlogDisplayLimit && !_showAllBlogs) ? Visibility.Visible : Visibility.Collapsed;
                ShowLessButton.Visibility = (_showAllBlogs && _filteredBlogs.Count > DefaultBlogDisplayLimit) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _showAllBlogs = false;
            ApplySearchAndDisplay();
        }

        private void TopicFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _showAllBlogs = false;
            ApplySearchAndDisplay();
        }

        private void BlogCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is BlogDTO blog)
            {
                MessageBox.Show($"Navigate to detail for blog: {blog.BlogId}");
            }
        }

        private void ExploreMoreButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllBlogs = true;
            ApplySearchAndDisplay();
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllBlogs = false;
            ApplySearchAndDisplay();
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