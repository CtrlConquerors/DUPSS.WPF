using DUPSS.ApiClients;
using DUPSS.Common; // Now includes LoginRequest, IProtectedLocalStorage, and WpfSecureStorageService
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Data;

namespace DUPSS.WPF.Views
{
    // Converter to convert boolean to Visibility for UI elements
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
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

    public partial class Login : Page, INotifyPropertyChanged
    {
        private readonly AuthApiService _authApiService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        // Public properties for data binding
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasErrorMessage)); // Notify change for visibility
                }
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        private string? _message;
        public string? Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasMessage)); // Notify change for visibility
                }
            }
        }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public Login()
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext to this page for binding

            // Add the BooleanToVisibilityConverter to resources for use in XAML
            this.Resources.Add("BooleanToVisibilityConverter", new BooleanToVisibilityConverter());

            // Ensure App.HttpClient is initialized (from your App.xaml.cs or similar)
            if (App.HttpClient == null)
            {
                MessageBox.Show("App.HttpClient is not initialized. Cannot proceed with login.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Consider disabling login functionality or exiting
                return;
            }

            // Initialize AuthApiService
            _authApiService = new AuthApiService(App.HttpClient);

            // Initialize WpfSecureStorageService from DUPSS.Common
            var wpfSecureStorage = new WpfSecureStorageService();

            // Initialize JwtAuthenticationStateProvider with your WPF secure storage implementation
            _authStateProvider = new JwtAuthenticationStateProvider(_authApiService, wpfSecureStorage);
        }

        // Event handler for the Login button click
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage = null; // Clear previous errors
            Message = null; // Clear previous messages

            // Retrieve password directly from PasswordBox (WPF security best practice)
            string password = PasswordBox.Password;

            Console.WriteLine($"Login attempt: Email={Email}");

            try
            {
                // Call the authentication service
                await _authStateProvider.LoginAsync(Email, password);

                // If login is successful, show success message and trigger popup animation
                Message = "Login Successfully";
                // Get the Storyboard resource and begin the animation
                Storyboard fadeInStoryboard = (Storyboard)this.Resources["PopupFadeIn"];
                if (fadeInStoryboard != null)
                {
                    // Ensure the popup is visible before starting animation
                    SuccessPopupBoard.Visibility = Visibility.Visible;
                    fadeInStoryboard.Begin(SuccessPopupBoard);
                }

                await Task.Delay(2000); // Wait for 2 seconds

                // Navigate to the home page
                // Check if NavigationService is available (e.g., if hosted in a Frame)
                if (NavigationService.CanGoBack)
                {
                    // Navigate to the root URI. In a WPF NavigationWindow/Frame,
                    // this typically means navigating to the default page set in App.xaml
                    // or a specific page. Adjust the Uri as per your application's navigation setup.
                    NavigationService.Navigate(new Uri("/", UriKind.Relative));
                }
                else
                {
                    // Fallback for direct window or if NavigationService is not available
                    // You might need to set the MainWindow's content directly or restart.
                    MessageBox.Show("Login successful! Navigation service not available for direct navigation. Please restart or manually navigate.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Example if your main window is a NavigationWindow:
                    // ((NavigationWindow)Application.Current.MainWindow).Navigate(new Uri("/", UriKind.Relative));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = "Invalid email or password.";
                Console.WriteLine($"Login failed (Unauthorized): {ex.Message}");
                // Ensure error message is visible
                ErrorMessageBorder.Visibility = Visibility.Visible;
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                ErrorMessage = "Unable to reach the server. Please check your network connection or server status.";
                Console.WriteLine($"Login failed (HTTP Request): {ex.Message}");
                // Ensure error message is visible
                ErrorMessageBorder.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                Console.WriteLine($"Login failed (General Exception): {ex}");
                // Ensure error message is visible
                ErrorMessageBorder.Visibility = Visibility.Visible;
            }
        }

        // Event handler for Hyperlink navigation
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Navigate to the specified URI
            if (NavigationService.CanGoBack)
            {
                NavigationService.Navigate(e.Uri);
            }
            else
            {
                // Fallback for direct window or if NavigationService is not available
                MessageBox.Show($"Navigate to: {e.Uri.OriginalString}", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            e.Handled = true; // Mark the event as handled
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
