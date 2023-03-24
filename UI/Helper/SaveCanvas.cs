using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI.Helper
{
    public static class SaveCanvas
    {
        public static void SaveAsPNG(Canvas target, string path = "C:/Users/arnes/source/repos/BauphysikToolWPF/Resources/ElementImages/")
        {
            //string path2 = Environment.CurrentDirectory; //"C:\\Users\\arnes\\source\\repos\\BauphysikToolWPF\\bin\\Debug\\net6.0-windows"

            // Set the Bitmap size and target to save
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)target.RenderSize.Width, (int)target.RenderSize.Height, 48d, 48d, PixelFormats.Default); // Default DPI: 96d
            bitmap.Render(target);

            // Set Width, Height and Croppings: Create always img of same size, regardless of current canvas dimensions
            var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(0, 0, (int)target.RenderSize.Width / 2, (int)target.RenderSize.Width / 2));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            string imgName = "Element_" + FO0_LandingPage.SelectedElementId + ".png";

            // use using to call Dispose() after use of unmanaged resources. GC cannot manage this

            using (var fileStream = File.OpenWrite(path + imgName))
            {
                encoder.Save(fileStream);
            }
        }

        public static byte[]? SaveAsBLOB(ItemsControl target)
        {
            if (target.ItemsSource is null)
                return null;

            // Convert the BitmapImage to a byte array
            byte[] imageBytes;

            // Set the Bitmap size and target to save
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)target.RenderSize.Width, (int)target.RenderSize.Height, 96d, 96d, PixelFormats.Default); // Default DPI: 96d -> Adapt cropping (48d -> Width / 2)
            bitmap.Render(target);

            // Set Width, Height and Croppings: Create always rectangular img of same size, regardless of current canvas dimensions
            var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(0, 0, (int)target.RenderSize.Width, (int)target.RenderSize.Width));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            // use using to call Dispose() after use of unmanaged resources. GC cannot manage this
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                imageBytes = stream.ToArray();
            }
            return imageBytes;
        }
    }
}
