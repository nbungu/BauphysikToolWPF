using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class CategoryEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is MaterialCategory[] enumValues
                ? enumValues.Select(e => MaterialCategoryMapping[e]).ToList()
                : new List<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
