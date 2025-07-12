using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using DUPSS.Common; // WpfSecureStorageService
using Microsoft.AspNetCore.Components.Authorization; // AuthenticationStateProvider
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DUPSS.WPF.Views
{
    public partial class BookAppointment : Page
    {
        private readonly AppointmentApiService _appointmentService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;
        private readonly UserApiService _userApiService;

        private string? _currentUserId;

        public BookAppointment()
        {
            InitializeComponent();

            _appointmentService = new AppointmentApiService(App.HttpClient);
            _userApiService = new UserApiService(App.HttpClient);

            var wpfSecureStorage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(new AuthApiService(App.HttpClient), wpfSecureStorage);

            Loaded += BookAppointment_Loaded;
        }

        private async void BookAppointment_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserIdAsync();
            LoadAvailableDates();
        }

        private async Task LoadUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                _currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        private void LoadAvailableDates()
        {
            var next7Days = Enumerable.Range(0, 7)
                                      .Select(i => DateTime.Today.AddDays(i))
                                      .ToList();

            DateComboBox.ItemsSource = next7Days;
            DateComboBox.SelectedIndex = 0;
        }

        private void DateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var slots = new List<string> { "17:30", "18:30", "19:30", "20:30" };
            SlotComboBox.ItemsSource = slots;
            SlotComboBox.SelectedIndex = 0;
        }

        private async void BookNow_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                MessageBox.Show("Cannot book: user is not authenticated.", "Error");
                return;
            }

            if (DateComboBox.SelectedItem == null ||
                SlotComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(TopicTextBox.Text))
            {
                MessageBox.Show("Please select date, time slot and enter topic.",
                                "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // ✏️ 1️⃣ Kiểm tra user đã có appointment Pending/Accepted chưa
                var existingAppointments = await _appointmentService.GetAppointmentsForMemberAsync(_currentUserId);
                bool hasActive = existingAppointments.Any(a => a.Status == "Pending" || a.Status == "Accepted");

                if (hasActive)
                {
                    MessageBox.Show("⚠️ You already have an active appointment (Pending/Accepted). Please cancel or finish it before booking a new one.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedDate = (DateTime)DateComboBox.SelectedItem;
                var slot = SlotComboBox.SelectedItem.ToString();
                var topic = TopicTextBox.Text.Trim();

                if (!TimeSpan.TryParse(slot, out TimeSpan time))
                {
                    MessageBox.Show("Invalid time slot format.", "Error");
                    return;
                }

                var appointmentDate = selectedDate.Date.Add(time).ToUniversalTime();

                // 2️⃣ Tự động tìm consultant ít bận nhất & không bị conflict
                var consultants = await _userApiService.GetConsultantsAsync();

                var consultantLoad = new List<(UserDTO Consultant, int ActiveCount)>();

                foreach (var consultant in consultants)
                {
                    var consultantAppointments = await _appointmentService.GetAppointmentsForConsultantAsync(consultant.UserId);

                    bool hasConflict = consultantAppointments.Any(a =>
                    {
                        if (a.Status == "Cancel") return false;

                        var existingStart = a.AppointmentDate;
                        var existingEnd = existingStart.AddMinutes(40);
                        var requestedStart = appointmentDate;
                        var requestedEnd = appointmentDate.AddMinutes(40);

                        return requestedStart < existingEnd && existingStart < requestedEnd;
                    });

                    if (!hasConflict)
                    {
                        int activeCount = consultantAppointments.Count(a => a.Status == "Pending" || a.Status == "Accepted");
                        consultantLoad.Add((consultant, activeCount));
                    }
                }

                var best = consultantLoad.OrderBy(x => x.ActiveCount).ThenBy(_ => Guid.NewGuid()).FirstOrDefault();

                if (best.Consultant == null)
                {
                    MessageBox.Show("No available consultant at selected time.", "Info");
                    return;
                }

                var dto = new AppointmentDTO
                {
                    AppointmentId = Guid.NewGuid().ToString(),
                    MemberId = _currentUserId,
                    ConsultantId = best.Consultant.UserId,
                    Topic = topic,
                    AppointmentDate = appointmentDate,
                    Status = "Pending"
                };

                bool success = await _appointmentService.CreateAppointmentAsync(dto);

                if (success)
                {
                    MessageBox.Show("✅ Appointment booked successfully!", "Success");

                    // Navigate back to appointment list page
                    NavigationService?.Navigate(new Appointment());
                }
                else
                {
                    MessageBox.Show("❌ Failed to book appointment.", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error");
            }
        }
    }
}
