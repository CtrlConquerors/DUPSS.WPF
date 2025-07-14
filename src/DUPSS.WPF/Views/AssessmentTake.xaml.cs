using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DUPSS.ApiClients;
using DUPSS.DTO.DTOs;

namespace DUPSS.WPF.Views
{
    public partial class AssessmentTake : INotifyPropertyChanged
    {
        private readonly AssessmentApiService _assessmentApiService;
        private AssessmentDTO? _assessment;
        private List<AssessmentQuestionDTO> _questions = new();
        private Dictionary<string, List<AssessmentAnswerDTO>> _questionAnswers = new();
        private Dictionary<string, string> _userAnswers = new();
        private AssessmentResultDTO? _assessmentResult;
        private bool _isLoading = true;
        private bool _hasError;
        private bool _isSubmitting;
        private bool _showResults;
        private bool _showExitConfirmation;
        private string _errorMessage = string.Empty;
        private int _currentQuestionIndex;
        private List<string> _selectedAnswerIds = new();
        private string? _currentMemberId;

        private readonly List<string> _substances = new()
        {
            "Tobacco", "Alcohol", "Cannabis", "Cocaine", "Amphetamines",
            "Inhalants", "Sedatives", "Hallucinogens", "Opioids", "Other"
        };

        public AssessmentTake(AssessmentApiService assessmentApiService, string assessmentId)
        {
            InitializeComponent();
            _assessmentApiService = assessmentApiService ?? throw new ArgumentNullException(nameof(assessmentApiService));
            AssessmentId = assessmentId;
            DataContext = this;
            SelectAnswerCommand = new RelayCommand(SelectAnswer, _ => true);
            Console.WriteLine($"[AssessmentTakePage] Constructor: AssessmentId={AssessmentId}");
            Loaded += AssessmentTakePage_Loaded;
        }

        public string AssessmentId { get; }

        public AssessmentDTO? Assessment
        {
            get => _assessment;
            private set
            {
                _assessment = value;
                OnPropertyChanged(nameof(Assessment));
                OnPropertyChanged(nameof(IsAssist));
                Console.WriteLine($"[Assessment] Updated: {(value != null ? $"AssessmentId={value.AssessmentId}" : "null")}");
            }
        }

