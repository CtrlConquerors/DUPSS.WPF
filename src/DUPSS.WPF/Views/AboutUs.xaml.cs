using System.Windows.Controls;
using System.Windows.Threading;
using System;

namespace DUPSS.WPF.Views
{
    public partial class AboutUs : UserControl
    {
        private string typewriterText = "We Are ALPHA";
        private int charIndex = 0;
        private DispatcherTimer timer;

        public AboutUs()
        {
            InitializeComponent();

            // Dummy data for timeline
            this.DataContext = new
            {
                Timeline = new[]
                {
                    new { Year="2019", Content="Founded by a group of passionate youth aiming to make a difference in drug prevention." },
                    new { Year="2021", Content="Reached 10,000+ students and held 100+ awareness events in schools and communities." },
                    new { Year="2024", Content="Launched nationwide peer-support networks to foster ongoing connections and care." }
                }
            };

            Loaded += AboutUs_Loaded;
        }

        private void AboutUs_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (charIndex < typewriterText.Length)
            {
                TypewriterText.Text += typewriterText[charIndex];
                charIndex++;
            }
            else
            {
                timer.Stop();
            }
        }
    }
}
