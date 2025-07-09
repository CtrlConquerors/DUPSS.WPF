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
    public partial class Campaign : Page
    {
        private List<CampaignDTO> _allCampaigns = new();
        private List<CampaignDTO> _filteredCampaigns = new();
        private const int DefaultCampaignDisplayLimit = 3;
        private bool _showAllCampaigns = false;

        public Campaign()
        {
            InitializeComponent();
            Loaded += Campaign_Loaded;
        }

        private async void Campaign_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCampaignsAsync();
        }

        private async Task LoadCampaignsAsync()
        {
            try
            {
                ShowStatus("Loading campaigns...");
                using var http = new HttpClient();
                var result = await http.GetFromJsonAsync<List<CampaignDTO>>("https://localhost:7026/api/Campaigns/GetAll");
                _allCampaigns = result ?? new List<CampaignDTO>();
                HideStatus();
                ApplyDisplay();
            }
            catch
            {
                ShowStatus("Oops! Something went wrong while loading campaigns. Please try again later.");
            }
        }

        private void ApplyDisplay()
        {
            _filteredCampaigns = _allCampaigns.ToList();

            if (!_filteredCampaigns.Any())
            {
                ShowStatus("No campaign available at the moment. Please check back later!");
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
        }

        private void ShowLessButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllCampaigns = false;
            ApplyDisplay();
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