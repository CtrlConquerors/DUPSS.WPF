using DUPSS.ApiClients;
using DUPSS.Common;
using DUPSS.DTO.DTOs;
using System.Security.Claims;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DUPSS.WPF.Views
{
    public partial class AssessmentTake : Page
    {
        private readonly AssessmentApiService _assessmentService;
        private readonly JwtAuthenticationStateProvider _authStateProvider;
        private AssessmentDTO? _assessment;
        private List<AssessmentQuestionDTO> _questions = new();
        private Dictionary<string, List<AssessmentAnswerDTO>> _questionAnswers = new();
        private Dictionary<string, string> _userAnswers = new();
        private AssessmentResultDTO? _assessmentResult;
        private bool _isLoading = true;
        private bool _hasError = false;
        private bool _isSubmitting = false;
        private bool _showResults = false;
        private bool _showExitConfirmation = false;
        private string _errorMessage = string.Empty;
        private int _currentQuestionIndex = 0;
        private List<string> _selectedAnswerIds = new();
        private string? _currentMemberId;
        

        private readonly List<string> _substances = new()
        {
            "Tobacco", "Alcohol", "Cannabis", "Cocaine", "Amphetamines",
            "Inhalants", "Sedatives", "Hallucinogens", "Opioids", "Other"
        };

        public AssessmentTake(string assessmentId)
        {
            InitializeComponent();
            _assessmentService = App.AssessmentApiService;
            AssessmentId = assessmentId;
            
            var httpClient = App.HttpClient;
            
            var storage = new WpfSecureStorageService();
            _authStateProvider = new JwtAuthenticationStateProvider(new AuthApiService(httpClient), storage);
        }

        public string AssessmentId { get; set; } = string.Empty;

        private AssessmentQuestionDTO CurrentQuestion => _questions[_currentQuestionIndex];
        private List<AssessmentAnswerDTO> CurrentAnswers => _questionAnswers.GetValueOrDefault(CurrentQuestion.QuestionId, new List<AssessmentAnswerDTO>());

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMemberIdAsync();
            await LoadAssessmentAsync();
        }
        
        private async Task LoadMemberIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                _currentMemberId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        private async Task LoadAssessmentAsync()
        {
            _isLoading = true;
            _hasError = false;
            _errorMessage = string.Empty;
            ShowPanels(loading: true);

            try
            {
                Console.WriteLine($"Loading assessment with ID: {AssessmentId}");

                // Load assessment metadata
                _assessment = await _assessmentService.GetByIdAsync(AssessmentId);
                if (_assessment == null)
                {
                    _hasError = true;
                    _errorMessage = "Assessment not found.";
                    ShowPanels(error: true);
                    return;
                }

                // Load questions and answers
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
                    _hasError = true;
                    _errorMessage = "Unsupported assessment type.";
                    ShowPanels(error: true);
                    return;
                }

                if (!_questions.Any())
                {
                    _hasError = true;
                    _errorMessage = "No questions defined for this assessment.";
                    ShowPanels(noContent: true);
                    return;
                }

                // Initialize user answers with default "No" for Q1 to allow skipping
                foreach (var question in _questions)
                {
                    if (question.QuestionId.EndsWith("_Q1"))
                    {
                        _userAnswers[question.QuestionId] = $"{question.QuestionId}_NO";
                    }
                    else
                    {
                        _userAnswers[question.QuestionId] = string.Empty;
                    }
                }

                LoadCurrentQuestionAnswers();
                UpdateAssessmentUI();
                Console.WriteLine($"Successfully loaded {_assessment.AssessmentType} with {_questions.Count} questions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assessment: {ex.Message}");
                _hasError = true;
                _errorMessage = $"Failed to load assessment: {ex.Message}";
                ShowPanels(error: true);
            }
            finally
            {
                _isLoading = false;
                ShowPanels();
            }
        }

        private void LoadAssistQuestions()
        {
            _questions = new List<AssessmentQuestionDTO>();
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

                _questions.AddRange(substanceQuestions);

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

            Console.WriteLine($"Loaded ASSIST questions: {_questions.Count} total, {_questionAnswers.Count} question-answer sets");
        }

        private void LoadCrafftQuestions()
        {
            _questions = new List<AssessmentQuestionDTO>
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
                {
                    "CRAFFT_Q1", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q1_0", QuestionId = "CRAFFT_Q1", Answer = "0", ScoreValue = 0 },
                        new() { AnswerId = "CRAFFT_Q1_1PLUS", QuestionId = "CRAFFT_Q1", Answer = "1 or more", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q2", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q2_0", QuestionId = "CRAFFT_Q2", Answer = "0", ScoreValue = 0 },
                        new() { AnswerId = "CRAFFT_Q2_1PLUS", QuestionId = "CRAFFT_Q2", Answer = "1 or more", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q3", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q3_0", QuestionId = "CRAFFT_Q3", Answer = "0", ScoreValue = 0 },
                        new() { AnswerId = "CRAFFT_Q3_1PLUS", QuestionId = "CRAFFT_Q3", Answer = "1 or more", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q4", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q4_YES", QuestionId = "CRAFFT_Q4", Answer = "Yes", ScoreValue = 1 },
                        new() { AnswerId = "CRAFFT_Q4_NO", QuestionId = "CRAFFT_Q4", Answer = "No", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q5", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q5_YES", QuestionId = "CRAFFT_Q5", Answer = "Yes", ScoreValue = 1 },
                        new() { AnswerId = "CRAFFT_Q5_NO", QuestionId = "CRAFFT_Q5", Answer = "No", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q6", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q6_YES", QuestionId = "CRAFFT_Q6", Answer = "Yes", ScoreValue = 1 },
                        new() { AnswerId = "CRAFFT_Q6_NO", QuestionId = "CRAFFT_Q6", Answer = "No", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q7", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q7_YES", QuestionId = "CRAFFT_Q7", Answer = "Yes", ScoreValue = 1 },
                        new() { AnswerId = "CRAFFT_Q7_NO", QuestionId = "CRAFFT_Q7", Answer = "No", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q8", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q8_YES", QuestionId = "CRAFFT_Q8", Answer = "Yes", ScoreValue = 1 },
                        new() { AnswerId = "CRAFFT_Q8_NO", QuestionId = "CRAFFT_Q8", Answer = "No", ScoreValue = 0 }
                    }
                },
                {
                    "CRAFFT_Q9", new List<AssessmentAnswerDTO>
                    {
                        new() { AnswerId = "CRAFFT_Q9_YES", QuestionId = "CRAFFT_Q9", Answer = "Yes", ScoreValue = 1 },
                        new() { AnswerId = "CRAFFT_Q9_NO", QuestionId = "CRAFFT_Q9", Answer = "No", ScoreValue = 0 }
                    }
                }
            };

            Console.WriteLine($"Loaded {_questions.Count} CRAFFT questions, {_questionAnswers.Count} answer sets.");
        }

        private void LoadCurrentQuestionAnswers()
        {
            _selectedAnswerIds = _userAnswers.ContainsKey(CurrentQuestion.QuestionId) && !string.IsNullOrEmpty(_userAnswers[CurrentQuestion.QuestionId])
                ? new List<string> { _userAnswers[CurrentQuestion.QuestionId] }
                : new List<string>();
            UpdateQuestionUI();
        }

        private void UpdateAssessmentUI()
        {
            if (_assessment == null || !_questions.Any())
            {
                ShowPanels(noContent: true);
                return;
            }

            AssessmentType.Text = _assessment.AssessmentType;
            AssessmentDescription.Text = _assessment.Description;
            SubstanceInfo.Visibility = AssessmentId == "ASSIST" ? Visibility.Visible : Visibility.Collapsed;
            if (AssessmentId == "ASSIST")
            {
                int substanceIndex = _currentQuestionIndex / 8;
                SubstanceInfo.Text = $"Current Section: {_substances[substanceIndex]}";
            }

            ProgressText.Text = $"Question {_currentQuestionIndex + 1} of {_questions.Count}";
            ProgressPercentage.Text = $"{Math.Round(((double)(_currentQuestionIndex + 1) / _questions.Count) * 100, 0)}%";
            ProgressBar.Value = ((double)(_currentQuestionIndex + 1) / _questions.Count) * 100;

            UpdateQuestionUI();
            PreviousButton.IsEnabled = _currentQuestionIndex > 0;
            NextButton.Visibility = _currentQuestionIndex < _questions.Count - 1 ? Visibility.Visible : Visibility.Collapsed;
            SubmitButton.Visibility = _currentQuestionIndex == _questions.Count - 1 || (AssessmentId == "CRAFFT" && CurrentQuestion.QuestionId == "CRAFFT_Q4" && IsCrafftQ1ToQ3AllZero()) ? Visibility.Visible : Visibility.Collapsed;
            NextButton.IsEnabled = IsAnswerSelected();
            SubmitButton.IsEnabled = IsAnswerSelected() && !_isSubmitting;

            ShowPanels(assessment: true);
        }

        private void UpdateQuestionUI()
        {
            QuestionHeader.Text = $"Question {_currentQuestionIndex + 1}";
            QuestionText.Text = CurrentQuestion.Question;
            AnswersContainer.Children.Clear();

            foreach (var answer in CurrentAnswers)
            {
                var radio = new RadioButton
                {
                    Content = answer.Answer,
                    GroupName = $"answer-{CurrentQuestion.QuestionId}",
                    Tag = answer.AnswerId,
                    IsChecked = _selectedAnswerIds.Contains(answer.AnswerId)
                };
                radio.Checked += (s, e) => SelectSingleAnswer(answer.AnswerId);
                var stackPanel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
                stackPanel.Children.Add(radio);
                if (!string.IsNullOrEmpty(answer.AnswerDetails))
                {
                    stackPanel.Children.Add(new TextBlock { Text = answer.AnswerDetails, Foreground = Brushes.Gray, Margin = new Thickness(20, 0, 0, 0) });
                }
                AnswersContainer.Children.Add(stackPanel);
            }
        }

        private void SelectSingleAnswer(string answerId)
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
            }

            UpdateAssessmentUI();
        }

        private bool IsCrafftQ1ToQ3AllZero()
        {
            return _userAnswers.GetValueOrDefault("CRAFFT_Q1", "").EndsWith("_0") &&
                   _userAnswers.GetValueOrDefault("CRAFFT_Q2", "").EndsWith("_0") &&
                   _userAnswers.GetValueOrDefault("CRAFFT_Q3", "").EndsWith("_0");
        }

        private bool IsAnswerSelected()
        {
            return _userAnswers.ContainsKey(CurrentQuestion.QuestionId) && !string.IsNullOrEmpty(_userAnswers[CurrentQuestion.QuestionId]);
        }

        private void SaveCurrentAnswer()
        {
            _userAnswers[CurrentQuestion.QuestionId] = _selectedAnswerIds.FirstOrDefault() ?? string.Empty;
        }

        private async void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex >= _questions.Count - 1 || !IsAnswerSelected())
                return;
            SaveCurrentAnswer();

            if (AssessmentId == "ASSIST" && CurrentQuestion.QuestionId.EndsWith("_Q1") && _userAnswers[CurrentQuestion.QuestionId].EndsWith("_NO"))
            {
                int substanceIndex = _currentQuestionIndex / 8;
                _currentQuestionIndex = (substanceIndex + 1) * 8;
                if (_currentQuestionIndex >= _questions.Count)
                {
                    _currentQuestionIndex = _questions.Count - 1;
                }
            }
            else if (AssessmentId == "CRAFFT" && CurrentQuestion.QuestionId == "CRAFFT_Q4")
            {
                if (IsCrafftQ1ToQ3AllZero())
                {
                    Console.WriteLine("CRAFFT Q1-Q3 all '0', submitting after Q4");
                    await SubmitAssessmentAsync();
                    return;
                }
                _currentQuestionIndex++;
                Console.WriteLine($"CRAFFT Q4 answered, Q1-Q3 not all '0', navigating to Q5, Index: {_currentQuestionIndex}, QuestionId: {_questions[_currentQuestionIndex].QuestionId}");
            }
            else
            {
                _currentQuestionIndex++;
            }

            Console.WriteLine($"Navigating to QuestionId: {_questions[_currentQuestionIndex].QuestionId}, Index: {_currentQuestionIndex}");
            LoadCurrentQuestionAnswers();
            UpdateAssessmentUI();
        }

        private void PreviousQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex <= 0)
                return;
            SaveCurrentAnswer();

            if (AssessmentId == "ASSIST" && CurrentQuestion.QuestionId.EndsWith("_Q1"))
            {
                int substanceIndex = _currentQuestionIndex / 8;
                if (substanceIndex > 0)
                {
                    var prevSubstanceIndex = substanceIndex - 1;
                    var prevSubstanceQ1Id = $"ASSIST_{_substances[prevSubstanceIndex].ToUpper().Replace(" ", "_")}_Q1";
                    var prevQ1Answer = _userAnswers.GetValueOrDefault(prevSubstanceQ1Id, string.Empty);
                    _currentQuestionIndex = prevQ1Answer.EndsWith("_NO") ? prevSubstanceIndex * 8 : ((prevSubstanceIndex * 8) + 7);
                }
                else
                {
                    _currentQuestionIndex = 0;
                }
            }
            else
            {
                _currentQuestionIndex--;
            }

            LoadCurrentQuestionAnswers();
            UpdateAssessmentUI();
        }

        private async void SubmitAssessment_Click(object sender, RoutedEventArgs e)
        {
            await SubmitAssessmentAsync();
        }

        private async Task SubmitAssessmentAsync()
        {
            if (!IsAnswerSelected() || _isSubmitting)
                return;

            SaveCurrentAnswer();
            _isSubmitting = true;
            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Submitting...";

            try
            {
                if (string.IsNullOrEmpty(_currentMemberId))
                {
                    // // Assuming App provides a way to get current user identity
                    // var identity = (System.Security.Principal.IIdentity)App.CurrentUser?.Identity;
                    // if (identity?.IsAuthenticated == true)
                    // {
                    //     _currentMemberId = identity.Name; // Adjust based on actual claim type
                    //     Console.WriteLine($"Retrieved MemberId: {_currentMemberId}");
                    // }

                    if (string.IsNullOrEmpty(_currentMemberId))
                    {
                        MessageBox.Show("Failed to retrieve Member ID. Please login again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService?.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                        return;
                    }
                }

                var scoreDetails = new List<string>();
                var recommendations = new List<string>();
                int totalScore = 0;

                switch (AssessmentId)
                {
                    case "ASSIST":
                        var substanceScores = new Dictionary<string, int>();
                        foreach (var substance in _substances)
                        {
                            var substanceId = substance.ToUpper().Replace(" ", "_");
                            if (_userAnswers[$"ASSIST_{substanceId}_Q1"] == $"ASSIST_{substanceId}_Q1_NO")
                            {
                                substanceScores[substance] = 0;
                                scoreDetails.Add($"{substance}: No use reported");
                                recommendations.Add($"{substance}: No intervention needed.");
                                continue;
                            }

                            int substanceScore = 0;
                            var substanceDetails = new List<string>();

                            for (int i = 2; i <= 8; i++)
                            {
                                var questionId = $"ASSIST_{substanceId}_Q{i}";
                                if (!_userAnswers.TryGetValue(questionId, out var answerId) || string.IsNullOrEmpty(answerId))
                                    continue;
                                var answer = _questionAnswers[questionId].FirstOrDefault(a => a.AnswerId == answerId);
                                if (answer == null)
                                {
                                    Console.WriteLine($"Invalid AnswerId for {questionId}: {answerId}");
                                    continue;
                                }
                                substanceScore += answer.ScoreValue;
                                substanceDetails.Add($"Q{i}:{answer.Answer}");
                            }

                            substanceScores[substance] = substanceScore;
                            totalScore += substanceScore;
                            scoreDetails.Add($"{substance}: Score={substanceScore}; {string.Join(", ", substanceDetails)}");

                            var substanceRecommendation = substanceScore switch
                            {
                                <= 10 => "Low risk: Brief education recommended.",
                                <= 26 => "Moderate risk: Brief intervention recommended.",
                                _ => "High risk: Referral to treatment recommended."
                            };
                            recommendations.Add($"{substance}: {substanceRecommendation}");
                        }
                        break;
                    case "CRAFFT":
                        Console.WriteLine($"CRAFFT Submission - Q1: {_userAnswers.GetValueOrDefault("CRAFFT_Q1", "None")}, Q2: {_userAnswers.GetValueOrDefault("CRAFFT_Q2", "None")}, Q3: {_userAnswers.GetValueOrDefault("CRAFFT_Q3", "None")}");
                        bool allZero = _userAnswers.GetValueOrDefault("CRAFFT_Q1", "").EndsWith("_0") &&
                                      _userAnswers.GetValueOrDefault("CRAFFT_Q2", "").EndsWith("_0") &&
                                      _userAnswers.GetValueOrDefault("CRAFFT_Q3", "").EndsWith("_0");

                        foreach (var question in _questions)
                        {
                            if (!question.QuestionId.StartsWith("CRAFFT_Q") || (allZero && question.QuestionId != "CRAFFT_Q4"))
                                continue;
                            if (!_userAnswers.TryGetValue(question.QuestionId, out var answerId) || string.IsNullOrEmpty(answerId))
                                continue;
                            var answer = _questionAnswers[question.QuestionId].FirstOrDefault(a => a.AnswerId == answerId);
                            if (answer == null)
                            {
                                Console.WriteLine($"Invalid AnswerId for {question.QuestionId}: {answerId}");
                                continue;
                            }
                            totalScore += answer.ScoreValue;
                            scoreDetails.Add($"{question.QuestionId}: {answer.Answer}");
                        }
                        recommendations.Add(totalScore >= 2 ? "Further evaluation recommended." : "Low risk.");
                        break;
                }

                _assessmentResult = new AssessmentResultDTO
                {
                    ResultId = Guid.NewGuid().ToString(),
                    AssessmentId = AssessmentId,
                    MemberId = _currentMemberId,
                    TotalScore = totalScore,
                    ScoreDetails = string.Join(";", scoreDetails),
                    Recommendation = string.Join(" ", recommendations),
                    CompletedOn = DateOnly.FromDateTime(DateTime.Now)
                };

                if (string.IsNullOrEmpty(_assessmentResult.MemberId) || string.IsNullOrEmpty(_assessmentResult.AssessmentId) || string.IsNullOrEmpty(_assessmentResult.ResultId))
                {
                    Console.WriteLine("Invalid submission: Missing required fields.");
                    _hasError = true;
                    _errorMessage = "Invalid submission details.";
                    ShowPanels(error: true);
                    return;
                }

                Console.WriteLine($"Submission Payload: MemberId={_assessmentResult.MemberId}, ResultId={_assessmentResult.ResultId}, TotalScore={_assessmentResult.TotalScore}");
                var result = await _assessmentService.SubmitAssessmentAsync(AssessmentId, _assessmentResult);

                if (result != null)
                {
                    _showResults = true;
                    UpdateResultsUI();
                    Console.WriteLine($"Assessment submitted successfully. ResultId: {result.ResultId}");
                }
                else
                {
                    _hasError = true;
                    _errorMessage = "Failed to submit assessment. Please check your answers and try again.";
                    Console.WriteLine("Submission failed. Check API response logs for details.");
                    ShowPanels(error: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Submission Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                _hasError = true;
                _errorMessage = $"Failed to submit assessment: {ex.Message}";
                ShowPanels(error: true);
            }
            finally
            {
                _isSubmitting = false;
                SubmitButton.Content = "✓ Submit Assessment";
                ShowPanels();
            }
        }

        private void UpdateResultsUI()
        {
            if (_assessmentResult == null)
                return;

            ResultsHeader.Text = $"{_assessment?.AssessmentType} Results";
            TotalScore.Text = _assessmentResult.TotalScore.ToString();
            Recommendation.Text = _assessmentResult.Recommendation;
            ScoreDetails.ItemsSource = _assessmentResult.ScoreDetails?.Split(';').Select((detail, index) => ProcessAndReturnDetail(detail, index));
            CompletedOn.Text = _assessmentResult.CompletedOn?.ToString("MMMM dd, yyyy");
            ShowPanels(results: true);
        }

        private string ProcessAndReturnDetail(string detail, int index)
        {
            var trimmedDetail = detail.Trim();
            if (!string.IsNullOrEmpty(trimmedDetail) && trimmedDetail.Contains(':'))
            {
                return trimmedDetail;
            }
            else
            {
                return $"Detail {index + 1}: {trimmedDetail}";
            }
        }

        private void RetryLoadAssessment_Click(object sender, RoutedEventArgs e)
        {
            LoadAssessmentAsync();
        }

        private void ConfirmExit_Click(object sender, RoutedEventArgs e)
        {
            _showExitConfirmation = true;
            ShowPanels();
        }

        private void CancelExit_Click(object sender, RoutedEventArgs e)
        {
            _showExitConfirmation = false;
            ShowPanels();
        }

        private void ExitAssessment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("/Assessment.xaml", UriKind.Relative));
        }

        private void GoBackToAssessments_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("/Assessment.xaml", UriKind.Relative));
        }

        private void TakeAnotherAssessment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("/Assessment.xaml", UriKind.Relative));
        }

        private void ShowPanels(bool loading = false, bool error = false, bool noContent = false, bool results = false, bool assessment = false)
        {
            LoadingPanel.Visibility = loading || _isLoading ? Visibility.Visible : Visibility.Collapsed;
            ErrorPanel.Visibility = error || _hasError ? Visibility.Visible : Visibility.Collapsed;
            NoContentPanel.Visibility = noContent ? Visibility.Visible : Visibility.Collapsed;
            ResultsPanel.Visibility = results && _showResults ? Visibility.Visible : Visibility.Collapsed;
            AssessmentPanel.Visibility = assessment && !_showResults ? Visibility.Visible : Visibility.Collapsed;
            ExitConfirmationPanel.Visibility = _showExitConfirmation ? Visibility.Visible : Visibility.Collapsed;

            if (_hasError)
            {
                ErrorMessage.Text = _errorMessage;
            }
        }
    }
}