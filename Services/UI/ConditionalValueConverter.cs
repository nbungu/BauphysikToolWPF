using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI
{
    public class ConditionalValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0]?.ToString();
            var show = values.Length > 1 && values[1] is bool b && b;

            return show ? value : null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
