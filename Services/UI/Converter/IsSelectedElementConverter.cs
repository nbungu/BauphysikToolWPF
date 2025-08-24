using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI.Converter
{
    public class IsSelectedElementConverter : IMultiValueConverter
    {
        public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
        {
            // values are the objects from the MultiBindings (Paths), "ElementId" and "SelectedElementId"
            // parameter is from ConverterParameter Property if set.
            if (values is null) return false;

            int currentElement = -1;
            int selectedElement = -1;

            // use the 'is' keyword to check if the object is of the correct type before casting it!
            // Avoids Error "Unable to cast object of type 'MS.Internal.NamedObject' to type 'System.Int32'"
            if (values[0] is int)
            {
                currentElement = (int)values[0];
            }
            if (values[1] is int)
            {
                selectedElement = (int)values[1];
            }
            return currentElement == selectedElement;

            // If the WrapPanel Button (an Element) is currently the SelectedElement, return true.
            //return Page_Elements.SelectedElementId == (int)values ? true : false;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
