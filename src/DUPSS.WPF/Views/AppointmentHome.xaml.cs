using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DUPSS.DTO.DTOs;
using DUPSS.ApiClients;

namespace DUPSS.WPF.Views
{
    public partial class AppointmentHome : Page
    {
        private readonly UserApiService userService;

        public AppointmentHome()
        {
            InitializeComponent();

            userService = new UserApiService(new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7026/")
            });

            Loaded += AppointmentHome_Loaded;
        }

        private async void AppointmentHome_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[DEBUG] Loading consultants...");
            try
            {
                var consultants = await userService.GetConsultantsAsync();
                ConsultantsListBox.ItemsSource = consultants ?? new List<UserDTO>();

                if (consultants == null || consultants.Count == 0)
                {
                    MessageBox.Show("No consultants available. Please check back later.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] {ex}");
                MessageBox.Show($"Error loading consultants: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new DUPSS.WPF.Views.Appointment());
        }

        private void BookAppointment_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new BookAppointment());
        }
    }
}
