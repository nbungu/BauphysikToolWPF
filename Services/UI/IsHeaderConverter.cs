using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI
{
    public class IsHeaderConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            // value is the object from the Binding (ItemsSource of ListBox): e.g. string "Header1" or "LayerSetup"
            if (value is null) return false;

            string itemsSourceValue = value.ToString() ?? "";
            Regex regex = new Regex("(H|h)eader[0-9]?"); // regex that matches disallowed text

            // return true if Item is a header
            return regex.IsMatch(itemsSourceValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
