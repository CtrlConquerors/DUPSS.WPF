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

namespace DUPSS.WPF.Views
{
    public partial class AppointmentForConsultant : Page
    {
        private readonly AppointmentApiService _apiService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;
        private string? _currentConsultantId;
        private List<AppointmentDTO> _allAppointments = new();

        public AppointmentForConsultant()
        {
            InitializeComponent();

            var httpClient = App.HttpClient; // Dùng chung HttpClient đã có BaseAddress
            _apiService = new AppointmentApiService(httpClient);

            var storage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(new AuthApiService(httpClient), storage);

            Loaded += AppointmentForConsultant_Loaded;
        }

        private async void AppointmentForConsultant_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadConsultantIdAsync();
            await LoadAppointments();
        }

        private async Task LoadConsultantIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                _currentConsultantId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        private async Task LoadAppointments()
        {
            if (!string.IsNullOrEmpty(_currentConsultantId))
            {
                _allAppointments = await _apiService.GetAppointmentsForConsultantAsync(_currentConsultantId);

                var sorted = _allAppointments
                    .OrderByDescending(a => a.Status == "Pending" || a.Status == "Accepted")
                    .ThenBy(a => a.AppointmentDate)
                    .ToList();

                AppointmentGrid.ItemsSource = sorted;
            }
        }


        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var keyword = SearchTextBox.Text?.Trim() ?? "";
            var from = FromDatePicker.SelectedDate;
            var to = ToDatePicker.SelectedDate;
            var statusItem = StatusComboBox.SelectedItem as ComboBoxItem;
            var selectedStatus = statusItem?.Content?.ToString() ?? "";

            var filtered = _allAppointments.Where(a =>
                (string.IsNullOrEmpty(keyword) ||
                 (a.Member?.Username?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 a.Topic.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                && (!from.HasValue || a.AppointmentDate.Date >= from.Value.Date)
                && (!to.HasValue || a.AppointmentDate.Date <= to.Value.Date)
                && (string.IsNullOrEmpty(selectedStatus) || a.Status == selectedStatus)
            )
            .OrderByDescending(a => a.Status == "Pending" || a.Status == "Accepted")
            .ThenBy(a => a.AppointmentDate)
            .ToList();

            AppointmentGrid.ItemsSource = filtered;
        }


        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = -1;
            AppointmentGrid.ItemsSource = _allAppointments;
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            var appointmentId = (string)((Button)sender).Tag;
            var success = await _apiService.UpdateAppointmentStatusAsync(appointmentId, "Accepted");
            if (success) UpdateLocalStatus(appointmentId, "Accepted");
        }

        private async void Decline_Click(object sender, RoutedEventArgs e)
        {
            var appointmentId = (string)((Button)sender).Tag;
            var success = await _apiService.UpdateAppointmentStatusAsync(appointmentId, "Declined");
            if (success) UpdateLocalStatus(appointmentId, "Declined");
        }

        private async void Finish_Click(object sender, RoutedEventArgs e)
        {
            var appointmentId = (string)((Button)sender).Tag;
            var success = await _apiService.UpdateAppointmentStatusAsync(appointmentId, "Finished");
            if (success) UpdateLocalStatus(appointmentId, "Finished");
        }

        private async void SaveNote_Click(object sender, RoutedEventArgs e)
        {
            var appointmentId = (string)((Button)sender).Tag;
            var appt = _allAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            if (appt != null)
            {
                var newNote = appt.Notes ?? "";
                var success = await _apiService.UpdateAppointmentNotesAsync(appointmentId, newNote);
                if (success)
                {
                    MessageBox.Show("✅ Note saved successfully!");
                }
                else
                {
                    MessageBox.Show("❌ Failed to save note.");
                }
            }
        }

        private void UpdateLocalStatus(string appointmentId, string newStatus)
        {
            var appt = _allAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            if (appt != null)
            {
                appt.Status = newStatus;
                AppointmentGrid.Items.Refresh();
            }
        }
    }
}
