using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.SQLiteRepo.Helper;
using BauphysikToolWPF.UI.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class GroupingTypeToPropertyName : IValueConverter
    {
        // value is the object from the Binding (Path), e.g. "Element"
        private ElementGroupingType _grouping = FO0_LandingPage_VM.SelectedGrouping;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Element)
            {
                var element = (Element)value;
                switch (_grouping)
                {
                    case ElementGroupingType.Type:
                        return element.Construction.TypeName;
                    case ElementGroupingType.Orientation:
                        return element.Orientation.TypeName;
                    case ElementGroupingType.Color:
                        return element.Color;
                    case ElementGroupingType.Tag:
                        return element.TagList[0];
                    default:
                        return element.Construction.TypeName;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
