using BT.Logging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.Services
{
    public static class CaptureImage
    {
        // render relevant visual elements off-screen, when they aren't part of the active visual tree.
        public static byte[] CaptureVisualAsImage(FrameworkElement target, bool asSquareImage = false, bool includeMargins = false)
        {
            if (target == null) return Array.Empty<byte>();

            try
            {
                Size size = new Size(target.ActualWidth, target.ActualHeight);
                if (includeMargins)
                {
                    // Calculate the size, including margins and padding
                    double width = target.ActualWidth + target.Margin.Left + target.Margin.Right;
                    double height = target.ActualHeight + target.Margin.Top + target.Margin.Bottom;
                    size = new Size(width, height);
                }
                target.Measure(size);
                target.Arrange(new Rect(size));

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);
                renderBitmap.Render(target);

                BitmapEncoder encoder = new PngBitmapEncoder();
                if (asSquareImage)
                {
                    int offset = Math.Abs((int)size.Width - (int)size.Height) / 2;
                    int offsetX = (int)size.Width > (int)size.Height ? offset : 0;
                    int offsetY = (int)size.Width < (int)size.Height ? offset : 0;
                    int squareSize = Math.Min((int)size.Width, (int)size.Height);
                    var croppedBitmap = new CroppedBitmap(renderBitmap, new Int32Rect(offsetX, offsetY, squareSize, squareSize));
                    encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                }
                else
                {
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                }

                using MemoryStream stream = new MemoryStream();
                encoder.Save(stream);
                Logger.LogInfo($"Successfully captured image of {target.GetType().Name}");
                return stream.ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error capturing image of {target.GetType().Name}: {e.Message}");
                return Array.Empty<byte>();
            }
        }

        public static void SaveImageToFile(byte[] imageData, string fileName)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BauphysikTool", fileName);
            File.WriteAllBytes(path, imageData);
            Logger.LogInfo($"Image saved to {path}");
        }

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

        /*public static byte[] SaveAsBLOB(LayersCanvas? target, bool asSquareImage = false)
        {
            if (target is null) return Array.Empty<byte>();
            if (target?.DrawingGeometries is null) return Array.Empty<byte>();

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
        public static byte[] SaveGridAsBLOB(Grid? targetGrid, bool asSquareImage = false)
        {
            if (targetGrid == null) return Array.Empty<byte>();

            try
            {
                // Calculate the size, including margins and padding
                double width = targetGrid.ActualWidth + targetGrid.Margin.Left + targetGrid.Margin.Right;
                double height = targetGrid.ActualHeight + targetGrid.Margin.Top + targetGrid.Margin.Bottom;

                Size size = new Size(width, height);
                targetGrid.Measure(size);
                targetGrid.Arrange(new Rect(new Point(0, 0), size));

                // Create a RenderTargetBitmap to capture the grid's visual
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
                bitmap.Render(targetGrid);

                // Create an encoder and save the bitmap to a memory stream
                BitmapEncoder encoder = new PngBitmapEncoder();
                if (asSquareImage)
                {
                    // Optional: Crop to a square image
                    int offset = Math.Abs((int)size.Width - (int)size.Height) / 2;
                    int offsetX = (int)size.Width > (int)size.Height ? offset : 0;
                    int offsetY = (int)size.Width < (int)size.Height ? offset : 0;
                    int squareSize = Math.Min((int)size.Width, (int)size.Height);
                    var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(offsetX, offsetY, squareSize, squareSize));
                    encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                }
                else
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                }

                using MemoryStream stream = new MemoryStream();
                encoder.Save(stream);
                Logger.LogInfo($"Successfully created Image from Grid");
                return stream.ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error creating Image from Grid: {e.Message}");
                return Array.Empty<byte>();
            }
        }*/
    }
}
