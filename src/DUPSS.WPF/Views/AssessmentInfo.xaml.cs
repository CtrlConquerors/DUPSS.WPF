using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DUPSS.WPF.Views
{
    public partial class AssessmentInfo : Page
    {
        private readonly AssessmentApiService _apiService;
        private string _assessmentId = string.Empty;
        private AssessmentDTO? _assessment;

        public AssessmentInfo(string assessmentId)
        {
            InitializeComponent();
            _apiService = App.AssessmentApiService;
            _assessmentId = assessmentId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Show(loading: true);
            try
            {
                _assessment = await _apiService.GetByIdAsync(_assessmentId);
                if (_assessment == null)
                {
                    Show(notFound: true);
                    return;
                }

                TitleTextBlock.Text = $"{_assessment.AssessmentType} Assessment";
                VersionTextBlock.Text = $"Version {_assessment.Version}";
                LanguageTextBlock.Text = GetLanguageDisplay(_assessment.Language);
                DurationTextBlock.Text = GetEstimatedDuration(_assessment.AssessmentType);
                DescriptionTextBlock.Text = _assessment.Description ?? GetDetailedDescription(_assessment.AssessmentType);

                if (_assessment.AssessmentType == "CRAFFT")
                {
                    ExtraContentPanel.Children.Add(new TextBlock
                    {
                        Text = "CRAFFT stands for:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 10, 0, 5)
                    });
                    ExtraContentPanel.Children.Add(new TextBlock
                    {
                        Text = "C - Car\nR - Relax\nA - Alone\nF - Forget\nF - Friends\nT - Trouble",
                        TextWrapping = TextWrapping.Wrap
                    });
                }
                else if (_assessment.AssessmentType == "ASSIST")
                {
                    ExtraContentPanel.Children.Add(new TextBlock
                    {
                        Text = "Covers: Tobacco, Alcohol, Cannabis, Cocaine, Amphetamines, Inhalants, Sedatives, Hallucinogens, Opioids, Others.",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 10, 0, 0)
                    });
                }

                Show(content: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assessment info: {ex.Message}");
                Show(error: true);
            }
        }

        private void Show(bool loading = false, bool error = false, bool notFound = false, bool content = false)
        {
            LoadingPanel.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
            ErrorPanel.Visibility = error ? Visibility.Visible : Visibility.Collapsed;
            NotFoundPanel.Visibility = notFound ? Visibility.Visible : Visibility.Collapsed;
            ContentPanel.Visibility = content ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Retry_Click(object sender, RoutedEventArgs e) => Page_Loaded(sender, e);

        private void StartAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_assessmentId))
                NavigationService?.Navigate(new AssessmentTake(_assessmentId));
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Assessment());
        }

        private string GetLanguageDisplay(string language) => language switch
        {
            "eng" => "English",
            "vie" => "Tiếng Việt",
            _ => language.ToUpper()
        };

        private string GetEstimatedDuration(string type) => type switch
        {
            "CRAFFT" => "2-3 minutes",
            "ASSIST" => "5-10 minutes",
            _ => "3-5 minutes"
        };

        private string GetDetailedDescription(string type) => type switch
        {
            "CRAFFT" => "A validated screening instrument for adolescents and young adults to identify substance use risks. It includes 6 core behavior-based questions.",
            "ASSIST" => "WHO's comprehensive tool to identify at-risk individuals for various substance uses, with validated cultural adaptation.",
            _ => "A screening tool for evaluating mental health and substance use risk, offering personalized recommendations."
        };
    }
}
