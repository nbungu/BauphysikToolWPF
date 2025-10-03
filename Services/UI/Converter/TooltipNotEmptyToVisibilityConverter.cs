using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BauphysikToolWPF.Services.UI.Converter
{
    public class TooltipNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? tooltip = value as string;
            return string.IsNullOrWhiteSpace(tooltip) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
