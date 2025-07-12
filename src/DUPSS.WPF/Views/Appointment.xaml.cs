using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using DUPSS.Common;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DUPSS.WPF.Views
{
    public partial class Appointment : Page
    {
        private readonly AppointmentApiService _appointmentService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        private string? _currentUserId;
        private List<AppointmentDTO> _appointments = new();

        public Appointment()
        {
            InitializeComponent();

            _appointmentService = new AppointmentApiService(App.HttpClient);
            var secureStorage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(new AuthApiService(App.HttpClient), secureStorage);

            Loaded += Appointment_Loaded;
        }

        private async void Appointment_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserIdAsync();
            await LoadAppointmentsAsync();
        }

        private async Task LoadUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            _currentUserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private async Task LoadAppointmentsAsync()
        {
            if (!string.IsNullOrEmpty(_currentUserId))
            {
                _appointments = await _appointmentService.GetAppointmentsForMemberAsync(_currentUserId);

                ShowNextAppointment();

                await AutoUpdateExpiredAppointmentsAsync();

                ApplyFilter();


            }
        }

        private void ApplyFilter()
        {
            var keyword = SearchBox.Text == "Find by consultant" ? "" : SearchBox.Text.Trim();
            var from = FromDatePicker.SelectedDate;
            var to = ToDatePicker.SelectedDate;
            var status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = _appointments
    .Where(a =>
        (string.IsNullOrEmpty(keyword) || a.Consultant?.Username?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) &&
        (!from.HasValue || a.AppointmentDate.ToLocalTime().Date >= from.Value.Date) &&
        (!to.HasValue || a.AppointmentDate.ToLocalTime().Date <= to.Value.Date) &&
        (string.IsNullOrEmpty(status) || status == "-- Status --" || a.Status == status)
    )
    .OrderByDescending(a => a.Status == "Pending" || a.Status == "Accepted")
    .ThenBy(a => a.AppointmentDate)
    .Select(dto => new AppointmentViewModel(dto))
    .ToList();

            // Đánh dấu next appointment
            var next = filtered
                .Where(a => a.AppointmentDate > DateTime.UtcNow && (a.Status == "Pending" || a.Status == "Accepted"))
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefault();

            if (next != null)
            {
                next.IsNext = true;
            }

            AppointmentDataGrid.ItemsSource = filtered;

        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AppointmentViewModel appt)
            {
                if (MessageBox.Show("Cancel this appointment?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var success = await _appointmentService.UpdateAppointmentStatusAsync(appt.AppointmentId, "Cancel");
                    if (success)
                    {
                        appt.Status = "Cancel";
                        ApplyFilter();
                        MessageBox.Show("Appointment cancelled.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel.");
                    }
                }
            }
        }

        private void FilterButton_Click(object _, RoutedEventArgs __) => ApplyFilter();

        private void ResetButton_Click(object _, RoutedEventArgs __)
        {
            SearchBox.Text = "Find by consultant";
            SearchBox.Foreground = Brushes.Gray;
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = 0;
            ApplyFilter();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Find by consultant")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Find by consultant";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void ShowNextAppointment()
        {
            var next = _appointments
                .Where(a => a.AppointmentDate > DateTime.UtcNow && (a.Status == "Pending" || a.Status == "Accepted"))
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefault();

            if (next != null)
            {
                MessageBox.Show(
                    $"📌 Your next appointment:\n\n" +
                    $"With: {next.Consultant?.Username}\n" +
                    $"At: {next.AppointmentDate.ToLocalTime():dd/MM/yyyy HH:mm} - {next.AppointmentDate.ToLocalTime().AddMinutes(40):HH:mm}\n" +
                    $"Status: {next.Status}",
                    "Upcoming Appointment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task AutoUpdateExpiredAppointmentsAsync()
        {
            var now = DateTime.UtcNow;
            foreach (var appt in _appointments)
            {
                if (appt.AppointmentDate.AddMinutes(40) < now &&
                    (appt.Status == "Pending" || appt.Status == "Accepted"))
                {
                    var success = await _appointmentService.UpdateAppointmentStatusAsync(appt.AppointmentId, "Missed");
                    if (success)
                    {
                        appt.Status = "Missed";
                    }
                }
            }
            ApplyFilter();
        }


    }


    public class AppointmentViewModel
    {
        public string AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string ConsultantUsername { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public bool IsNext { get; set; }


        public string DisplayDate =>
            $"{AppointmentDate.ToLocalTime():dd/MM/yyyy HH:mm} - {AppointmentDate.ToLocalTime().AddMinutes(40):HH:mm}";

        public Visibility CancelVisibility =>
            (Status == "Pending" || Status == "Accepted") && (AppointmentDate - DateTime.UtcNow).TotalHours >= 4
                ? Visibility.Visible : Visibility.Collapsed;

        public AppointmentViewModel(AppointmentDTO dto)
        {
            AppointmentId = dto.AppointmentId;
            AppointmentDate = dto.AppointmentDate.UtcDateTime;
            ConsultantUsername = dto.Consultant?.Username ?? "";
            Status = dto.Status;
            Notes = dto.Notes ?? "";
        }
    }

}

