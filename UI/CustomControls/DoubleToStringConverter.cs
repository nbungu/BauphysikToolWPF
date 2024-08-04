using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the number of decimal places from the parameter or default to 2
            int decimalPlaces = 2;
            if (parameter is string parameterString && int.TryParse(parameterString, out int parsedPlaces))
            {
                decimalPlaces = parsedPlaces;
            }

            // Create a format string with the specified number of decimal places
            string formatString = $"F{decimalPlaces}";

            if (value is int intValue)
            {
                // Format the number with no decimal places
                return intValue.ToString("F0", CultureInfo.CurrentCulture);
            }
            else if (value is double doubleValue)
            {
                // Format the number with four decimal places
                return doubleValue.ToString(formatString, CultureInfo.CurrentCulture);
            }
            return value;
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
    }
}
