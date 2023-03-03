using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class ItemSelectedToEnabledConverter : IValueConverter
    {
        // value is the object from the Binding (Path), e.g. int "ElementId"
        // parameter is from ConverterParameter Property if set.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null; // Value object is SelectedItem from a ListView
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
