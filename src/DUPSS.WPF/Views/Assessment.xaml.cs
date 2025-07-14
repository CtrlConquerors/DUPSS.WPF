using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;

namespace DUPSS.WPF.Views
{
    public partial class Assessment : INotifyPropertyChanged
    {
        private readonly AssessmentApiService _assessmentApiService;
        private List<AssessmentDTO> _assessments;
        private bool _hasError;
        private bool _showAllAssessments;
        private const int DefaultAssessmentDisplayLimit = 6;
        private bool _isLoaded;

        public Assessment()
        {
            InitializeComponent();
            _assessmentApiService = new AssessmentApiService(App.HttpClient);
            _assessments = new List<AssessmentDTO>();
            _isLoaded = false;
            DataContext = this;
            Console.WriteLine("[AssessmentPage] Constructor: AssessmentApiService initialized.");
            Loaded += AssessmentPage_Loaded;
        }

        public List<AssessmentDTO> Assessments
        {
            get => _assessments;
            private set
            {
                _assessments = value;
                OnPropertyChanged(nameof(Assessments));
                OnPropertyChanged(nameof(DisplayAssessments));
                OnPropertyChanged(nameof(ToggleButtonVisibility));
                OnPropertyChanged(nameof(ToggleButtonText));
                Console.WriteLine($"[Assessments] Updated: Count={_assessments.Count}");
            }
        }

        public IEnumerable<AssessmentDTO> DisplayAssessments
        {
            get => _showAllAssessments ? Assessments : Assessments.Take(DefaultAssessmentDisplayLimit);
        }

        public bool HasError
        {
            get => _hasError;
            private set
            {
                _hasError = value;
                OnPropertyChanged(nameof(HasError));
                Console.WriteLine($"[HasError] Updated: {value}");
            }
        }

        public Visibility ToggleButtonVisibility
        {
            get => _isLoaded && Assessments.Count > DefaultAssessmentDisplayLimit ? Visibility.Visible : Visibility.Collapsed;
        }

        public string ToggleButtonText
        {
            get => _showAllAssessments ? "Show Less" : "Show More Assessments";
        }

        public static int NonZero
        {
            get => 1; // Used for DataTrigger comparison
        }

        private async void AssessmentPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
                return;
            _isLoaded = true;
            Console.WriteLine("[AssessmentPage] Loaded: Starting assessment load.");
            await LoadAssessmentsAsync();
            OnPropertyChanged(nameof(ToggleButtonVisibility));
            Console.WriteLine("[AssessmentPage] Hydration complete: isLoaded=true");
        }

        private async Task LoadAssessmentsAsync()
        {
            try
            {
                Assessments = await _assessmentApiService.GetAllAsync();
                HasError = false;
                Console.WriteLine($"[LoadAssessmentsAsync] Loaded {Assessments.Count} assessments.");
            }
            catch (Exception ex)
            {
                HasError = true;
                Console.WriteLine($"[LoadAssessmentsAsync] Error: {ex.Message}");
            }
        }

        private async void RetryLoadAssessments_Click(object sender, RoutedEventArgs e)
        {
            HasError = false;
            Console.WriteLine("[RetryLoadAssessments] Retrying assessment load.");
            await LoadAssessmentsAsync();
        }

        private void ToggleShowAllAssessments_Click(object sender, RoutedEventArgs e)
        {
            _showAllAssessments = !_showAllAssessments;
            OnPropertyChanged(nameof(DisplayAssessments));
            OnPropertyChanged(nameof(ToggleButtonText));
            Console.WriteLine($"[ToggleShowAllAssessments] showAllAssessments={_showAllAssessments}");
            if (_showAllAssessments)
                return;
            var scrollViewer = FindVisualAncestor<ScrollViewer>(AssessmentContainer);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
                Console.WriteLine("[ToggleShowAllAssessments] Scrolled to top of ScrollViewer.");
            }
            else
            {
                Console.WriteLine("[ToggleShowAllAssessments] ScrollViewer not found.");
            }
        }

        // Helper method to find the ScrollViewer in the visual tree
        private static T? FindVisualAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        private void StartAssessment_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AssessmentDTO assessment)
                return;
            Console.WriteLine($"[StartAssessment] Navigating to AssessmentId={assessment.AssessmentId}");
            NavigationService?.Navigate(new Uri($"/Views/AssessmentTake.xaml?assessmentId={assessment.AssessmentId}", UriKind.Relative));
        }

        private void ViewAssessmentInfo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AssessmentDTO assessment)
                return;
            Console.WriteLine($"[ViewAssessmentInfo] Navigating to AssessmentId={assessment.AssessmentId}");
            NavigationService?.Navigate(new Uri($"/Views/AssessmentInfo.xaml?assessmentId={assessment.AssessmentId}", UriKind.Relative));
        }

        public static string GetDefaultDescription(string assessmentType)
        {
            return assessmentType switch
            {
                "CRAFFT" => "A brief screening tool to identify adolescents and young adults who may have a substance use disorder or are at risk of developing one.",
                "ASSIST" => "The World Health Organization's comprehensive screening tool for alcohol, tobacco, and other substance use problems.",
                _ => "A validated screening tool to assess mental health and substance use patterns."
            };
        }

        public static string GetLanguageDisplay(string language)
        {
            return language.ToLower() switch
            {
                "eng" => "English",
                "vie" => "Tiếng Việt",
                _ => language.ToUpper()
            };
        }

        public static string GetEstimatedDuration(string assessmentType)
        {
            return assessmentType switch
            {
                "CRAFFT" => "2-3 min",
                "ASSIST" => "5-10 min",
                _ => "3-5 min"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class AssessmentDTOExtensions
    {
        public static string DescriptionDisplay(this AssessmentDTO assessment)
        {
            if (string.IsNullOrEmpty(assessment.Description))
                return Assessment.GetDefaultDescription(assessment.AssessmentType);
            return assessment.Description.Length > 150
                ? assessment.Description.Substring(0, 150) + "..."
                : assessment.Description;
        }

        public static string LanguageDisplay(this AssessmentDTO assessment)
        {
            return Assessment.GetLanguageDisplay(assessment.Language);
        }

        public static string EstimatedDuration(this AssessmentDTO assessment)
        {
            return Assessment.GetEstimatedDuration(assessment.AssessmentType);
        }
    }
}