using BauphysikToolWPF.Services.Application;
using BT.Logging;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = BT.Geometry.Point;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public static class OglConverterExtension
    {
        public static Vector4 ToVectorColor(this Brush b)
        {
            if (b is SolidColorBrush s)
            {
                var c = s.Color;
                return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            }
            return new Vector4(1f, 1f, 1f, 1f);
        }

        public static BitmapSource ToBitmapSource(this DrawingBrush brush)
        {
            int width = (int)brush.Viewbox.Width;
            int height = (int)brush.Viewbox.Height;

            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            return rtb;
        }

        public static Point ToPoint(this System.Windows.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static byte[] ToByteArray(this BitmapSource bmp)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

        public static BitmapImage ToBitmapImage(this byte[] imageBytes)
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

        public static void SaveToImage(this byte[] imageData, string fileName)
        {
            string path = Path.Combine(PathService.LocalProgramDataPath, "BauphysikTool", fileName);
            File.WriteAllBytes(path, imageData);
            Logger.LogInfo($"Image saved to {path}");
        }

        public static void SaveToImageInDownloadsFolder(this byte[] imageData, string fileName)
        {
            string path = Path.Combine(PathService.DownloadsFolderPath, fileName);
            File.WriteAllBytes(path, imageData);
            Logger.LogInfo($"Image saved to {path}");
        }

        public static void SaveToImage(this BitmapSource bmp, string fileName)
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

        public static void SaveToImageInDownloadsFolder(this BitmapSource bmp, string fileName)
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
    }
}
