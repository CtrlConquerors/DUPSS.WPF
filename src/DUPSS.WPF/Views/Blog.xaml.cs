using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging; // Required for BitmapImage
using System.IO; // Required for Path.GetExtension
using DUPSS.DTO.DTOs;
using DUPSS.ApiClients; // Assuming you have a BlogApiService or similar

namespace DUPSS.WPF.Views
{
    public partial class Blog : Page
    {
        private List<BlogDTO> _allBlogs = new();
        private List<BlogDTO> _filteredBlogs = new();
        private List<BlogTopicDTO> _blogTopics = new();
        private const int DefaultBlogDisplayLimit = 3;
        private bool _showAllBlogs = false;

        // Define common image extensions to check
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        // Define a placeholder image URI for when a blog image is not found
        // NOW USING THE SAME PLACEHOLDER AS COURSES
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/placeholder.png");


        public Blog()
        {
            InitializeComponent();
            Loaded += Blog_Loaded;
        }

        private async void Blog_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if App.HttpClient is initialized
            if (App.HttpClient == null)
            {
                Debug.WriteLine("❌ App.HttpClient is null! Cannot load blogs.");
                ShowStatus("App.HttpClient is not initialized. Cannot load blogs.");
                return;
            }

            await LoadBlogTopicsAsync();
            await LoadBlogsAsync();
        }

        private async Task LoadBlogTopicsAsync()
        {
            try
            {
                var result = await App.HttpClient.GetFromJsonAsync<List<BlogTopicDTO>>("https://localhost:7026/api/BlogTopics/GetAll");
                _blogTopics = result ?? new List<BlogTopicDTO>();
                TopicFilter.ItemsSource = _blogTopics;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load blog topics: {ex.Message}");
                ShowStatus("Failed to load blog topics. Please check API endpoint.");
            }
        }

        private async Task LoadBlogsAsync()
        {
            try
            {
                ShowStatus("Loading blogs...");
                var result = await App.HttpClient.GetFromJsonAsync<List<BlogDTO>>("https://localhost:7026/api/Blogs/GetAll");
                _allBlogs = (result ?? new List<BlogDTO>()).Where(b => b.Status == "Published").ToList();
                SetBlogImageUris(); // Call method to set local image URIs
                HideStatus();
                ApplySearchAndDisplay(); // Call here, after blogs are loaded and images set
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error loading blogs: {ex.Message}");
                ShowStatus("Oops! Something went wrong while loading blogs. Please try again later.");
            }
        }

        // Method to set local image URIs for each blog
        private void SetBlogImageUris()
        {
            foreach (var blog in _allBlogs)
            {
                Uri imageUri = null;
                foreach (var ext in _imageExtensions)
                {
                    var potentialUriString = $"pack://application:,,,/DUPSS.WPF;component/Images/{blog.BlogId}{ext}";
                    try
                    {
                        var uri = new Uri(potentialUriString);
                        if (Application.GetResourceStream(uri) != null)
                        {
                            imageUri = uri;
                            break;
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
                    blog.ImageUrl = imageUri.ToString();
                }
                else
                {
                    blog.ImageUrl = _placeholderImageUri.ToString();
                    Debug.WriteLine($"No specific image found for BlogId: {blog.BlogId}. Using placeholder.");
                }
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
            MainScrollViewer.ScrollToHome();
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllBlogs = false;
            ApplySearchAndDisplay();
            MainScrollViewer.ScrollToHome();
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
