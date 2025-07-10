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
using DUPSS.ApiClients; // Assuming you have a CampaignApiService or similar

namespace DUPSS.WPF.Views
{
    public partial class Campaign : Page
    {
        // Assuming you have a CampaignApiService similar to CourseApiService
        // For now, I'll use HttpClient directly but encourage refactoring to an ApiService.
        private List<CampaignDTO> _allCampaigns = new();
        private List<CampaignDTO> _filteredCampaigns = new();
        private const int DefaultCampaignDisplayLimit = 3;
        private bool _showAllCampaigns = false;

        // Define common image extensions to check
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        // Define the shared placeholder image URI
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/Campaigns/placeholder.png");

        public Campaign()
        {
            InitializeComponent();
            Loaded += Campaign_Loaded;
        }

        private async void Campaign_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if App.HttpClient is initialized
            if (App.HttpClient == null)
            {
                Debug.WriteLine("❌ App.HttpClient is null! Cannot load campaigns.");
                MessageBox.Show("App.HttpClient is not initialized. Cannot load campaigns.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await LoadCampaignsAsync();
        }

        private async Task LoadCampaignsAsync()
        {
            try
            {
                ShowStatus("Loading campaigns...");
                // Use App.HttpClient for consistency
                var result = await App.HttpClient.GetFromJsonAsync<List<CampaignDTO>>("https://localhost:7026/api/Campaigns/GetAll");
                _allCampaigns = result ?? new List<CampaignDTO>();
                SetCampaignImageUris(); // NEW: Call method to set local image URIs
                HideStatus();
                ApplyDisplay();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error loading campaigns: {ex.Message}");
                ShowStatus("Oops! Something went wrong while loading campaigns. Please try again later.");
            }
        }

        // NEW: Method to set local image URIs for each campaign
        private void SetCampaignImageUris()
        {
            foreach (var campaign in _allCampaigns)
            {
                Uri imageUri = null;
                // Try to find an image with any of the specified extensions
                foreach (var ext in _imageExtensions)
                {
                    // Construct the pack URI for the image
                    // Campaign ID starts with "CAM" and 4 decimal numbers, e.g., CAM0001
                    var potentialUriString = $"pack://application:,,,/DUPSS.WPF;component/Images/Campaigns/{campaign.CampaignId}{ext}";
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
                        // Log invalid URI format but continue trying other extensions
                        Debug.WriteLine($"Invalid URI format for: {potentialUriString}");
                    }
                    catch (Exception ex)
                    {
                        // Catch other potential errors during resource stream check
                        Debug.WriteLine($"Error checking resource stream for {potentialUriString}: {ex.Message}");
                    }
                }

                if (imageUri != null)
                {
                    // Set the ImageUrl property of the DTO to the found pack URI string
                    campaign.ImageUrl = imageUri.ToString();
                }
                else
                {
                    // If no specific image is found, use the placeholder image
                    campaign.ImageUrl = _placeholderImageUri.ToString();
                    Debug.WriteLine($"No specific image found for CampaignId: {campaign.CampaignId}. Using placeholder.");
                }
            }
        }

        private void ApplyDisplay()
        {
            string searchTerm = SearchBox?.Text?.Trim().ToLower() ?? "";
            IEnumerable<CampaignDTO> query = _allCampaigns;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    (c.Title?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Description?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Location?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Introduction?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.CampaignId?.ToLower().Contains(searchTerm) ?? false)
                );
            }

            _filteredCampaigns = query.ToList();

            if (!_filteredCampaigns.Any())
            {
                ShowStatus("No campaigns available at the moment. Please check back later!");
                CampaignGrid.ItemsSource = null;
                ExploreMoreButton.Visibility = ShowLessButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                HideStatus();
                var displayList = _showAllCampaigns ? _filteredCampaigns : _filteredCampaigns.Take(DefaultCampaignDisplayLimit).ToList();
                CampaignGrid.ItemsSource = displayList;
                ExploreMoreButton.Visibility = (_filteredCampaigns.Count > DefaultCampaignDisplayLimit && !_showAllCampaigns) ? Visibility.Visible : Visibility.Collapsed;
                ShowLessButton.Visibility = (_showAllCampaigns && _filteredCampaigns.Count > DefaultCampaignDisplayLimit) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // NEW: Search Box Text Changed Event Handler
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _showAllCampaigns = false; // Reset to limited view on new search
            ApplyDisplay();
        }

        private void CampaignCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is CampaignDTO campaign)
            {
                MessageBox.Show($"Navigate to detail for campaign: {campaign.CampaignId}");
            }
        }

        private void ExploreMoreButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllCampaigns = true;
            ApplyDisplay();
            // NEW: Scroll to the top of the ScrollViewer after expanding
            MainScrollViewer.ScrollToHome();
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllCampaigns = false;
            ApplyDisplay();
            // NEW: Scroll to the top of the ScrollViewer after collapsing
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
