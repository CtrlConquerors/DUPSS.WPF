using DUPSS.ApiClients;
using DUPSS.Common;
using DUPSS.DTO.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DUPSS.WPF.Views
{
    public partial class ConsultantDashboard : Page
    {
        private readonly AppointmentApiService _appointmentService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        private string? _currentConsultantId;
        private List<AppointmentDTO> _allAppointments = new();

        public ConsultantDashboard()
        {
            InitializeComponent();

            _appointmentService = new AppointmentApiService(App.HttpClient);
            var secureStorage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(new AuthApiService(App.HttpClient), secureStorage);

            Loaded += ConsultantDashboard_Loaded;
        }

        private async void ConsultantDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadConsultantIdAsync();
            await LoadAppointmentsAsync();
        }

        private async Task LoadConsultantIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            _currentConsultantId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private void ViewAllAppointments_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new AppointmentForConsultant());
        }


        private async Task LoadAppointmentsAsync()
        {
            if (string.IsNullOrEmpty(_currentConsultantId))
                return;

            _allAppointments = await _appointmentService.GetAppointmentsForConsultantAsync(_currentConsultantId);

            // Tính toán số liệu
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            TodayCountText.Text = _allAppointments.Count(a =>
            {
                var local = a.AppointmentDate.ToLocalTime();
                return local >= today && local < tomorrow && (a.Status == "Pending" || a.Status == "Accepted");
            }).ToString();

            UpcomingCountText.Text = _allAppointments.Count(a =>
                a.AppointmentDate > DateTime.UtcNow && (a.Status == "Pending" || a.Status == "Accepted")).ToString();

            FinishedCountText.Text = _allAppointments.Count(a => a.Status == "Finished").ToString();
            MissedCountText.Text = _allAppointments.Count(a => a.Status == "Missed").ToString();
            CancelledCountText.Text = _allAppointments.Count(a => a.Status == "Cancel").ToString();
            DeclinedCountText.Text = _allAppointments.Count(a => a.Status == "Declined").ToString();

            // Lấy top 5 upcoming
            var upcoming = _allAppointments
                .Where(a => a.AppointmentDate > DateTime.UtcNow && (a.Status == "Pending" || a.Status == "Accepted"))
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .Select(dto => new ConsultantDashboardAppointmentViewModel(dto))
                .ToList();

            UpcomingDataGrid.ItemsSource = upcoming;
        }
    }

    // ViewModel riêng cho dashboard, KHÁC TÊN với Appointment.xaml.cs
    public class ConsultantDashboardAppointmentViewModel
    {
        public string DisplayDate { get; }
        public string MemberUsername { get; }
        public string Topic { get; }
        public string Status { get; }

        public ConsultantDashboardAppointmentViewModel(AppointmentDTO dto)
        {
            var start = dto.AppointmentDate.ToLocalTime();
            DisplayDate = $"{start:dd/MM/yyyy HH:mm} - {start.AddMinutes(40):HH:mm}";
            MemberUsername = dto.Member?.Username ?? "";
            Topic = dto.Topic ?? "";
            Status = dto.Status ?? "";
        }
    }
}
