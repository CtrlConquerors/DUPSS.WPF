using DUPSS.ApiClients;
using DUPSS.Common;
using DUPSS.DTO.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DUPSS.WPF.Views
{
    public partial class CampaignDetail : Page
    {
        private readonly CampaignDTO _campaign;
        private readonly CampaignRegistrationApiService _registrationService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        private string? _currentUserId;

        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/Campaigns/placeholder.png");

        public CampaignDetail(CampaignDTO campaign)
        {
            InitializeComponent();
            _campaign = campaign;
            _registrationService = new CampaignRegistrationApiService(App.HttpClient);

            var wpfSecureStorage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(new AuthApiService(App.HttpClient), wpfSecureStorage);

            Loaded += CampaignDetail_Loaded;
        }

        private async void CampaignDetail_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserIdAsync();
            PopulateCampaignDetails();
            LoadImage();
        }

        private async Task LoadUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                _currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        private void PopulateCampaignDetails()
        {
            TitleTextBlock.Text = _campaign.Title;
            StatusTextBlock.Text = _campaign.Status?.ToString() ?? "-";
            LocationTextBlock.Text = _campaign.Location ?? "-";
            IntroductionTextBlock.Text = _campaign.Introduction ?? "-";
            DescriptionTextBlock.Text = _campaign.Description ?? "-";
        }

        private void LoadImage()
        {
            Uri imageUri = null;

            foreach (var ext in _imageExtensions)
            {
                var potentialUri = $"pack://application:,,,/DUPSS.WPF;component/Images/Campaigns/{_campaign.CampaignId}{ext}";
                try
                {
                    var uri = new Uri(potentialUri);
                    if (Application.GetResourceStream(uri) != null)
                    {
                        imageUri = uri;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Image check failed: {ex.Message}");
                }
            }

            CampaignImage.Source = new BitmapImage(imageUri ?? _placeholderImageUri);
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                MessageBox.Show("❌ You must log in to register.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dto = new CampaignRegistrationDTO
                {
                    RegistrationId = Guid.NewGuid().ToString(),
                    MemberId = _currentUserId,
                    CampaignId = _campaign.CampaignId,
                    RegisteredAt = DateTime.UtcNow
                };

                bool success = await _registrationService.RegisterAsync(dto);

                if (success)
                {
                    MessageBox.Show("✅ Registered successfully!", "Success");
                }
                else
                {
                    MessageBox.Show("❌ Failed to register (maybe already registered).", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error");
            }
        }
    }
}
