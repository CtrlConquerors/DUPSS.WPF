using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using System.Windows;
using System.Windows.Controls;

namespace DUPSS.WPF.Views
{
    public partial class Assessment : Page
    {
        private AssessmentApiService _assessmentService;
        private List<AssessmentDTO> _assessments = new();
        private bool _showAll = false;
        private const int DefaultLimit = 6;

        public Assessment()
        {
            InitializeComponent();
            _assessmentService = App.AssessmentApiService;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAssessmentsAsync();
        }

        private async Task LoadAssessmentsAsync()
        {
            ShowPanels(loading: true);
            try
            {
                _assessments = await _assessmentService.GetAllAsync() ?? new();
                if (_assessments.Any())
                {
                    AssessmentList.ItemsSource = _showAll ? _assessments : _assessments.Take(DefaultLimit);
                    ShowPanels(list: true);
                    ShowMorePanel.Visibility = _assessments.Count > DefaultLimit ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    ShowPanels(noData: true);
                }
            }
            catch
            {
                ShowPanels(error: true);
            }
        }

        private void ShowPanels(bool loading = false, bool error = false, bool noData = false, bool list = false)
        {
            LoadingPanel.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
            ErrorPanel.Visibility = error ? Visibility.Visible : Visibility.Collapsed;
            NoDataPanel.Visibility = noData ? Visibility.Visible : Visibility.Collapsed;
            AssessmentList.Visibility = list ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAssessmentsAsync();
        }

        private void TakeAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                NavigationService?.Navigate(new AssessmentTake(id));
            }
        }

        private void LearnMore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                NavigationService?.Navigate(new AssessmentInfo(id));
            }
        }

        private void ToggleShowButton_Click(object sender, RoutedEventArgs e)
        {
            _showAll = !_showAll;
            AssessmentList.ItemsSource = _showAll ? _assessments : _assessments.Take(DefaultLimit);
            ToggleShowButton.Content = _showAll ? "Show Less" : "Show More Assessments";
        }
    }
}
