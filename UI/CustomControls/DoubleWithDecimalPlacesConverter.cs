using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class DoubleWithDecimalPlacesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[1] is int decimalPlaces)
            {
                // Create a format string with the specified number of decimal places
                string formatString = $"F{decimalPlaces}";

                if (values[0] is int intValue)
                {
                    // Format the number with no decimal places
                    return intValue.ToString("F0", CultureInfo.CurrentCulture);
                }
                else if (values[0] is double doubleValue)
                {
                    // Format the number with four decimal places
                    return doubleValue.ToString(formatString, CultureInfo.CurrentCulture);
                }
            }
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // Implement ConvertBack logic if needed
            if (value is string stringValue && double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
            {
                return new object[] { result, Binding.DoNothing };
            }
            return new object[] { 0.0, 0 };
        }
    }
}
