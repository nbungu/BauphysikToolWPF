using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;
        public bool CollapseInsteadOfHidden { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            if (Invert) isNull = !isNull;

            if (isNull)
                return CollapseInsteadOfHidden ? Visibility.Collapsed : Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
