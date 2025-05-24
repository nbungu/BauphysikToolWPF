using System;
using System.Globalization;

namespace BauphysikToolWPF.Services.UI
{
    public class ConditionalNumberConverter : NumberConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return string.Empty;

            var numberValue = values[0];
            var showRoomData = values[1] as bool? ?? false;

            if (!showRoomData)
                return null; // or "N/A" if you prefer

            // Reuse base NumberConverter logic:
            return base.Convert(new object[] { numberValue, parameter ?? 2 }, targetType, parameter, culture);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return base.ConvertBack(value, targetTypes, parameter, culture);
        }
    }
}
