using System;
using System.Globalization;
using System.Windows.Data;

namespace DUPSS.WPF.Views
{
    public class TruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && int.TryParse(parameter?.ToString(), out int maxLength))
            {
                return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}