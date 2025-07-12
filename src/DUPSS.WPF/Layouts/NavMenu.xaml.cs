using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // Required for Binding
using System.Windows.Media; // Required for VisualBrush, etc.
using System.Windows.Media.Effects; // Required for DropShadowEffect
using System.Windows.Media.Animation; // Required for Storyboard

namespace DUPSS.WPF.Layouts
{
    /// <summary>
    /// Interaction logic for NavMenu.xaml
    /// </summary>
    public partial class NavMenu : UserControl
    {
        // Event to notify the parent (MainLayout) to navigate
        public event EventHandler<string> NavigateRequested;

        // Dependency Property for IsAuthenticated (simulating user login state)
        public static readonly DependencyProperty IsAuthenticatedProperty =
            DependencyProperty.Register("IsAuthenticated", typeof(bool), typeof(NavMenu), new PropertyMetadata(false, OnIsAuthenticatedChanged));

        public bool IsAuthenticated
        {
            get { return (bool)GetValue(IsAuthenticatedProperty); }
            set { SetValue(IsAuthenticatedProperty, value); }
        }

        // Dependency Property for UserName
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(string), typeof(NavMenu), new PropertyMetadata("Guest User"));

        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        // Dependency Property for UserRole (simulating user roles for authorization)
        public static readonly DependencyProperty UserRoleProperty =
            DependencyProperty.Register("UserRole", typeof(string), typeof(NavMenu), new PropertyMetadata("Guest", OnUserRoleChanged));

        public string UserRole
        {
            get { return (string)GetValue(UserRoleProperty); }
            set { SetValue(UserRoleProperty, value); }
        }

        // Dependency Property for IsOverlayOpen (for the avatar popup)
        public static readonly DependencyProperty IsOverlayOpenProperty =
            DependencyProperty.Register("IsOverlayOpen", typeof(bool), typeof(NavMenu), new PropertyMetadata(false));

        public bool IsOverlayOpen
        {
            get { return (bool)GetValue(IsOverlayOpenProperty); }
            set { SetValue(IsOverlayOpenProperty, value); }
        }

