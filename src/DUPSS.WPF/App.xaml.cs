using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using DUPSS.WPF;

namespace DUPSS.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; } // Static property to access configuration globally

        protected override void OnStartup(StartupEventArgs e)
        {
            // Build configuration from appsettings.json
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Example of how to access the BaseUrl
            string baseUrl = Configuration["ApiSettings:BaseUrl"];
            MessageBox.Show($"API Base URL loaded: {baseUrl}", "Configuration Loaded"); // For demonstration

            // Call the base OnStartup method to ensure normal WPF startup processes
            base.OnStartup(e);

            // You might want to initialize your main window here, especially if
            // you're moving away from StartupUri in App.xaml
            // For example:
            // var mainWindow = new Home(); // Assuming Home is your main window class
            // mainWindow.Show();
        }
    }
}