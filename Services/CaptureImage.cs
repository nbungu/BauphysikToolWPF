using BauphysikToolWPF.UI.Drawing;
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
        public static byte[] CaptureOffscreenVisualAsImage(CanvasDrawingService drawingService)
        {
            // Create a DrawingVisual to perform off-screen rendering
            DrawingVisual drawingVisual = new DrawingVisual();

            // Use the DrawingContext to draw the geometries
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                foreach (IDrawingGeometry geometry in drawingService.DrawingGeometries)
                {
                    // Draw the rectangle using the properties defined in each IDrawingGeometry
                    drawingContext.DrawRectangle(
                        geometry.BackgroundColor,
                        new Pen(geometry.RectangleBorderColor, geometry.RectangleBorderThickness),
                        new Rect(
                            geometry.Rectangle.TopLeft.X,
                            geometry.Rectangle.TopLeft.Y,
                            geometry.Rectangle.Width,
                            geometry.Rectangle.Height)
                    );

                    // Optionally draw additional details (like hatch patterns)
                    if (geometry.DrawingBrush != null)
                    {
                        drawingContext.DrawRectangle(
                            geometry.DrawingBrush,
                            null,
                            new Rect(
                                geometry.Rectangle.TopLeft.X,
                                geometry.Rectangle.TopLeft.Y,
                                geometry.Rectangle.Width,
                                geometry.Rectangle.Height)
                        );
                    }
                }
            }

            // Create a RenderTargetBitmap to capture the visual
            int width = (int)drawingService.CanvasSize.Width;
            int height = (int)drawingService.CanvasSize.Height;
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);

            // Convert the bitmap to a byte array
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using (var memoryStream = new MemoryStream())
            {
                pngEncoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] CaptureUIElementAsImage(FrameworkElement target, bool asSquareImage = false, bool includeMargins = false)
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
    }
}
