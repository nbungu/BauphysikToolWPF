using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using BauphysikToolWPF.Repository.Models;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class CategoryEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = new List<string>();
            if (value is MaterialCategory[] enumValues)
            {
                foreach (var entry in enumValues)
                {
                    list.Add(Material.TranslateToCategoryName(entry));
                }
            }
            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
