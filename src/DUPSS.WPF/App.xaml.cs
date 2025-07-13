using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using System.Net.Http;
using DUPSS.ApiClients; // Make sure to include this namespace
using DUPSS.Common; // Make sure to include this namespace for IProtectedLocalStorage and WpfSecureStorageService

namespace DUPSS.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; } = null!; // Static property to access configuration globally
        public static HttpClient HttpClient { get; private set; } = null!; // Initialize with null-forgiving operator, will be set in OnStartup

        // Declare static properties for your API services
        public static CourseApiService CourseApiService { get; private set; } = null!;
        public static CourseEnrollApiService CourseEnrollApiService { get; private set; } = null!;
        public static BlogApiService? BlogApiService { get; private set; }
        public static UserApiService UserApiService { get; private set; } = null!;
        public static JwtAuthenticationStateProvider JwtAuthenticationStateProvider { get; private set; } = null!;
        public static AuthApiService AuthApiService { get; private set; } = null!; // ADDED: Static property for AuthApiService

        protected override void OnStartup(StartupEventArgs e)
        {
            // Build configuration from appsettings.json
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string baseUrl = Configuration["ApiSettings:BaseUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                MessageBox.Show("⚠️ Missing ApiSettings:BaseUrl in appsettings.json! Application will shut down.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Initialize HttpClient
            HttpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            MessageBox.Show($"API Base URL loaded: {baseUrl}", "Configuration Loaded"); // For demonstration

            // --- Initialize your services here ---

            // Use your provided WpfSecureStorageService for secure local storage
            var localStorage = new WpfSecureStorageService();

            // Initialize AuthApiService and assign to static property
            AuthApiService = new AuthApiService(HttpClient); // Initialized here
                                                             // Initialize BlogApiService and assign to static property
            BlogApiService = new BlogApiService(HttpClient);

            // Initialize your static API service properties
            CourseApiService = new CourseApiService(HttpClient);
            CourseEnrollApiService = new CourseEnrollApiService(HttpClient);
            UserApiService = new UserApiService(HttpClient);
            // Ensure JwtAuthenticationStateProvider uses the static AuthApiService
            JwtAuthenticationStateProvider = new JwtAuthenticationStateProvider(AuthApiService, localStorage);

            // Call the base OnStartup method to ensure normal WPF startup processes
            base.OnStartup(e);

            // You might want to initialize your main window here, especially if
            // you're moving away from StartupUri in App.xaml
            // For example:
            // var mainWindow = new Home(); // Assuming Home is your main window class
            // mainWindow.Show();
        }
    }

    // The InMemoryProtectedLocalStorage and ProtectedBrowserStorageResult classes
    // are no longer needed here as you have WpfSecureStorageService and
    // ProtectedBrowserStorageResult defined in DUPSS.Common.
    // Ensure DUPSS.Common project is referenced and compiles correctly.
}
