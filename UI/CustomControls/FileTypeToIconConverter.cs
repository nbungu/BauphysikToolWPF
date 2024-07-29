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
                case ".dgn":
                case ".dwf":
                case ".dwg":
                case ".dxf":
                case ".ifc":
                case ".rvt":
                case ".rfa":
                case ".rte":
                case ".rft":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/cad.png"));
                case ".doc":
                case ".docx":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/doc.png"));
                case ".jpg":
                case ".jpeg":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/jpg.png"));
                case ".pdf":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/pdf.png"));
                case ".png":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/png.png"));
                case ".ppt":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/ppt.png"));
                case ".txt":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/txt.png"));
                case ".xls":
                case ".xlsx":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/xls.png"));
                case ".xml":
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/xml.png"));
                // Add more cases for different file types
                default:
                    return new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Filetypes/file.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
