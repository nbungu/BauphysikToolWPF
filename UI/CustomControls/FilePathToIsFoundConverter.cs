using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class FilePathToIsFoundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string filePath)
            {
                return File.Exists(filePath);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
