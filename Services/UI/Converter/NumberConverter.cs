using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI.Converter
{
    public class NumberConverter : IValueConverter, IMultiValueConverter
    {
        public static string ConvertToString(object value, int decimals = 2)
        {
            var culture = CultureInfo.CurrentCulture; // Use current culture for parsing
            var returnValue = new NumberConverter().Convert(value, null, decimals, culture);
            return (string)returnValue ?? string.Empty;
        }

        public static double ConvertToDouble(object value, int decimals = 6)
        {
            var culture = CultureInfo.CurrentCulture;

            if (value is double d)
            {
                return Math.Round(d, decimals);
            }

            if (value is string s)
            {
                // Reuse ConvertBack logic
                var result = new NumberConverter().ConvertBack(s, typeof(double), decimals, culture);
                if (result is double parsed)
                    return Math.Round(parsed, decimals);
            }

            return 0.0;
        }


        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            // Use current culture for parsing
            culture = CultureInfo.CurrentCulture;
            // Get the number of decimal places from the parameter or default to 2
            int decimalPlaces = 2;
            if (parameter is string parameterString && int.TryParse(parameterString, out int parsedPlaces))
            {
                decimalPlaces = parsedPlaces;
            }
            else if (parameter is int intPlaces)
            {
                decimalPlaces = intPlaces;
            }

            // Create a format string with the specified number of decimal places
            string formatString = $"F{decimalPlaces}";

            if (value is int intValue)
            {
                // Format the number with no decimal places
                return intValue.ToString("F0", culture);
            }
            else if (value is double doubleValue)
            {
                string formatted = doubleValue.ToString(formatString, culture);

                // If value is non-zero but rounds to 0.00, return the full precision instead
                if (Math.Abs(doubleValue) > 0 && formatted == (0.0).ToString(formatString, culture))
                {
                    // Show full precision without trailing zeros (or you can use "G" for general format)
                    return doubleValue.ToString("G", culture); // Or use "0.####" for more control
                }

                return formatted;
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                {
                    return result;
                }
            }
            return 0.0;
        }

        #region IMultiValueConverter

        // MultiBinding Convert method
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] != null && values[1] is int decimalPlaces)
            {
                culture = CultureInfo.CurrentCulture; // Use current culture for parsing
                string formatString = $"F{decimalPlaces}"; // Format based on decimal places

                if (values[0] is double doubleValue)
                {
                    string formatted = doubleValue.ToString(formatString, culture);

                    // If the value is non-zero but rounds to 0.00, return the full-precision value
                    if (Math.Abs(doubleValue) > 0 && formatted == (0.0).ToString(formatString, culture))
                    {
                        // You can use "G" or "0.####" depending on formatting preferences
                        return doubleValue.ToString("G", culture);
                    }

                    return formatted;
                }
                else if (values[0] is int intValue)
                {
                    return intValue.ToString("F0", culture); // Integers have no decimals
                }
            }
            return values[0]?.ToString() ?? string.Empty; // Fallback for null values
        }

        // MultiBinding ConvertBack method
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            culture = CultureInfo.CurrentCulture; // Use current culture for parsing
            if (value is string stringValue && double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
            {
                return new object[] { result, Binding.DoNothing }; // Return parsed double + unchanged decimal places
            }
            return new object[] { 0.0, 0 }; // Default fallback
        }

        #endregion
    }
}
