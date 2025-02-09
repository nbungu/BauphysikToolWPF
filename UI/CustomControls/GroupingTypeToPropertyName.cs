using System;
using System.Globalization;
using System.Windows.Data;
using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Repository.Models.Helper;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class GroupingTypeToPropertyName : IValueConverter
    {
        private readonly ElementGroupingType _grouping;
        public GroupingTypeToPropertyName(ElementGroupingType grouping)
        {
            _grouping = grouping;
        }

        // value is the object from the Binding (Path), e.g. "Element"
        //private readonly ElementGroupingType _grouping = Page_Elements_VM.SelectedGrouping;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Element element && _grouping != ElementGroupingType.None)
            {
                switch (_grouping)
                {
                    case ElementGroupingType.Type:
                        return element.Construction.TypeName;
                    case ElementGroupingType.Orientation:
                        return element.OrientationType;
                    case ElementGroupingType.Tag:
                        return element.TagList;
                    default:
                        return element.Construction.TypeName;
                }
            }
            return "Ohne Gruppierung"; // null
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
