using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class IsSelectedElementConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values are the objects from the MultiBindings (Paths), "ElementId" and "SelectedElementId"
            // parameter is from ConverterParameter Property if set.
            if (values is null)
                return false;

            int currentElement = (int)values[0];
            int selectedElement = (int)values[1];

            return currentElement == selectedElement;


            // If the WrapPanel Button (an Element) is currently the SelectedElement, return true.
            //return FO0_LandingPage.SelectedElementId == (int)values ? true : false;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
