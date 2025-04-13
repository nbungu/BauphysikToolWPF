using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class DoubleWithDecimalPlacesConverter : IMultiValueConverter, IValueConverter
    {
        // MultiBinding Convert method
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] != null && values[1] is int decimalPlaces)
            {
                culture = CultureInfo.CurrentCulture; // Use current culture for parsing
                string formatString = $"F{decimalPlaces}"; // Format based on decimal places

                if (values[0] is double doubleValue)
                {
                    return doubleValue.ToString(formatString, culture);
                }
                else if (values[0] is int intValue)
                {
                    return intValue.ToString("F0", culture); // Integers have no decimals
                }
            }
            return values[0]?.ToString() ?? string.Empty; // Fallback for null values
        }

        // MultiBinding ConvertBack method
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            culture = CultureInfo.CurrentCulture; // Use current culture for parsing
            if (value is string stringValue && double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
            {
                return new object[] { result, Binding.DoNothing }; // Return parsed double + unchanged decimal places
            }
            return new object[] { 0.0, 0 }; // Default fallback
        }

        // Single Binding Convert method
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            culture = CultureInfo.CurrentCulture; // Use current culture for parsing
            int decimalPlaces = parameter is int param ? param : 2; // Default to 2 decimal places if not provided
            string formatString = $"F{decimalPlaces}"; // Format based on decimal places

            if (value is double doubleValue)
            {
                return doubleValue.ToString(formatString, culture);
            }
            else if (value is int intValue)
            {
                return intValue.ToString("F0", culture); // Integers have no decimals
            }
            return value?.ToString() ?? string.Empty;
        }

        // Single Binding ConvertBack method
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            culture = CultureInfo.CurrentCulture; // Use current culture for parsing
            if (value is string stringValue && double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
            {
                return result;
            }
            return 0.0; // Default fallback
        }
    }
}