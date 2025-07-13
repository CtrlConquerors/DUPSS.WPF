using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using DUPSS.DTO.DTOs;
using DUPSS.ApiClients;

namespace DUPSS.WPF.Views
{
    public partial class BlogDetail : Page, INotifyPropertyChanged
    {
        private readonly BlogApiService _blogApiService;

        private BlogDTO? _blog;
        public BlogDTO? Blog
        {
            get => _blog;
            set { _blog = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowBlogContent)); }
        }

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowBlogContent)); }
        }

        private bool _hasError = false;
        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowBlogContent)); }
        }

        private bool _noBlogFound = false;
        public bool NoBlogFound
        {
            get => _noBlogFound;
            set { _noBlogFound = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowBlogContent)); }
        }

        public bool ShowBlogContent => Blog != null && !IsLoading && !HasError && !NoBlogFound;

        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly Uri _placeholderImageUri = new Uri("pack://application:,,,/DUPSS.WPF;component/Images/Blogs/placeholder.png");

        public BlogDetail(string blogId)
        {
            InitializeComponent();
            DataContext = this;

            if (App.HttpClient == null)
            {
                MessageBox.Show("App.HttpClient is not initialized. Cannot load blog details.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            _blogApiService = App.BlogApiService ?? new BlogApiService(App.HttpClient);

            Loaded += async (s, e) => await LoadBlogAsync(blogId);
        }

        private async Task LoadBlogAsync(string blogId)
        {
            IsLoading = true;
            HasError = false;
            NoBlogFound = false;
            Blog = null;

            try
            {
                var blog = await _blogApiService.GetByIdAsync(blogId);

                if (blog == null)
                {
                    NoBlogFound = true;
                    return;
                }

                blog.ImageUrl = GetPackUriForImage($"Images/Blogs/{blog.BlogId}", _imageExtensions, _placeholderImageUri).ToString();

                Blog = blog;
                RenderBlogContent(blog.Content ?? "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading blog detail: {ex.Message}");
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Uri GetPackUriForImage(string basePath, string[] extensions, Uri placeholderUri)
        {
            foreach (var ext in extensions)
            {
                var potentialUriString = $"pack://application:,,,/DUPSS.WPF;component/{basePath}{ext}";
                try
                {
                    var uri = new Uri(potentialUriString);
                    if (Application.GetResourceStream(uri) != null)
                        return uri;
                }
                catch { }
            }
            return placeholderUri;
        }

        private void RenderBlogContent(string content)
        {
            // Just display the raw blog content as plain text
            var flowDoc = new FlowDocument();
            var range = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            range.Text = content;
            BlogContentDocument.Document = flowDoc;
        }

        private async void RetryLoadBlog_Click(object sender, RoutedEventArgs e)
        {
            if (Blog != null)
                await LoadBlogAsync(Blog.BlogId);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                NavigationService?.Navigate(new Blog());
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Converter for Boolean to Visibility
    //public class BooleanToVisibilityConverter : System.Windows.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //        => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //        => throw new NotImplementedException();
    //}

    // Converter for Inverse Boolean to Visibility
    public class InverseBooleanToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => (value is bool b && !b) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}