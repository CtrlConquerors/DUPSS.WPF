using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading; // For DispatcherTimer

namespace DUPSS.WPF.Views // This namespace matches your Home.xaml's x:Class and local namespace
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        // For the typewriter effect
        private DispatcherTimer _typewriterTimer;
        private string _fullText = "Every individual and community has the ability to protect themselves from the risks of drug use and plays a vital role in building a healthier, safer society.";
        private int _charIndex = 0;

        // For the image carousel
        private int _currentSlideIndex = 0;
        private UIElement[] _carouselSlides; // Array to hold references to your slides

        public Home()
        {
            InitializeComponent();
            this.Loaded += Home_Loaded; // Attach event handler for when the window is loaded
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the carousel slides array after the XAML components are loaded
            _carouselSlides = new UIElement[] { Slide1, Slide2 }; // Ensure Slide1 and Slide2 are x:Name'd in XAML

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

        private void ShowCarouselSlide(int index)
        {
            // Ensure index is within bounds
            if (index < 0 || index >= _carouselSlides.Length) return;

            // Hide all slides
            foreach (var slide in _carouselSlides)
            {
                slide.Visibility = Visibility.Collapsed;
                slide.Opacity = 0;
            }

            // Show the active slide with fade-in effect
            var activeSlide = _carouselSlides[index];
            activeSlide.Visibility = Visibility.Visible;

            // Simple fade-in animation
            var opacityAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.7),
                FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd
            };
            activeSlide.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);

            _currentSlideIndex = index;
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            int nextIndex = (_currentSlideIndex + 1) % _carouselSlides.Length;
            ShowCarouselSlide(nextIndex);
        }

        private void PrevImage_Click(object sender, RoutedEventArgs e)
        {
            int prevIndex = (_currentSlideIndex - 1 + _carouselSlides.Length) % _carouselSlides.Length;
            ShowCarouselSlide(prevIndex);
        }

        private void ReadTheStory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigating to inspiring story!", "Navigation");
            // In a real WPF app, you would navigate to another window or change content
            // Example: var storyWindow = new InspiringStoryWindow();
            // storyWindow.Show();
            // this.Close(); // Close current window if navigating to a new one
        }
    }
}
