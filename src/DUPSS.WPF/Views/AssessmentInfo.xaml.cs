using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;

namespace DUPSS.WPF.Views
{
    public partial class AssessmentInfo : INotifyPropertyChanged
    {
        private readonly AssessmentApiService _assessmentApiService;
        private AssessmentDTO? _assessment;
        private bool _isLoading = true;
        private bool _hasError;
        private bool _isLoaded;

        public AssessmentInfo(AssessmentApiService assessmentApiService, string assessmentId)
        {
            InitializeComponent();
            _assessmentApiService = assessmentApiService ?? throw new ArgumentNullException(nameof(assessmentApiService));
            AssessmentId = assessmentId;
            DataContext = this;
            Console.WriteLine($"[AssessmentInfoPage] Constructor: AssessmentId={AssessmentId}");
            Loaded += AssessmentInfoPage_Loaded;
        }

        public string AssessmentId { get; }

        public AssessmentDTO? Assessment
        {
            get => _assessment;
            private set
            {
                _assessment = value;
                OnPropertyChanged(nameof(Assessment));
                OnPropertyChanged(nameof(AssessmentDescription));
                OnPropertyChanged(nameof(AssessmentQuestionCount));
                OnPropertyChanged(nameof(SpecificAssessmentInfo));
                OnPropertyChanged(nameof(AssessmentDetails));
                Console.WriteLine($"[Assessment] Updated: {(value != null ? $"AssessmentId={value.AssessmentId}" : "null")}");
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                Console.WriteLine($"[IsLoading] Updated: {value}");
            }
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

        public new bool IsLoaded
        {
            get => _isLoaded;
            private set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
                Console.WriteLine($"[IsLoaded] Updated: {value}");
            }
        }

        public string AssessmentDescription
        {
            get => Assessment?.Description ?? GetDetailedDescription(Assessment?.AssessmentType);
        }

        public string AssessmentQuestionCount
        {
            get => GetQuestionCount(Assessment?.AssessmentType);
        }

        public string SpecificAssessmentInfo
        {
            get => Assessment?.AssessmentType switch
            {
                "CRAFFT" => "The CRAFFT is a validated screening tool designed to identify adolescents and young adults who may have a substance use disorder or are at risk of developing one.",
                "ASSIST" => "The Alcohol, Smoking and Substance Involvement Screening Test (ASSIST) was developed by the World Health Organization to detect and manage substance use and related problems in primary and general medical care settings.",
                _ => "This is a validated screening tool designed to assess patterns of substance use."
            };
        }

        public List<string> AssessmentDetails
        {
            get
            {
                if (Assessment?.AssessmentType == "CRAFFT")
                {
                    return new List<string>
                    {
                        "C - Have you ever ridden in a CAR driven by someone (including yourself) who was 'high' or had been using alcohol or drugs?",
                        "R - Do you ever use alcohol or drugs to RELAX, feel better about yourself, or fit in?",
                        "A - Do you ever use alcohol/drugs while you are by yourself, ALONE?",
                        "F - Do you ever FORGET things you did while using alcohol or drugs?",
                        "F - Do your family or FRIENDS ever tell you that you should cut down on your drinking or drug use?",
                        "T - Have you gotten into TROUBLE while you were using alcohol or drugs?"
                    };
                }
                else if (Assessment?.AssessmentType == "ASSIST")
                {
                    return new List<string>
                    {
                        "Tobacco products",
                        "Alcoholic beverages",
                        "Cannabis (marijuana, pot, grass, hash, etc.)",
                        "Cocaine (coke, crack, etc.)",
                        "Amphetamine type stimulants",
                        "Inhalants (nitrous, glue, petrol, paint thinner, etc.)",
                        "Sedatives or sleeping pills",
                        "Hallucinogens",
                        "Opioids",
                        "Other drugs"
                    };
                }
                return new List<string>();
            }
        }

        private async void AssessmentInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
                return;
            IsLoaded = true;
            Console.WriteLine("[AssessmentInfoPage] Loaded: Starting assessment load.");
            await LoadAssessmentAsync();
        }

        private async Task LoadAssessmentAsync()
        {
            IsLoading = true;
            HasError = false;
            try
            {
                Console.WriteLine($"[LoadAssessmentAsync] Loading AssessmentId={AssessmentId}");
                Assessment = await _assessmentApiService.GetByIdAsync(AssessmentId);
                if (Assessment == null)
                {
                    Console.WriteLine($"[LoadAssessmentAsync] Assessment not found for AssessmentId={AssessmentId}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                Console.WriteLine($"[LoadAssessmentAsync] Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private async void RetryLoadAssessment_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[RetryLoadAssessment] Retrying assessment load.");
            await LoadAssessmentAsync();
        }

        private void StartAssessment_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[StartAssessment] Navigating to AssessmentId={AssessmentId}");
            NavigationService?.Navigate(new Uri($"/Views/AssessmentTakePage.xaml?assessmentId={AssessmentId}", UriKind.Relative));
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[GoBack] Navigating back to /Assessment");
            NavigationService?.Navigate(new Uri("/Views/Assessment.xaml", UriKind.Relative));
        }

        private string GetLanguageDisplay(string language)
        {
            return language switch
            {
                "eng" => "English",
                "vie" => "Tiếng Việt",
                _ => language.ToUpper()
            };
        }

        private string GetEstimatedDuration(string assessmentType)
        {
            return assessmentType switch
            {
                "CRAFFT" => "2-3 minutes",
                "ASSIST" => "5-10 minutes",
                _ => "3-5 minutes"
            };
        }

        private string GetDetailedDescription(string assessmentType)
        {
            return assessmentType switch
            {
                "CRAFFT" => "The CRAFFT is a validated screening instrument designed for use with adolescents and young adults to identify those who may have problematic substance use. It consists of six questions that cover various aspects of substance use behavior and its consequences.",
                "ASSIST" => "The Alcohol, Smoking and Substance Involvement Screening Test (ASSIST) is a comprehensive screening tool developed by the World Health Organization to identify people who may be at risk of developing problems related to their substance use.",
                _ => "This is a validated screening tool designed to assess patterns of substance use and identify individuals who may benefit from further evaluation or intervention."
            };
        }

        private string GetQuestionCount(string assessmentType)
        {
            return assessmentType switch
            {
                "CRAFFT" => "6",
                "ASSIST" => "8-10",
                _ => "Multiple"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.WriteLine($"[OnPropertyChanged] Property: {propertyName}");
        }
    }
}