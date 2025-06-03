using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI
{
    public class ValueAndWidthToOffsetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 ||
                !(values[0] is double value) ||
                !(values[1] is double width) ||
                !(values[2] is double min) ||
                !(values[3] is double max))
                return 0.0;

            double range = max - min;
            if (range <= 0)
                return 0.0;

            double margin = 8.0;
            double normalized = (value - min) / range;
            double usableWidth = width - margin; // adjust margin if needed
            return margin / 2 + normalized * usableWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
