using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using DUPSS.ApiClients; // For AuthApiService
using DUPSS.Common; // For ForgotPasswordRequest and ForgotPasswordResponse
using DUPSS.WPF.Layouts; // For MainLayout (to update UI or navigate)

namespace DUPSS.WPF.Authentication // CHANGED: Namespace updated to match XAML's x:Class
{
    /// <summary>
    /// Interaction logic for ForgotPassword.xaml
    /// </summary>
    public partial class ForgotPassword : Page, INotifyPropertyChanged
    {
        private readonly AuthApiService _authApiService;

        // Properties for data binding
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
                    OnPropertyChanged(nameof(HasErrorMessage));
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
                    OnPropertyChanged(nameof(HasMessage));
                }
            }
        }
        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public ForgotPassword()
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext for data binding

            // Ensure App.HttpClient and App.AuthApiService are initialized
            if (App.HttpClient == null || App.AuthApiService == null)
            {
                MessageBox.Show("Application services are not initialized. Cannot proceed.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            _authApiService = App.AuthApiService;
        }

        /// <summary>
        /// Handles the click event for the "Send Reset Link" button.
        /// Initiates the forgot password process.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private async void SendResetLink_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage = null; // Clear previous error messages
            Message = null;      // Clear previous success messages

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please enter your email address.";
                return;
            }

            try
            {
                // Call the backend API service to request a password reset
                var response = await _authApiService.ForgotPasswordAsync(Email);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    Message = "Reset link sent successfully!";
                    // Trigger the popup animation
                    Storyboard fadeInStoryboard = (Storyboard)this.Resources["PopupFadeIn"];
                    if (fadeInStoryboard != null)
                    {
                        SuccessPopupBoard.Visibility = Visibility.Visible;
                        fadeInStoryboard.Begin(SuccessPopupBoard);
                    }

                    await Task.Delay(2000); // Wait for 2 seconds before redirecting

                    // Navigate to the Reset Password page, passing email and token
                    if (NavigationService != null)
                    {
                        // Ensure this path matches the actual location of ResetPassword.xaml
                        string uri = $"/Views/Authentication/ResetPassword.xaml?email={Uri.EscapeDataString(response.Email)}&token={Uri.EscapeDataString(response.Token)}";
                        NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                    }
                }
                else
                {
                    // If response is null or token is empty, it means email not found or other issue
                    ErrorMessage = "Email not found or an issue occurred. Please check your email and try again.";
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                ErrorMessage = "Unable to reach the server. Please check your network connection or server status.";
                Console.WriteLine($"Forgot password failed (HTTP Request): {ex.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                Console.WriteLine($"Forgot password failed (General Exception): {ex}");
            }
        }

        /// <summary>
        /// Handles navigation for Hyperlinks (e.g., "Log in").
        /// </summary>
        /// <param name="sender">The Hyperlink that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(e.Uri);
            }
            else
            {
                MessageBox.Show($"Cannot navigate to: {e.Uri.OriginalString}. Navigation service not available.", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
