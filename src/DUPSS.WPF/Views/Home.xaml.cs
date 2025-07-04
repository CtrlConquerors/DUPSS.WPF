using System;
using System.Windows;
using System.Windows.Controls; // Changed from System.Windows to System.Windows.Controls for Page
using System.Windows.Media.Animation; // For Storyboard animations
using System.Windows.Media; // For Brushes, etc.
using System.Windows.Threading; // For DispatcherTimer
using System.Windows.Navigation; // For Frame navigation

namespace DUPSS.WPF.Views // This namespace matches your Home.xaml's x:Class and local namespace
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page // Changed from Window to Page
    {
        // For the typewriter effect
        private DispatcherTimer _typewriterTimer;
        private string _fullText = "Empowering Communities, Fostering Resilience, Preventing Drug Abuse.";
        private int _charIndex = 0;

        // For the image carousel
        private int _currentSlideIndex = 0;
        private UIElement[] _carouselSlides; // Array to hold references to your slides

        // Storyboards for carousel fade effects
        private Storyboard _fadeInStoryboard;
        private Storyboard _fadeOutStoryboard;

        public Home()
        {
            InitializeComponent();
            this.Loaded += Home_Loaded; // Attach event handler for when the page is loaded
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the carousel slides array after the XAML components are loaded
            // Ensure Slide1 and Slide2 are x:Name'd in Home.xaml
            _carouselSlides = new UIElement[] { Slide1, Slide2 };

            // Initialize Storyboards for fade effects
            _fadeInStoryboard = new Storyboard();
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            _fadeInStoryboard.Children.Add(fadeInAnimation);

            _fadeOutStoryboard = new Storyboard();
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
            _fadeOutStoryboard.Children.Add(fadeOutAnimation);

            // Start the typewriter effect
            StartTypewriterEffect();

            // Initialize carousel to show the first slide
            ShowCarouselSlide(_currentSlideIndex);
        }

        private void StartTypewriterEffect()
        {
            TypewriterTextBlock.Text = ""; // Clear text initially
            _charIndex = 0;
            _typewriterTimer = new DispatcherTimer();
            _typewriterTimer.Interval = TimeSpan.FromMilliseconds(50); // Adjust typing speed here
            _typewriterTimer.Tick += TypewriterTimer_Tick;
            _typewriterTimer.Start();
        }

        private void TypewriterTimer_Tick(object sender, EventArgs e)
        {
            if (_charIndex < _fullText.Length)
            {
                TypewriterTextBlock.Text += _fullText[_charIndex];
                _charIndex++;
            }
            else
            {
                _typewriterTimer.Stop();
                // Optionally, add a blinking cursor effect here if desired (more complex in WPF)
            }
        }

        /// <summary>
        /// Displays the specified slide with a fade effect.
        /// </summary>
        /// <param name="index">The index of the slide to show.</param>
        private void ShowCarouselSlide(int index)
        {
            // Ensure index is within bounds
            if (index < 0 || index >= _carouselSlides.Length) return;

            // Hide all slides (remove fade-out event handler to avoid stacking)
            foreach (var slide in _carouselSlides)
            {
                _fadeOutStoryboard.Completed -= (s, e) => slide.Visibility = Visibility.Collapsed;
                slide.Visibility = Visibility.Collapsed;
                slide.Opacity = 0;
            }

            // Show the active slide with fade-in effect
            var activeSlide = _carouselSlides[index];
            activeSlide.Visibility = Visibility.Visible;
            Storyboard.SetTarget(_fadeInStoryboard, activeSlide);
            _fadeInStoryboard.Begin();

            _currentSlideIndex = index;
        }

        /// <summary>
        /// Navigates to the previous image in the carousel.
        /// </summary>
        private void PrevImage_Click(object sender, RoutedEventArgs e)
        {
            int prevIndex = (_currentSlideIndex - 1 + _carouselSlides.Length) % _carouselSlides.Length;
            ShowCarouselSlide(prevIndex);
        }

        /// <summary>
        /// Navigates to the next image in the carousel.
        /// </summary>
        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            int nextIndex = (_currentSlideIndex + 1) % _carouselSlides.Length;
            ShowCarouselSlide(nextIndex);
        }

        /// <summary>
        /// Handles the "Read The Story" button click.
        /// When a button on a Page needs to navigate, it should typically
        /// communicate with its hosting Frame or Window.
        /// </summary>
        private void ReadTheStory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigating to inspiring story!", "Navigation");

            // To navigate to another page within the hosting Frame:
            // This assumes the current page is hosted within a Frame.
            // You would replace "AnotherPage.xaml" with the actual path to your story page.
            // NavigationService?.Navigate(new Uri("/Views/StoryPage.xaml", UriKind.Relative));
            // Or, if you want to communicate back to MainLayout for navigation:
            // (Parent as Frame)?.NavigationService?.Navigate(new Uri("/Views/StoryPage.xaml", UriKind.Relative));
        }
    }
}
