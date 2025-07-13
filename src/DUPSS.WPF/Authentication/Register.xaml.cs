using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text.RegularExpressions; // For email and phone number validation
using System.Windows.Media.Animation; // Required for Storyboard
using DUPSS.ApiClients; // For AuthApiService
using DUPSS.Common; // For CreateUserRequest (will be removed from usage)
using DUPSS.DTO.DTOs; // For UserDTO

namespace DUPSS.WPF.Authentication // Namespace updated to match XAML
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page, INotifyPropertyChanged
    {
        private readonly AuthApiService _authApiService;

        // Properties for data binding
        private UserDTO _newUser = new UserDTO { UserId = Guid.NewGuid().ToString(), Username = "", Email = "", RoleId = "ME" }; // Initialize with default RoleId
        public UserDTO NewUser
        {
            get => _newUser;
            set
            {
                if (_newUser != value)
                {
                    _newUser = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
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

        public Register()
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

            // Handle pre-filling email from query parameter if needed (Blazor concept)
            // In WPF, you'd typically pass this via constructor or a public method if navigating from another page
            // For now, assuming no query parameter handling is needed directly on load for WPF
        }

        /// <summary>
        /// Handles the click event for the "Sign Up" button.
        /// Initiates the user registration process.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage = null; // Clear previous error messages
            Message = null;      // Clear previous success messages

            // Retrieve password from PasswordBox (WPF security best practice)
            // And assign it directly to the NewUser DTO's Password property
            NewUser.Password = PasswordBox.Password; // Assign password directly to NewUser

            // Client-side validation
            if (string.IsNullOrWhiteSpace(NewUser.Username))
            {
                ErrorMessage = "Username is required.";
                return;
            }
            if (string.IsNullOrWhiteSpace(NewUser.Email))
            {
                ErrorMessage = "Email is required.";
                return;
            }
            if (!IsValidEmail(NewUser.Email))
            {
                ErrorMessage = "Invalid email format.";
                return;
            }
            if (string.IsNullOrWhiteSpace(NewUser.Password)) // Check NewUser.Password now
            {
                ErrorMessage = "Password is required.";
                return;
            }

            // Phone Number Validation: Must be exactly 10 digits and cannot be null/empty
            if (string.IsNullOrWhiteSpace(NewUser.PhoneNumber))
            {
                ErrorMessage = "Phone number is required.";
                return;
            }
            if (!IsValidPhoneNumber(NewUser.PhoneNumber))
            {
                ErrorMessage = "Phone number must be exactly 10 digits.";
                return;
            }

            // Date of Birth Validation: Cannot be null and cannot be in the future
            if (!NewUser.DoB.HasValue)
            {
                ErrorMessage = "Date of Birth is required.";
                return;
            }
            if (NewUser.DoB.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                ErrorMessage = "Date of Birth cannot be in the future.";
                return;
            }


            try
            {
                Console.WriteLine($"Registering user: {NewUser.Username}, Email: {NewUser.Email}, RoleId: {NewUser.RoleId}");

                // Call the backend API service to register the user with the populated UserDTO
                // The CreateUserRequest object is no longer needed if AuthApiService.RegisterAsync takes UserDTO directly
                var createdUser = await _authApiService.RegisterAsync(NewUser); // Pass NewUser directly

                if (createdUser != null)
                {
                    Message = "Register Successfully!";
                    // Trigger the popup animation (if defined in XAML)
                    Storyboard fadeInStoryboard = (Storyboard)this.Resources["PopupFadeIn"];
                    if (fadeInStoryboard != null)
                    {
                        SuccessPopupBoard.Visibility = Visibility.Visible;
                        fadeInStoryboard.Begin(SuccessPopupBoard);
                    }

                    await Task.Delay(2000); // Wait for 2 seconds before redirecting

                    // Navigate to the Login page
                    if (NavigationService != null)
                    {
                        NavigationService.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
                    }
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                ErrorMessage = "Unable to reach the server. Please check your network connection or server status.";
                Console.WriteLine($"Registration failed (HTTP Request): {ex.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                Console.WriteLine($"Registration failed (General Exception): {ex}");
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

        // Helper methods for client-side validation
        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
                RegexOptions.IgnoreCase
            );
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Regex for exactly 10 digits
            return Regex.IsMatch(
                phoneNumber,
                @"^\d{10}$"
            );
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
