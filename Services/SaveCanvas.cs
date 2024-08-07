using BauphysikToolWPF.UI.CustomControls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BT.Logging;

namespace BauphysikToolWPF.Services
{
    public static class SaveCanvas
    {
        // TODO adjust offsets like BLOB
        /*public static void SaveAsPNG(ItemsControl target, string path = "C:/Users/arnes/source/repos/BauphysikToolWPF/Resources/ElementImages/")
        {
            if (target.ItemsSource is null) return;

            //path = "C:\\Users\\arnes\\source\\repos\\BauphysikToolWPF\\bin\\Debug\\net6.0-windows";

            // Set the Bitmap size and target to save
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)target.RenderSize.Width, (int)target.RenderSize.Height, 96d, 96d, PixelFormats.Default); // Default DPI: 96d
            bitmap.Render(target);

            // Set Width, Height and Croppings: Create always img of same size, regardless of current canvas dimensions
            int yOffset = Math.Abs((int)target.RenderSize.Width - (int)target.RenderSize.Height);
            var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(0, yOffset, (int)target.RenderSize.Width, (int)target.RenderSize.Width));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            string imgName = "Element_" + UserSaved.SelectedElement.Id + ".png";

            // use using to call Dispose() after use of unmanaged resources. GC cannot manage this

            using var fileStream = File.OpenWrite(path + imgName);
            encoder.Save(fileStream);
        }*/

        public static byte[] SaveAsBLOB(LayersCanvas target, bool asSquareImage = false)
        {
            if (target?.DrawingGeometries == null) return Array.Empty<byte>();

            // Convert the BitmapImage to a byte array
            try
            {
                // Set the Bitmap size and target to save
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)target.RenderSize.Width, (int)target.RenderSize.Height, 96d, 96d, PixelFormats.Default); // Default DPI: 96d -> Adapt cropping (48d -> Width / 2)
                bitmap.Render(target);

                BitmapEncoder encoder = new PngBitmapEncoder();
                if (asSquareImage)
                {
                    // Set Width, Height and Croppings: Create always img of same size, regardless of current canvas dimensions
                    int offset = Math.Abs((int)target.RenderSize.Width - (int)target.RenderSize.Height) / 2;

                    // Set Focus to middle of Image via offsets
                    bool isVertical = (int)target.RenderSize.Width < (int)target.RenderSize.Height;
                    bool isSquare = (int)target.RenderSize.Width == (int)target.RenderSize.Height;
                    int offsetX = isVertical ? 0 : offset;
                    int offsetY = isVertical ? offset : 0;
                    if (isSquare)
                    {
                        offsetX = offset;
                        offsetY = offset;
                    }

                    var sizeOfSquareImage = Math.Min((int)target.RenderSize.Width, (int)target.RenderSize.Height);
                    var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(offsetX, offsetY, sizeOfSquareImage, sizeOfSquareImage));
                    encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                }
                else
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                }
                
                // use using to call Dispose() after use of unmanaged resources. GC cannot manage this
                using MemoryStream stream = new MemoryStream();
                encoder.Save(stream);
                Logger.LogError($"Successfully created Image from Canvas");
                return stream.ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error creating Image from Canvas: {e.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}
