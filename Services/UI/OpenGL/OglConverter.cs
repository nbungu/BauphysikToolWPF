using System.Windows;
using OpenTK.Mathematics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = BT.Geometry.Point;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public static class OglConverter
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
    }
}
