using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class IsSelectedElementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value is the object from the Binding, e.g. int "ElementId"
            if (value is null)
                return false;

            // If the WrapPanel Button (an Element) is currently the SelectedElement, return true.
            return FO0_LandingPage.SelectedElementId == (int)value ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
