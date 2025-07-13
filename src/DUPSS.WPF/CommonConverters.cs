using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics; // Added for Debug.WriteLine in converters if needed, though not strictly required for basic functionality

namespace DUPSS.WPF // This namespace should match the 'local' prefix in App.xaml
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Visibility)value == Visibility.Collapsed;
        }
    }

    public class BooleanToVisibilityConverterInverted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double width)
            {
                return width / 2;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // NEW: StringToVisibilityConverter
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // NEW: BooleanToChevronConverter
    public class BooleanToChevronConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return "▼"; // Down arrow for expanded
            }
            return "▶"; // Right arrow for collapsed
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // NEW: Custom converter to handle string to Uri conversion for MediaElement Source
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string uriString && !string.IsNullOrEmpty(uriString))
            {
                // Use UriKind.RelativeOrAbsolute for local paths
                return new Uri(uriString, UriKind.RelativeOrAbsolute);
            }
            // Return null or DependencyProperty.UnsetValue if the string is null or empty
            return null; // Returning null is generally safer for MediaElement.Source when no video
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts between System.DateTime? (for WPF DatePicker) and System.DateOnly? (for DTOs).
    /// </summary>
    public class DateTimeToDateOnlyConverter : IValueConverter
    {
        /// <summary>
        /// Converts a DateOnly? value to a DateTime? value for display in the DatePicker.
        /// </summary>
        /// <param name="value">The DateOnly? value from the source (UserDTO.DoB).</param>
        /// <param name="targetType">The type of the target property (should be DateTime?).</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A DateTime? value, or null if the input is null.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly dateOnlyValue)
            {
                return dateOnlyValue.ToDateTime(TimeOnly.MinValue); // Convert DateOnly to DateTime
            }
            return null; // Return null if the input is not a DateOnly or is null
        }

        /// <summary>
        /// Converts a DateTime? value from the DatePicker back to a DateOnly? value for the DTO.
        /// </summary>
        /// <param name="value">The DateTime? value from the target (DatePicker.SelectedDate).</param>
        /// <param name="targetType">The type of the source property (should be DateOnly?).</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A DateOnly? value, or null if the input is null.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTimeValue)
            {
                return DateOnly.FromDateTime(dateTimeValue); // Convert DateTime to DateOnly
            }
            return null; // Return null if the input is not a DateTime or is null
        }
    }
}
