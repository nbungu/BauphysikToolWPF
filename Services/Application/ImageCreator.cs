using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.UI;
using BT.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;

namespace BauphysikToolWPF.Services.Application
{
    public static class ImageCreator
    {
        public static byte[] CaptureOffscreenVisualAsImage(List<IDrawingGeometry> drawingGeometries, int imgWidth, int imgHeight)
        {
            if (drawingGeometries.Count == 0) return Array.Empty<byte>();

            // Create a DrawingVisual to perform off-screen rendering
            DrawingVisual drawingVisual = new DrawingVisual();

            // Use the DrawingContext to draw the geometries
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                foreach (IDrawingGeometry geometry in drawingGeometries)
                {
                    // Adjust the background brush with opacity
                    Brush backgroundBrush = geometry.BackgroundColor.Clone();
                    backgroundBrush.Opacity = geometry.Opacity; // Apply opacity

                    // Zeichne das Hauptrechteck
                    drawingContext.DrawRectangle(
                        backgroundBrush,
                        geometry.BorderPen,
                        new Rect(
                            geometry.Rectangle.TopLeft.X,
                            geometry.Rectangle.TopLeft.Y,
                            geometry.Rectangle.Width,
                            geometry.Rectangle.Height)
                    );

                    // Falls es eine spezielle Brush (z. B. Schraffur oder Labels) gibt, zeichnen
                    if (geometry.TextureBrush != null)
                    {
                        Brush drawingBrush = geometry.TextureBrush.Clone();
                        drawingBrush.Opacity = geometry.Opacity;

                        drawingContext.DrawRectangle(
                            drawingBrush,
                            null,
                            new Rect(
                                geometry.Rectangle.TopLeft.X,
                                geometry.Rectangle.TopLeft.Y,
                                geometry.Rectangle.Width,
                                geometry.Rectangle.Height)
                        );
                    }

                    // Labels zeichnen
                    if (geometry.Tag is string tag && tag.StartsWith("Label_"))
                    {
                        string labelText = tag.Replace("Label_", ""); // Text aus Tag extrahieren

                        double radius = 10; // 20x20 px Kreis → Radius = 10
                        Point center = new Point(geometry.Rectangle.Left + geometry.Rectangle.Width / 2, geometry.Rectangle.Top + geometry.Rectangle.Height / 2);

                        // Kreis zeichnen (weiß mit schwarzem Rand)
                        drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 1), center, radius, radius);

                        // Text in den Kreis setzen
                        FormattedText formattedText = new FormattedText(
                            labelText,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            14, // Schriftgröße 14px
                            Brushes.Black,
                            1.0 // PixelsPerDip
                        )
                        {
                            TextAlignment = TextAlignment.Center
                        };
                        var textOffset = labelText.Contains("b") ? new Vector(8, 0) : new Vector(4, 0);
                        drawingContext.DrawText(formattedText, new Point(center.X - formattedText.Width / 2 + textOffset.X, center.Y - formattedText.Height / 2 + textOffset.Y));
                    }
                }
            }

            // Render das Bild in ein Byte-Array umwandeln
            RenderTargetBitmap bitmap = new RenderTargetBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
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

        public static void SaveToImage(byte[] imageData, string fileName)
        {
            string path = Path.Combine(PathService.LocalProgramDataPath, "BauphysikTool", fileName);
            File.WriteAllBytes(path, imageData);
            Logger.LogInfo($"Image saved to {path}");
        }

        public static void SaveToImageInDownloadsFolder(byte[] imageData, string fileName)
        {
            string path = Path.Combine(PathService.DownloadsFolderPath, fileName);
            File.WriteAllBytes(path, imageData);
            Logger.LogInfo($"Image saved to {path}");
        }

        public static void SaveToImage(BitmapSource bmp, string fileName)
        {
            string path = Path.Combine(PathService.LocalProgramDataPath, "BauphysikTool", fileName);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (var fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }
            Logger.LogInfo($"Image saved to {path}");
        }

        public static void SaveToImageInDownloadsFolder(BitmapSource bmp, string fileName)
        {
            string path = Path.Combine(PathService.DownloadsFolderPath, fileName);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (var fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }
            Logger.LogInfo($"Image saved to {path}");
        }

        public static void RenderElementPreviewImage(Element element)
        {
            // Create a CrossSectionDrawing for the selected element
            var canvasSize = new BT.Geometry.Rectangle(new BT.Geometry.Point(0, 0), 880, 400);
            var drawingService = new CrossSectionBuilder(element, canvasSize, DrawingType.CrossSection);
            var drawingContents = drawingService.DrawingGeometries;
            var imgWidth = (int)drawingService.CanvasSize.Width;
            var imgHeight = (int)drawingService.CanvasSize.Height;

            // Capture images using the GeometryRenderer
            var imageBytes = CaptureOffscreenVisualAsImage(drawingContents, imgWidth, imgHeight);

            // Update the selected element with the captured images
            element.Image = imageBytes;
        }

        public static BitmapImage ByteArrayToBitmap(byte[] imageBytes)
        {
            if (imageBytes == Array.Empty<byte>() || imageBytes.Length == 0) return new BitmapImage();// return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/placeholder_256px_light.png"));

            BitmapImage image = new BitmapImage();
            // use using to call Dispose() after use of unmanaged resources. GC cannot manage this
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                stream.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static byte[] EncodeBitmapSourceToPng(BitmapSource image)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
    }
}
