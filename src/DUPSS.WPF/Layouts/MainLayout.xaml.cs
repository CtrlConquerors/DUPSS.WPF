using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation; // Required for NavigationService
using DUPSS.WPF.Views; // Required to reference Login and Home pages directly for initial navigation
using DUPSS.ApiClients; // For AuthApiService, UserApiService, JwtAuthenticationStateProvider
using DUPSS.Common; // For WpfSecureStorageService, UserDTO
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization; // For AuthenticationState
using System.Security.Claims; // For ClaimTypes

namespace DUPSS.WPF.Layouts
{
    /// <summary>
    /// Interaction logic for MainLayout.xaml
    /// </summary>
    public partial class MainLayout : Window
    {
        private bool _isSidebarExpanded = true;
        private DoubleAnimation _sidebarAnimation;

        // Dependency injected services (or accessed via App.cs static properties)
        private readonly AuthApiService _authApiService;
        private readonly UserApiService _userApiService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        public MainLayout()
        {
            InitializeComponent();
            this.Loaded += MainLayout_Loaded; // Attach the Loaded event handler

            // Ensure App.HttpClient is initialized (from your App.xaml.cs or similar)
            if (App.HttpClient == null)
            {
                MessageBox.Show("App.HttpClient is not initialized. Cannot proceed.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(); // Shut down the application if HttpClient is not available
                return;
            }

            // Initialize API services using App's static HttpClient
            _authApiService = App.AuthApiService; // Access static instance from App.xaml.cs
            _userApiService = App.UserApiService; // Access static instance from App.xaml.cs
            _authStateProvider = App.JwtAuthenticationStateProvider; // Access static instance from App.xaml.cs

            // Subscribe to authentication state changes to update UI
            _authStateProvider.AuthenticationStateChanged += AuthStateProvider_AuthenticationStateChanged;

            // Initialize sidebar animation for future use
            _sidebarAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        /// <summary>
        /// Handles the Loaded event for the MainLayout window.
        /// Performs initial UI updates and navigates to the appropriate starting page.
        /// </summary>
        private async void MainLayout_Loaded(object sender, RoutedEventArgs e)
        {
            // Perform initial UI update based on authentication state
            await UpdateUIForAuthStateAsync();

            // Always navigate to the Home page first, regardless of authentication status.
            // If authentication is required for certain features, it should be handled within Home.xaml.cs
            // or by redirecting from Home.xaml.cs if a protected resource is accessed.
            MainFrame.Navigate(new Uri("/Views/Home.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Handles authentication state changes and triggers UI updates.
        /// </summary>
        /// <param name="authenticationStateTask">The task representing the new authentication state.</param>
        private async void AuthStateProvider_AuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
        {
            await UpdateUIForAuthStateAsync();
        }

        /// <summary>
        /// Updates the visibility of login/logout buttons and the welcome message based on authentication state.
        /// Fetches username from the database if authenticated.
        /// </summary>
        public async Task UpdateUIForAuthStateAsync() // Made public for access from Login.xaml.cs
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // Ensure UI updates happen on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
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
                        // Fetch user details from the database using UserApiService
                        // Use .Result or await if necessary, but be careful with deadlocks
                        // For simplicity, directly accessing static App.UserApiService
                        _ = Task.Run(async () => // Run on a background thread to avoid blocking UI
                        {
                            try
                            {
                                var userDto = await _userApiService.GetByIdAsync(userId);
                                Application.Current.Dispatcher.Invoke(() => // Update UI on UI thread
                                {
                                    if (userDto != null)
                                    {
                                        WelcomeTextBlock.Text = $"Welcome, {userDto.Username}!";
                                    }
                                    else
                                    {
                                        WelcomeTextBlock.Text = "Welcome, Authenticated User!"; // Fallback if user not found in DB
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching user details in MainLayout: {ex.Message}");
                                Application.Current.Dispatcher.Invoke(() => // Update UI on UI thread
                                {
                                    WelcomeTextBlock.Text = "Welcome, User (Error)!"; // Show error if fetching fails
                                });
                            }
                        });
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
            });
        }

        /// <summary>
        /// Handles navigation clicks from the sidebar buttons.
        /// The Tag property of the button is used to determine the target page.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
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
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await _authStateProvider.Logout(); // Corrected: Changed from LogoutAsync() to Logout()
            MessageBox.Show("You have been logged out.", "Logout Successful", MessageBoxButton.OK, MessageBoxImage.Information);

            // Update UI visibility immediately after logout
            await UpdateUIForAuthStateAsync(); // Corrected: Changed to await UpdateUIForAuthStateAsync()

            // Navigate back to the login page after logout
            MainFrame.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Handles the Login button click, navigating to the Login page.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Handles the click event for the sidebar toggle button, collapsing or expanding the sidebar.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isSidebarExpanded = !_isSidebarExpanded;
            double from = SidebarColumn.Width.Value;
            double to = _isSidebarExpanded ? 250 : 50;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            AnimationClock clock = animation.CreateClock();
            clock.CurrentTimeInvalidated += (s, ev) =>
            {
                if (clock.CurrentProgress.HasValue)
                {
                    double current = from + (to - from) * clock.CurrentProgress.Value;
                    SidebarColumn.Width = new GridLength(current, GridUnitType.Pixel);
                }
            };
            clock.Completed += (s, ev) =>
            {
                SidebarColumn.Width = new GridLength(to, GridUnitType.Pixel);
            };

            // Start the animation
            clock.Controller.Begin();

            SidebarContentPanel.Visibility = _isSidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
            SidebarToggleButton.Content = _isSidebarExpanded ? "⮜" : "⮞";
            SidebarToggleButton.HorizontalAlignment = _isSidebarExpanded ? HorizontalAlignment.Right : HorizontalAlignment.Center;
        }
    }
}
