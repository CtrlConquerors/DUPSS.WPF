using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation; // Required for Frame navigation

using DUPSS.WPF.Views;

using DUPSS.WPF.Views; // For Login page
using DUPSS.ApiClients; // For AuthApiService, UserApiService, JwtAuthenticationStateProvider
using DUPSS.Common; // For WpfSecureStorageService, UserDTO
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization; // For AuthenticationState
using System.Security.Claims; // For ClaimTypes


namespace DUPSS.WPF.Layouts // Namespace matches the folder structure
{
    /// <summary>
    /// Interaction logic for MainLayout.xaml
    /// </summary>
    public partial class MainLayout : Window
    {

        private bool _sidebarCollapsed = false;

        private bool _sidebarCollapsed = false;

        private readonly AuthApiService _authApiService;
        private readonly UserApiService _userApiService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;


        public MainLayout()
        {
            InitializeComponent();
            this.Loaded += MainLayout_Loaded; // Attach event handler for when the window is loaded

            // Ensure App.HttpClient is initialized (from your App.xaml.cs or similar)
            if (App.HttpClient == null)
            {
                MessageBox.Show("App.HttpClient is not initialized. Cannot proceed.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(); // Shut down the application if HttpClient is not available
                return;
            }

            // Initialize API services
            _authApiService = new AuthApiService(App.HttpClient);
            _userApiService = new UserApiService(App.HttpClient);

            // Initialize secure storage and authentication state provider
            // WpfSecureStorageService needs to be available in DUPSS.Common or a referenced project
            var wpfSecureStorage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(_authApiService, wpfSecureStorage);

            // Subscribe to authentication state changes
            _authStateProvider.AuthenticationStateChanged += AuthStateProvider_AuthenticationStateChanged;

            // Initial UI update based on current state (don't await in constructor directly)
            _ = UpdateUIForAuthStateAsync();
        }

        private async void MainLayout_Loaded(object sender, RoutedEventArgs e)
        {
            // Perform initial UI update based on authentication state
            await UpdateUIForAuthStateAsync();

            // Navigate to the appropriate page based on authentication status
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                NavigateToPage("Home"); // Navigate to Home if already logged in
            }
            else
            {
                MainFrame.Navigate(new Uri("/Views/Home.xaml", UriKind.Relative)); // Navigate to Login if not logged in
            }
        }

        /// <summary>
        /// Handles authentication state changes and updates the UI accordingly.
        /// </summary>
        private async void AuthStateProvider_AuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
        {
            await UpdateUIForAuthStateAsync();
        }

        /// <summary>
        /// Updates the visibility of login/logout buttons and the welcome message.
        /// Fetches username from the database if authenticated.
        /// </summary>
        private async Task UpdateUIForAuthStateAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                // User is logged in
                LoginButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;
                WelcomeTextBlock.Visibility = Visibility.Visible;

                // Attempt to get UserId from claims
                string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        // Fetch user details from the database using UserApiService
                        var userDto = await _userApiService.GetByIdAsync(userId);
                        if (userDto != null)
                        {
                            WelcomeTextBlock.Text = $"Welcome, {userDto.Username}!";
                        }
                        else
                        {
                            WelcomeTextBlock.Text = "Welcome, Authenticated User!"; // Fallback if user not found in DB
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching user details in MainLayout: {ex.Message}");
                        WelcomeTextBlock.Text = "Welcome, User (Error)!"; // Show error if fetching fails
                    }
                }
                else
                {
                    WelcomeTextBlock.Text = "Welcome, Authenticated User!"; // Fallback if UserId claim is missing
                }
            }
            else
            {
                // User is logged out
                LoginButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                WelcomeTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles navigation clicks from the sidebar buttons.
        /// The Tag property of the button is used to determine the target page.
        /// </summary>
        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                NavigateToPage(pageName);
            }
        }

        /// <summary>
        /// Navigates the MainFrame to the specified page.
        /// </summary>
        /// <param name="pageName">The name of the page (e.g., "Home", "Appointments").</param>
        private void NavigateToPage(string pageName)
        {
            // Construct the URI for the page.
            // Assuming your pages (like Home.xaml) are in a "Views" folder.
            // Example: "Views/Home.xaml"
            string uriString = $"/Views/{pageName}.xaml";

            try
            {
                Uri pageUri = new Uri(uriString, UriKind.Relative);
                MainFrame.Navigate(pageUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to {pageName}: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Logout button click.
        /// </summary>
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await _authStateProvider.Logout(); // Perform logout
            // Navigate back to the login page after logout
            MainFrame.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
            MessageBox.Show("You have been logged out.", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        /// <summary>
        /// Handles the Login button click, navigating to the Login page.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Handles the click event for the sidebar toggle button.
        /// Collapses or expands the sidebar.
        /// </summary>

        private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _sidebarCollapsed = !_sidebarCollapsed;
            if (_sidebarCollapsed)
            {
                SidebarColumn.Width = new GridLength(40); // Thin bar

                SidebarContentPanel.Visibility = Visibility.Collapsed; // Ensure SidebarContentPanel is defined in XAML
                SidebarToggleButton.Content = "⮞";

                SidebarContentPanel.Visibility = Visibility.Collapsed;
                SidebarToggleButton.Content = "⮞"; // Right arrow

                SidebarToggleButton.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                SidebarColumn.Width = new GridLength(250);

                SidebarContentPanel.Visibility = Visibility.Visible; // Ensure SidebarContentPanel is defined in XAML
                SidebarToggleButton.Content = "⮜";
                SidebarToggleButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }

                SidebarContentPanel.Visibility = Visibility.Visible;
                SidebarToggleButton.Content = "⮜"; // Left arrow
                SidebarToggleButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }

    }
}
