using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.Services.UI.Converter
{
    public class FileTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? filePath = value as string;
            if (filePath == null) return new BitmapImage();

            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".btk":
                    return Icons.FiletypeIcon_BTK;
                case ".db":
                    return Icons.FiletypeIcon_DB;
                case ".dgn":
                case ".dwf":
                case ".dwg":
                case ".dxf":
                case ".ifc":
                case ".rvt":
                case ".rfa":
                case ".rte":
                case ".rft":
                    return Icons.FiletypeIcon_CAD;
                case ".doc":
                case ".docx":
                    return Icons.FiletypeIcon_DOC;
                case ".jpg":
                case ".jpeg":
                    return Icons.FiletypeIcon_JPG;
                case ".pdf":
                    return Icons.FiletypeIcon_PDF;
                case ".png":
                    return Icons.FiletypeIcon_PNG;
                case ".ppt":
                    return Icons.FiletypeIcon_PPT;
                case ".txt":
                    return Icons.FiletypeIcon_TXT;
                case ".xls":
                case ".xlsx":
                    return Icons.FiletypeIcon_XLS;
                case ".xml":
                    return Icons.FiletypeIcon_XML;
                // Add more cases for different file types
                default:
                    return Icons.FiletypeIcon_ANY;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
