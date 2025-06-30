using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation; // Required for Frame navigation

namespace DUPSS.WPF.Layouts // Namespace matches the folder structure
{
    /// <summary>
    /// Interaction logic for MainLayout.xaml
    /// </summary>
    public partial class MainLayout : Window
    {
        public MainLayout()
        {
            InitializeComponent();
            this.Loaded += MainLayout_Loaded; // Attach event handler for when the window is loaded
        }

        private void MainLayout_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the initial page to load in the Frame
            // You might want to navigate to your Home.xaml here
            NavigateToPage("Home"); // Default to Home page
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
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logging out...", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            // In a real application, you would implement actual logout logic here,
            // e.g., clearing authentication tokens, redirecting to a login window.
            // Example: var loginWindow = new LoginWindow();
            // loginWindow.Show();
            // this.Close();
        }
    }
}