        public List<AssessmentQuestionDTO> Questions
        {
            get => _questions;
            private set
            {
                _questions = value;
                OnPropertyChanged(nameof(Questions));
                Console.WriteLine($"[Questions] Updated: Count={value.Count}");
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

        public bool IsSubmitting
        {
            get => _isSubmitting;
            private set
            {
                _isSubmitting = value;
                OnPropertyChanged(nameof(IsSubmitting));
                OnPropertyChanged(nameof(NextButtonText));
                Console.WriteLine($"[IsSubmitting] Updated: {value}");
            }
        }

        public bool ShowResults
        {
            get => _showResults;
            private set
            {
                _showResults = value;
                OnPropertyChanged(nameof(ShowResults));
                Console.WriteLine($"[ShowResults] Updated: {value}");
            }
        }

        public bool ShowExitConfirmation
        {
            get => _showExitConfirmation;
            private set
            {
                _showExitConfirmation = value;
                OnPropertyChanged(nameof(ShowExitConfirmation));
                Console.WriteLine($"[ShowExitConfirmation] Updated: {value}");
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                Console.WriteLine($"[ErrorMessage] Updated: {value}");
            }
        }

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            private set
            {
                _currentQuestionIndex = value;
                OnPropertyChanged(nameof(CurrentQuestionIndex));
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentAnswers));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(NextButtonText));
                OnPropertyChanged(nameof(CurrentSubstance));
                Console.WriteLine($"[CurrentQuestionIndex] Updated: {value}, QuestionId={CurrentQuestion?.QuestionId}");
            }
        }

        public AssessmentQuestionDTO? CurrentQuestion => _questions.ElementAtOrDefault(_currentQuestionIndex);

        public List<AnswerViewModel> CurrentAnswers
        {
            get
            {
                var answers = _questionAnswers.GetValueOrDefault(CurrentQuestion?.QuestionId, new List<AssessmentAnswerDTO>());
                return answers.Select(a => new AnswerViewModel
                {
                    AnswerId = a.AnswerId,
                    Answer = a.Answer,
                    IsSelected = _selectedAnswerIds.Contains(a.AnswerId)
                }).ToList();
            }
        }

        public AssessmentResultDTO? AssessmentResult
        {
            get => _assessmentResult;
            private set
            {
                _assessmentResult = value;
                OnPropertyChanged(nameof(AssessmentResult));
                OnPropertyChanged(nameof(AssessmentResult.ScoreDetails)); // Changed from ScoreDetailsList
                Console.WriteLine($"[AssessmentResult] Updated: {(value != null ? $"ResultId={value.ResultId}" : "null")}");
            }
        }

        public new bool IsLoaded
        {
            get => !_isLoading;
            private set
            {
                _isLoading = !value;
                OnPropertyChanged(nameof(IsLoaded));
                Console.WriteLine($"[IsLoaded] Updated: {value}");
            }
        }

        public double ProgressPercentage
        {
            get => Questions.Any() ? ((double)(CurrentQuestionIndex + 1) / Questions.Count) * 100 : 0;
        }

        public bool CanGoPrevious => CurrentQuestionIndex > 0;

        public string NextButtonText
        {
            get
            {
                if (IsSubmitting) return "Submitting...";
                if (AssessmentId == "CRAFFT" && CurrentQuestion?.QuestionId == "CRAFFT_Q4" && IsCrafftQ1ToQ3AllZero())
                    return "Submit Assessment";
                return CurrentQuestionIndex < Questions.Count - 1 ? "Next" : "Submit Assessment";
            }
        }

        public bool IsAnswerSelected => _userAnswers.ContainsKey(CurrentQuestion?.QuestionId) && !string.IsNullOrEmpty(_userAnswers[CurrentQuestion.QuestionId]);

        public bool IsAssist => Assessment?.AssessmentType == "ASSIST";

        public string CurrentSubstance
        {
            get
            {
                if (AssessmentId != "ASSIST") return string.Empty;
                int substanceIndex = CurrentQuestionIndex / 8;
                return _substances.ElementAtOrDefault(substanceIndex) ?? string.Empty;
            }
        }

        public ICommand SelectAnswerCommand { get; }

        private async void AssessmentTakePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                return;
            IsLoaded = true;
            Console.WriteLine("[AssessmentTakePage] Loaded: Starting assessment load.");
            await LoadAssessmentAsync();
        }

        private async Task LoadAssessmentAsync()
        {
            _isLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            try
            {
                Console.WriteLine($"[LoadAssessmentAsync] Loading AssessmentId={AssessmentId}");
                Assessment = await _assessmentApiService.GetByIdAsync(AssessmentId);
                if (Assessment == null)
                {
                    HasError = true;
                    ErrorMessage = "Assessment not found.";
                    Console.WriteLine($"[LoadAssessmentAsync] Assessment not found for AssessmentId={AssessmentId}");
                    return;
                }

                if (AssessmentId == "ASSIST")
                {
                    LoadAssistQuestions();
                }
                else if (AssessmentId == "CRAFFT")
                {
                    LoadCrafftQuestions();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Unsupported assessment type.";
                    Console.WriteLine("[LoadAssessmentAsync] Unsupported assessment type.");
                    return;
                }

                if (!Questions.Any())
                {
                    HasError = true;
                    ErrorMessage = "No questions defined for this assessment.";
                    Console.WriteLine("[LoadAssessmentAsync] No questions defined.");
                    return;
                }

                foreach (var question in Questions)
                {
                    _userAnswers[question.QuestionId] = question.QuestionId.EndsWith("_Q1") ? $"{question.QuestionId}_NO" : string.Empty;
                }

                LoadCurrentQuestionAnswers();
                Console.WriteLine($"[LoadAssessmentAsync] Successfully loaded {Assessment.AssessmentType} with {Questions.Count} questions");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Failed to load assessment: {ex.Message}";
                Console.WriteLine($"[LoadAssessmentAsync] Error: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                OnPropertyChanged(nameof(_isLoading));
            }
        }

        private void LoadAssistQuestions()
        {
            Questions = new List<AssessmentQuestionDTO>();
            _questionAnswers = new Dictionary<string, List<AssessmentAnswerDTO>>();

            var frequencyAnswers = new List<AssessmentAnswerDTO>
            {
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_fA_0", Answer = "Never", ScoreValue = 0 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_fA_2", Answer = "Once or twice", ScoreValue = 2 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_fA_3", Answer = "Monthly", ScoreValue = 3 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_fA_4", Answer = "Weekly", ScoreValue = 4 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_fA_6", Answer = "Daily or almost daily", ScoreValue = 6 }
            };

            var yesNoAnswers = new List<AssessmentAnswerDTO>
            {
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_yNA_yes", Answer = "Yes", ScoreValue = 0 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_yNA_no", Answer = "No", ScoreValue = 0 }
            };

            var injectionAnswers = new List<AssessmentAnswerDTO>
            {
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_iA_0", Answer = "Never", ScoreValue = 0 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_iA_6", Answer = "Yes, in the past 3 months", ScoreValue = 6 },
                new() { QuestionId = "ASSIST", AnswerId = "ASSIST_iA_3", Answer = "Yes, but not in the past 3 months", ScoreValue = 3 }
            };

            foreach (var substance in _substances)
            {
                var substanceId = substance.ToUpper().Replace(" ", "_");
                var substanceQuestions = new List<AssessmentQuestionDTO>
                {
                    new() { QuestionId = $"ASSIST_{substanceId}_Q1", AssessmentId = AssessmentId, Question = $"In your life, have you ever used {substance.ToLower()}?", QuestionType = "YesNo" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q2", AssessmentId = AssessmentId, Question = $"In the past 3 months, how often have you used {substance.ToLower()}?", QuestionType = "MultipleChoice" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q3", AssessmentId = AssessmentId, Question = $"In the past 3 months, how often have you had a strong desire or urge to use {substance.ToLower()}?", QuestionType = "MultipleChoice" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q4", AssessmentId = AssessmentId, Question = $"In the past 3 months, how often has {substance.ToLower()} use led to health, social, legal, or financial problems?", QuestionType = "MultipleChoice" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q5", AssessmentId = AssessmentId, Question = $"In the past 3 months, how often have you failed to do what was normally expected of you because of {substance.ToLower()}?", QuestionType = "MultipleChoice" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q6", AssessmentId = AssessmentId, Question = $"Has a friend or relative or anyone else ever expressed concern about your use of {substance.ToLower()}?", QuestionType = "YesNo" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q7", AssessmentId = AssessmentId, Question = $"Have you ever tried and failed to control, cut down, or stop using {substance.ToLower()}?", QuestionType = "YesNo" },
                    new() { QuestionId = $"ASSIST_{substanceId}_Q8", AssessmentId = AssessmentId, Question = $"Have you ever used {substance.ToLower()} by injection?", QuestionType = "MultipleChoice" }
                };

                Questions.AddRange(substanceQuestions);

                _questionAnswers[$"ASSIST_{substanceId}_Q1"] = yesNoAnswers.Select(a => new AssessmentAnswerDTO
                {
                    AnswerId = $"ASSIST_{substanceId}_Q1_{a.Answer.ToUpper()}",
                    QuestionId = $"ASSIST_{substanceId}_Q1",
                    Answer = a.Answer,
                    ScoreValue = a.ScoreValue
                }).ToList();

                for (int i = 2; i <= 5; i++)
                {
                    _questionAnswers[$"ASSIST_{substanceId}_Q{i}"] = frequencyAnswers.Select(a => new AssessmentAnswerDTO
                    {
                        AnswerId = $"ASSIST_{substanceId}_Q{i}_{a.Answer.ToUpper().Replace(" ", "_")}",
                        QuestionId = $"ASSIST_{substanceId}_Q{i}",
                        Answer = a.Answer,
                        ScoreValue = a.ScoreValue
                    }).ToList();
                }

                _questionAnswers[$"ASSIST_{substanceId}_Q6"] = yesNoAnswers.Select(a => new AssessmentAnswerDTO
                {
                    AnswerId = $"ASSIST_{substanceId}_Q6_{a.Answer.ToUpper()}",
                    QuestionId = $"ASSIST_{substanceId}_Q6",
                    Answer = a.Answer,
                    ScoreValue = a.Answer == "Yes" ? 7 : 0
                }).ToList();

                _questionAnswers[$"ASSIST_{substanceId}_Q7"] = yesNoAnswers.Select(a => new AssessmentAnswerDTO
                {
                    AnswerId = $"ASSIST_{substanceId}_Q7_{a.Answer.ToUpper()}",
                    QuestionId = $"ASSIST_{substanceId}_Q7",
                    Answer = a.Answer,
                    ScoreValue = a.Answer == "Yes" ? 4 : 0
                }).ToList();

                _questionAnswers[$"ASSIST_{substanceId}_Q8"] = injectionAnswers.Select(a => new AssessmentAnswerDTO
                {
                    AnswerId = $"ASSIST_{substanceId}_Q8_{a.Answer.ToUpper().Replace(" ", "_").Replace(",", "")}",
                    QuestionId = $"ASSIST_{substanceId}_Q8",
                    Answer = a.Answer,
                    ScoreValue = a.ScoreValue
                }).ToList();
            }

            Console.WriteLine($"[LoadAssistQuestions] Loaded {Questions.Count} ASSIST questions, {_questionAnswers.Count} answer sets");
        }

        private void LoadCrafftQuestions()
        {
            Questions = new List<AssessmentQuestionDTO>
            {
                new() { QuestionId = "CRAFFT_Q1", AssessmentId = AssessmentId, Question = "During the PAST 12 MONTHS, on how many days did you drink more than a few sips of beer, wine, or any drink containing alcohol? Put '0' if none.", QuestionType = "Numeric" },
                new() { QuestionId = "CRAFFT_Q2", AssessmentId = AssessmentId, Question = "During the PAST 12 MONTHS, on how many days did you use any marijuana (cannabis, weed, oil, wax, or hash by smoking, vaping, dabbing, or in edibles) or 'synthetic marijuana' (like 'K2,' 'Spice')? Put '0' if none.", QuestionType = "Numeric" },
                new() { QuestionId = "CRAFFT_Q3", AssessmentId = AssessmentId, Question = "During the PAST 12 MONTHS, on how many days did you use anything else to get high (like other illegal drugs, pills, prescription or over-the-counter medications, and things that you sniff, huff, vape, or inject)? Put '0' if none.", QuestionType = "Numeric" },
                new() { QuestionId = "CRAFFT_Q4", AssessmentId = AssessmentId, Question = "Have you ever ridden in a CAR driven by someone (including yourself) who was high or had been using alcohol or drugs?", QuestionType = "YesNo" },
                new() { QuestionId = "CRAFFT_Q5", AssessmentId = AssessmentId, Question = "Do you ever use alcohol or drugs to RELAX, feel better about yourself, or fit in?", QuestionType = "YesNo" },
                new() { QuestionId = "CRAFFT_Q6", AssessmentId = AssessmentId, Question = "Do you ever use alcohol or drugs while you are by yourself, ALONE?", QuestionType = "YesNo" },
                new() { QuestionId = "CRAFFT_Q7", AssessmentId = AssessmentId, Question = "Do you ever FORGET things you did while using alcohol or drugs?", QuestionType = "YesNo" },
                new() { QuestionId = "CRAFFT_Q8", AssessmentId = AssessmentId, Question = "Do your family or FRIENDS ever tell you that you should cut down on your drinking or drug use?", QuestionType = "YesNo" },
                new() { QuestionId = "CRAFFT_Q9", AssessmentId = AssessmentId, Question = "Have you ever gotten into TROUBLE while you were using alcohol or drugs?", QuestionType = "YesNo" }
            };

            _questionAnswers = new Dictionary<string, List<AssessmentAnswerDTO>>
            {
                { "CRAFFT_Q1", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q1_0", QuestionId = "CRAFFT_Q1", Answer = "0", ScoreValue = 0 }, new() { AnswerId = "CRAFFT_Q1_1PLUS", QuestionId = "CRAFFT_Q1", Answer = "1 or more", ScoreValue = 0 } } },
                { "CRAFFT_Q2", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q2_0", QuestionId = "CRAFFT_Q2", Answer = "0", ScoreValue = 0 }, new() { AnswerId = "CRAFFT_Q2_1PLUS", QuestionId = "CRAFFT_Q2", Answer = "1 or more", ScoreValue = 0 } } },
                { "CRAFFT_Q3", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q3_0", QuestionId = "CRAFFT_Q3", Answer = "0", ScoreValue = 0 }, new() { AnswerId = "CRAFFT_Q3_1PLUS", QuestionId = "CRAFFT_Q3", Answer = "1 or more", ScoreValue = 0 } } },
                { "CRAFFT_Q4", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q4_YES", QuestionId = "CRAFFT_Q4", Answer = "Yes", ScoreValue = 1 }, new() { AnswerId = "CRAFFT_Q4_NO", QuestionId = "CRAFFT_Q4", Answer = "No", ScoreValue = 0 } } },
                { "CRAFFT_Q5", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q5_YES", QuestionId = "CRAFFT_Q5", Answer = "Yes", ScoreValue = 1 }, new() { AnswerId = "CRAFFT_Q5_NO", QuestionId = "CRAFFT_Q5", Answer = "No", ScoreValue = 0 } } },
                { "CRAFFT_Q6", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q6_YES", QuestionId = "CRAFFT_Q6", Answer = "Yes", ScoreValue = 1 }, new() { AnswerId = "CRAFFT_Q6_NO", QuestionId = "CRAFFT_Q6", Answer = "No", ScoreValue = 0 } } },
                { "CRAFFT_Q7", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q7_YES", QuestionId = "CRAFFT_Q7", Answer = "Yes", ScoreValue = 1 }, new() { AnswerId = "CRAFFT_Q7_NO", QuestionId = "CRAFFT_Q7", Answer = "No", ScoreValue = 0 } } },
                { "CRAFFT_Q8", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q8_YES", QuestionId = "CRAFFT_Q8", Answer = "Yes", ScoreValue = 1 }, new() { AnswerId = "CRAFFT_Q8_NO", QuestionId = "CRAFFT_Q8", Answer = "No", ScoreValue = 0 } } },
                { "CRAFFT_Q9", new List<AssessmentAnswerDTO> { new() { AnswerId = "CRAFFT_Q9_YES", QuestionId = "CRAFFT_Q9", Answer = "Yes", ScoreValue = 1 }, new() { AnswerId = "CRAFFT_Q9_NO", QuestionId = "CRAFFT_Q9", Answer = "No", ScoreValue = 0 } } }
            };

            Console.WriteLine($"[LoadCrafftQuestions] Loaded {Questions.Count} CRAFFT questions, {_questionAnswers.Count} answer sets");
        }

        private void LoadCurrentQuestionAnswers()
        {
            _selectedAnswerIds = _userAnswers.ContainsKey(CurrentQuestion?.QuestionId) && !string.IsNullOrEmpty(_userAnswers[CurrentQuestion.QuestionId])
                ? new List<string> { _userAnswers[CurrentQuestion.QuestionId] }
                : new List<string>();
            OnPropertyChanged(nameof(CurrentAnswers));
            Console.WriteLine($"[LoadCurrentQuestionAnswers] Loaded answers for QuestionId={CurrentQuestion?.QuestionId}, SelectedAnswerIds={string.Join(",", _selectedAnswerIds)}");
        }

        private void SelectAnswer(object parameter)
        {
            if (parameter is string answerId && CurrentQuestion != null)
            {
                _selectedAnswerIds = new List<string> { answerId };
                _userAnswers[CurrentQuestion.QuestionId] = answerId;

                if (AssessmentId == "ASSIST" && CurrentQuestion.QuestionId.EndsWith("_Q1") && answerId.EndsWith("_NO"))
                {
                    var substanceId = CurrentQuestion.QuestionId.Split('_')[1];
                    for (int i = 2; i <= 7; i++)
                    {
                        _userAnswers[$"ASSIST_{substanceId}_Q{i}"] = $"ASSIST_{substanceId}_Q{i}_NO";
                    }
                    _userAnswers[$"ASSIST_{substanceId}_Q8"] = $"ASSIST_{substanceId}_Q8_NEVER";
                    Console.WriteLine($"[SelectAnswer] ASSIST Q1 answered 'NO' for {substanceId}, auto-set Q2-Q8");
                }

                OnPropertyChanged(nameof(CurrentAnswers));
                OnPropertyChanged(nameof(IsAnswerSelected));
                Console.WriteLine($"[SelectAnswer] Selected AnswerId={answerId} for QuestionId={CurrentQuestion.QuestionId}");
            }
        }

        private bool IsCrafftQ1ToQ3AllZero()
        {
            bool allZero = _userAnswers.GetValueOrDefault("CRAFFT_Q1", "").EndsWith("_0") &&
                           _userAnswers.GetValueOrDefault("CRAFFT_Q2", "").EndsWith("_0") &&
                           _userAnswers.GetValueOrDefault("CRAFFT_Q3", "").EndsWith("_0");
            Console.WriteLine($"[IsCrafftQ1ToQ3AllZero] Result: {allZero}");
            return allZero;
        }

        private async void NextOrSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAnswerSelected || IsSubmitting)
                return;

            SaveCurrentAnswer();
            if (AssessmentId == "CRAFFT" && CurrentQuestion?.QuestionId == "CRAFFT_Q4" && IsCrafftQ1ToQ3AllZero())
            {
                Console.WriteLine("[NextOrSubmit] CRAFFT Q4, Q1-Q3 all '0', submitting");
                await SubmitAssessmentAsync();
                return;
            }

            if (CurrentQuestionIndex < Questions.Count - 1)
            {
                if (AssessmentId == "ASSIST" && (bool)CurrentQuestion?.QuestionId.EndsWith("_Q1") && _userAnswers[CurrentQuestion.QuestionId].EndsWith("_NO"))
                {
                    int substanceIndex = CurrentQuestionIndex / 8;
                    CurrentQuestionIndex = (substanceIndex + 1) * 8;
                    if (CurrentQuestionIndex >= Questions.Count)
                    {
                        CurrentQuestionIndex = Questions.Count - 1;
                    }
                }
                else
                {
                    CurrentQuestionIndex++;
                }
                Console.WriteLine($"[NextOrSubmit] Navigating to QuestionId={Questions[CurrentQuestionIndex].QuestionId}, Index={CurrentQuestionIndex}");
                LoadCurrentQuestionAnswers();
            }
            else
            {
                Console.WriteLine("[NextOrSubmit] Submitting assessment");
                await SubmitAssessmentAsync();
            }
        }

        private void PreviousQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentQuestionIndex <= 0)
                return;

            SaveCurrentAnswer();
            if (AssessmentId == "ASSIST" && (bool)CurrentQuestion?.QuestionId.EndsWith("_Q1"))
            {
                int substanceIndex = CurrentQuestionIndex / 8;
                if (substanceIndex > 0)
                {
                    var prevSubstanceIndex = substanceIndex - 1;
                    var prevSubstanceQ1Id = $"ASSIST_{_substances[prevSubstanceIndex].ToUpper().Replace(" ", "_")}_Q1";
                    var prevQ1Answer = _userAnswers.GetValueOrDefault(prevSubstanceQ1Id, string.Empty);
                    CurrentQuestionIndex = prevQ1Answer.EndsWith("_NO") ? prevSubstanceIndex * 8 : (prevSubstanceIndex * 8) + 7;
                }
                else
                {
                    CurrentQuestionIndex = 0;
                }
            }
            else
            {
                CurrentQuestionIndex--;
            }

            Console.WriteLine($"[PreviousQuestion] Navigating to QuestionId={Questions[CurrentQuestionIndex].QuestionId}, Index={CurrentQuestionIndex}");
            LoadCurrentQuestionAnswers();
        }

        private async void RetryLoadAssessment_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[RetryLoadAssessment] Retrying assessment load.");
            await LoadAssessmentAsync();
        }

        private void ConfirmExit_Click(object sender, RoutedEventArgs e)
        {
            ShowExitConfirmation = true;
            Console.WriteLine("[ConfirmExit] Showing exit confirmation.");
        }

        private void CancelExit_Click(object sender, RoutedEventArgs e)
        {
            ShowExitConfirmation = false;
            Console.WriteLine("[CancelExit] Exit confirmation cancelled.");
        }

        private void ExitAssessment_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[ExitAssessment] Navigating back to /Assessment");
            NavigationService?.Navigate(new Uri("/Views/Assessment.xaml", UriKind.Relative));
        }

        private void GoBackToAssessments_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[GoBackToAssessments] Navigating back to /Assessment");
            NavigationService?.Navigate(new Uri("/Views/Assessment.xaml", UriKind.Relative));
        }

        private void TakeAnotherAssessment_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[TakeAnotherAssessment] Navigating back to /Assessment");
            NavigationService?.Navigate(new Uri("/Views/Assessment.xaml", UriKind.Relative));
        }

        private void SaveCurrentAnswer()
        {
            if (CurrentQuestion != null)
            {
                _userAnswers[CurrentQuestion.QuestionId] = _selectedAnswerIds.FirstOrDefault() ?? string.Empty;
                Console.WriteLine($"[SaveCurrentAnswer] Saved AnswerId={_userAnswers[CurrentQuestion.QuestionId]} for QuestionId={CurrentQuestion.QuestionId}");
            }
        }

        private async Task SubmitAssessmentAsync()
        {
            if (!IsAnswerSelected || IsSubmitting)
                return;

            SaveCurrentAnswer();
            IsSubmitting = true;
            try
            {
                if (string.IsNullOrEmpty(_currentMemberId))
                {
                    _currentMemberId = GetCurrentMemberId();
                    if (string.IsNullOrEmpty(_currentMemberId))
                    {
                        HasError = true;
                        ErrorMessage = "Failed to retrieve Member ID. Please login again.";
                        Console.WriteLine("[SubmitAssessment] Failed to retrieve MemberId.");
                        MessageBox.Show("Failed to retrieve Member ID. Please login again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService?.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
                        return;
                    }
                    Console.WriteLine($"[SubmitAssessment] Retrieved MemberId: {_currentMemberId}");
                }

                // Create AssessmentSubmissionDTO
                var submission = new AssessmentSubmissionDTO
                {
                    MemberId = _currentMemberId,
                    Answers = new List<AnswerSubmissionDTO>(),
                    TextAnswers = new List<TextAnswerSubmissionDTO>(),
                    EarlyTextScore = 0
                };

                if (AssessmentId == "ASSIST")
                {
                    foreach (var substance in _substances)
                    {
                        var substanceId = substance.ToUpper().Replace(" ", "_");
                        for (int i = 1; i <= 8; i++)
                        {
                            var questionId = $"ASSIST_{substanceId}_Q{i}";
                            if (_userAnswers.TryGetValue(questionId, out var answerId) && !string.IsNullOrEmpty(answerId))
                            {
                                submission.Answers.Add(new AnswerSubmissionDTO { AnswerId = answerId });
                                Console.WriteLine($"[SubmitAssessment] Added ASSIST answer: QuestionId={questionId}, AnswerId={answerId}");
                            }
                        }
                    }
                }
                else if (AssessmentId == "CRAFFT")
                {
                    bool allZero = IsCrafftQ1ToQ3AllZero();
                    foreach (var question in Questions)
                    {
                        if (!question.QuestionId.StartsWith("CRAFFT_Q") || (allZero && question.QuestionId != "CRAFFT_Q4"))
                            continue;
                        if (!_userAnswers.TryGetValue(question.QuestionId, out var answerId) || string.IsNullOrEmpty(answerId))
                            continue;

                        if (question.QuestionId is "CRAFFT_Q1" or "CRAFFT_Q2" or "CRAFFT_Q3")
                        {
                            var answer = _questionAnswers[question.QuestionId].FirstOrDefault(a => a.AnswerId == answerId)?.Answer;
                            submission.TextAnswers.Add(new TextAnswerSubmissionDTO
                            {
                                QuestionId = question.QuestionId,
                                Answer = answer ?? "0"
                            });
                            Console.WriteLine($"[SubmitAssessment] Added CRAFFT text answer: QuestionId={question.QuestionId}, Answer={answer}");
                        }
                        else
                        {
                            submission.Answers.Add(new AnswerSubmissionDTO { AnswerId = answerId });
                            Console.WriteLine($"[SubmitAssessment] Added CRAFFT answer: QuestionId={question.QuestionId}, AnswerId={answerId}");
                        }
                    }
                }

                // Calculate EarlyTextScore for CRAFFT (sum of Q1-Q3 numeric answers)
                if (AssessmentId == "CRAFFT")
                {
                    submission.EarlyTextScore = submission.TextAnswers
                        .Where(ta => ta.QuestionId is "CRAFFT_Q1" or "CRAFFT_Q2" or "CRAFFT_Q3")
                        .Sum(ta => int.TryParse(ta.Answer, out var score) ? score : 0);
                    Console.WriteLine($"[SubmitAssessment] CRAFFT EarlyTextScore: {submission.EarlyTextScore}");
                }

                // Submit to API
                Console.WriteLine($"[SubmitAssessment] Submitting: MemberId={submission.MemberId}, AnswerCount={submission.Answers.Count}, TextAnswerCount={submission.TextAnswers.Count}");
                var result = await _assessmentApiService.SubmitAssessmentAsync(AssessmentId, submission);
                if (result != null)
                {
                    // Map API result to AssessmentResultDTO
                    AssessmentResult = new AssessmentResultDTO
                    {
                        ResultId = result.ResultId,
                        AssessmentId = AssessmentId,
                        MemberId = _currentMemberId,
                        TotalScore = result.TotalScore,
                        ScoreDetails = result.ScoreDetails,
                        Recommendation = result.Recommendation,
                        CompletedOn = DateOnly.FromDateTime(DateTime.Now)
                    };
                    ShowResults = true;
                    Console.WriteLine($"[SubmitAssessment] Assessment submitted successfully. ResultId: {result.ResultId}");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Failed to submit assessment. Please check your answers and try again.";
                    Console.WriteLine("[SubmitAssessment] Submission failed.");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Failed to submit assessment: {ex.Message}";
                Console.WriteLine($"[SubmitAssessment] Exception: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private string GetCurrentMemberId()
        {
            // Placeholder: Implement actual authentication logic
            return "SampleMemberId"; // Replace with actual authentication mechanism
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.WriteLine($"[OnPropertyChanged] Property: {propertyName}");
        }
    }

    public class AnswerViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string AnswerId { get; set; }
        public string Answer { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    file class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    // Converters (Add these to a separate file or resource dictionary)
    file class BooleanToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndexToOneBasedConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int index)
            {
                return index + 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}