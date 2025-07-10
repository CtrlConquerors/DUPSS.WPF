using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DUPSS.WPF.Views
{
    public partial class BookAppointment : Page
    {
        private readonly AppointmentApiService _appointmentService;

        public BookAppointment()
        {
            InitializeComponent();

            // Khởi tạo service dùng HttpClient từ App (bạn cần khai báo public static HttpClient trong App.xaml.cs)
            _appointmentService = new AppointmentApiService(App.HttpClient);

            // Load danh sách ngày khả dụng khi mở trang
            LoadAvailableDatesAsync();
        }

        private async void LoadAvailableDatesAsync()
        {
            // Ví dụ: hiển thị 7 ngày tiếp theo kể từ hôm nay
            var next7Days = Enumerable.Range(0, 7)
                                      .Select(i => DateTime.Today.AddDays(i))
                                      .ToList();

            DateComboBox.ItemsSource = next7Days;
            DateComboBox.SelectedIndex = 0; // Chọn sẵn ngày đầu tiên
        }

        private void DateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateComboBox.SelectedItem is DateTime selectedDate)
            {
                // Hardcode slot demo – thực tế có thể load từ API theo ngày
                var slots = new List<string>
                {
                    "17:30", "18:30", "19:30", "20:30"
                };
                SlotComboBox.ItemsSource = slots;
                SlotComboBox.SelectedIndex = 0;
            }
        }

        private async void BookNow_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu bắt buộc
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
                // Giả sử memberId và consultantId lấy từ đăng nhập hoặc context
                string memberId = "test-member-id";
                string consultantId = "test-consultant";

                var selectedDate = (DateTime)DateComboBox.SelectedItem;
                var slot = SlotComboBox.SelectedItem.ToString();
                var topic = TopicTextBox.Text.Trim();

                // Ghép thành DateTime
                if (TimeSpan.TryParse(slot, out TimeSpan time))
                {
                    var appointmentDate = selectedDate.Date.Add(time);

                    var dto = new AppointmentDTO
                    {
                        AppointmentId = Guid.NewGuid().ToString(),
                        MemberId = memberId,
                        ConsultantId = consultantId,
                        Topic = topic,
                        AppointmentDate = appointmentDate.ToUniversalTime(),
                        Status = "Pending"
                    };

                    bool success = await _appointmentService.CreateAppointmentAsync(dto);

                    if (success)
                    {
                        MessageBox.Show("✅ Appointment booked successfully!",
                                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to book appointment. Please try again.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid time slot format.", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
