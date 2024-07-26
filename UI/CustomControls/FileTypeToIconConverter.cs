using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class FileTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string filePath = value as string;
            if (filePath == null)
                return null;

            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".txt":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/placeholder_256px.png"));
                case ".pdf":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/placeholder_256px.png"));
                // Add more cases for different file types
                default:
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/placeholder_256px.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
