using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DUPSS.WPF.Views
{
    public partial class Appointment : Page
    {
        private readonly AppointmentApiService _appointmentService;
        private List<AppointmentDTO> _appointments = new();
        private List<AppointmentViewModel> _filteredAppointments = new();

        public Appointment()
        {
            InitializeComponent();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7026/")  // <-- thay đúng URL API của bạn
            };

            _appointmentService = new AppointmentApiService(httpClient);
            LoadAppointments();
        }

        private async void LoadAppointments()
        {
            try
            {
                // TODO: Replace with actual memberId from login
                var memberId = "df8d14c0-f6cc-4c6b-98cd-175b28bc57eb";
                _appointments = await _appointmentService.GetAppointmentsForMemberAsync(memberId);

                // Debug: in ra số lượng
                MessageBox.Show($"Loaded {_appointments.Count} appointments from API.");

                foreach (var appt in _appointments)
                {
                    MessageBox.Show($"AppointmentId={appt.AppointmentId}, Date={appt.AppointmentDate}, Status={appt.Status}, Consultant={appt.Consultant?.Username ?? "null"}");
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}");
            }
        }


        private void ApplyFilter()
{
    var keyword = SearchBox.Text == "Find by consultant" ? "" : SearchBox.Text;
    var from = FromDatePicker.SelectedDate;
    var to = ToDatePicker.SelectedDate;
    var selectedStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

    _filteredAppointments = _appointments
        .Where(a =>
            (string.IsNullOrEmpty(keyword) || (a.Consultant?.Username?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)) &&
            (!from.HasValue || a.AppointmentDate.ToLocalTime().Date >= from.Value.Date) &&
            (!to.HasValue || a.AppointmentDate.ToLocalTime().Date <= to.Value.Date) &&
            (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "-- Status --" || a.Status == selectedStatus)
        )
        .Select(a => new AppointmentViewModel(a))
        .OrderByDescending(a => a.Status == "Pending" || a.Status == "Accepted")
        .ThenBy(a => a.AppointmentDate)
        .ToList();

    MessageBox.Show($"Filtered count: {_filteredAppointments.Count}");

    AppointmentDataGrid.ItemsSource = _filteredAppointments;
}


        private void FilterButton_Click(object sender, RoutedEventArgs e) => ApplyFilter();

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "Find by consultant";
            SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = 0;
            ApplyFilter();
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AppointmentViewModel appt)
            {
                if (MessageBox.Show("Are you sure to cancel this appointment?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var success = await _appointmentService.UpdateAppointmentStatusAsync(appt.AppointmentId, "Cancel");
                    if (success)
                    {
                        appt.Status = "Cancel";
                        ApplyFilter();
                        MessageBox.Show("Appointment cancelled successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel appointment.");
                    }
                }
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Find by consultant")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Find by consultant";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }

    public class AppointmentViewModel
    {
        public string AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string ConsultantUsername { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string DisplayDate => $"{AppointmentDate.ToLocalTime():dd/MM/yyyy HH:mm} - {AppointmentDate.ToLocalTime().AddMinutes(40):HH:mm}";
        public bool CanCancel => (Status == "Pending" || Status == "Accepted") && (AppointmentDate - DateTime.UtcNow).TotalHours >= 4;

        public AppointmentViewModel(AppointmentDTO dto)
        {
            AppointmentId = dto.AppointmentId;
            AppointmentDate = dto.AppointmentDate.UtcDateTime;
            ConsultantUsername = dto.Consultant?.Username ?? "";
            Status = dto.Status;
            Notes = dto.Notes;
        }
    }
}