        // Properties to control visibility of menu items based on roles
        public static readonly DependencyProperty IsManagementVisibleProperty =
            DependencyProperty.Register("IsManagementVisible", typeof(bool), typeof(NavMenu), new PropertyMetadata(false));
        public bool IsManagementVisible
        {
            get { return (bool)GetValue(IsManagementVisibleProperty); }
            set { SetValue(IsManagementVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsAppointmentVisibleProperty =
            DependencyProperty.Register("IsAppointmentVisible", typeof(bool), typeof(NavMenu), new PropertyMetadata(false));
        public bool IsAppointmentVisible
        {
            get { return (bool)GetValue(IsAppointmentVisibleProperty); }
            set { SetValue(IsAppointmentVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsConsultantDashboardVisibleProperty =
            DependencyProperty.Register("IsConsultantDashboardVisible", typeof(bool), typeof(NavMenu), new PropertyMetadata(false));
        public bool IsConsultantDashboardVisible
        {
            get { return (bool)GetValue(IsConsultantDashboardVisibleProperty); }
            set { SetValue(IsConsultantDashboardVisibleProperty, value); }
        }


        private TextBox SearchTextBox;

        public NavMenu()
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext to itself for easy binding to DependencyProperties

            // Initialize SearchTextBox by finding it in the XAML
            SearchTextBox = (TextBox)FindName("SearchTextBox");

            // Add converters to resources if not already in App.xaml
            if (!this.Resources.Contains("BooleanToVisibilityConverter"))
            {
                this.Resources.Add("BooleanToVisibilityConverter", new BooleanToVisibilityConverter());
            }
            if (!this.Resources.Contains("InverseBooleanToVisibilityConverter"))
            {
                this.Resources.Add("InverseBooleanToVisibilityConverter", new InverseBooleanToVisibilityConverter());
            }

            // Simulate initial authentication state (for testing)
            SetAuthenticationState(false, "Guest", "Guest"); // Default to not authenticated
        }

        /// <summary>
        /// Updates visibility of menu items based on the current UserRole.
        /// </summary>
        private static void OnUserRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavMenu navMenu = (NavMenu)d;
            navMenu.UpdateMenuItemVisibility((string)e.NewValue);
        }

        /// <summary>
        /// Updates visibility of menu items based on the current IsAuthenticated state.
        /// </summary>
        private static void OnIsAuthenticatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavMenu navMenu = (NavMenu)d;
            navMenu.UpdateMenuItemVisibility(navMenu.UserRole); // Re-evaluate visibility when auth state changes
        }

        /// <summary>
        /// Sets the authentication state and updates UI visibility.
        /// </summary>
        /// <param name="isAuthenticated">True if user is authenticated.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="userRole">The user's role (e.g., "AD", "ME", "CO", "Guest").</param>
        public void SetAuthenticationState(bool isAuthenticated, string userName, string userRole)
        {
            this.IsAuthenticated = isAuthenticated;
            this.UserName = userName;
            this.UserRole = userRole; // This will trigger OnUserRoleChanged and update visibility
        }

        /// <summary>
        /// Internal method to update the visibility of navigation items based on roles.
        /// </summary>
        /// <param name="role">The current user role.</param>
        private void UpdateMenuItemVisibility(string role)
        {
            // Blazor roles: AD, MA, ST (Management)
            // ME, AD, ST, MA (Appointment)
            // CO (Consultant Dashboard)

            // Management visibility
            IsManagementVisible = role.Contains("AD") || role.Contains("MA") || role.Contains("ST");

            // Appointment visibility
            IsAppointmentVisible = role.Contains("ME") || role.Contains("AD") || role.Contains("ST") || role.Contains("MA");

            // Consultant Dashboard visibility
            IsConsultantDashboardVisible = role.Contains("CO");
        }


        /// <summary>
        /// Handles clicks on navigation buttons and raises the NavigateRequested event.
        /// </summary>
        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                NavigateRequested?.Invoke(this, pageName);
                IsOverlayOpen = false; // Close overlay on navigation
            }
        }

        /// <summary>
        /// Toggles the visibility of the avatar overlay popup.
        /// </summary>
        private void Avatar_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsOverlayOpen = !IsOverlayOpen;
        }

        /// <summary>
        /// Handles the "Your Profile" button click in the avatar overlay.
        /// </summary>
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigating to Your Profile...", "Profile", MessageBoxButton.OK, MessageBoxImage.Information);
            IsOverlayOpen = false; // Close overlay after click
            // In a real application, you would navigate to the user profile page
            NavigateRequested?.Invoke(this, "Profile"); // Assuming a Profile.xaml page
        }

        /// <summary>
        /// Handles the "Logout" button click in the avatar overlay.
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logging out...", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            IsOverlayOpen = false; // Close overlay after click
            // In a real application, implement actual logout logic
            SetAuthenticationState(false, "Guest", "Guest"); // Simulate logout
            NavigateRequested?.Invoke(this, "Login"); // Navigate to login page
        }

        /// <summary>
        /// Handles the "Get Started" button click for unauthenticated users.
        /// </summary>
        private void GetStartedButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigating to Login/Registration...", "Get Started", MessageBoxButton.OK, MessageBoxImage.Information);
            // In a real application, navigate to the login/registration page            NavigateRequested?.Invoke(this, "Login"); // Assuming a Login.xaml page
        }


        /// <summary>
        /// Handles the search button click.
        /// </summary>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Searching for: {SearchTextBox.Text}", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            // Implement actual search logic here
        }

        /// <summary>
        /// Handles GotFocus event for SearchTextBox to clear default text.
        /// </summary>
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == " Search")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black; // Change text color when focused
            }
        }

        /// <summary>
        /// Handles LostFocus event for SearchTextBox to restore default text if empty.
        /// </summary>
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = " Search";
                SearchTextBox.Foreground = (Brush)FindResource("TextMutedBrush"); // Restore muted text color
            }
        }
    }

    // --- Converters (can be moved to App.xaml.cs if desired for global use) ---
   

  
}
