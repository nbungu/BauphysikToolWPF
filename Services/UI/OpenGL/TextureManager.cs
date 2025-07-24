using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Size = System.Drawing.Size;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Manages the creation, caching, and disposal of OpenGL textures created from WPF DrawingBrush instances.
    /// This class prevents redundant texture generation and ensures correct texture settings.
    /// </summary>
    public class TextureManager : IDisposable
    {
        private readonly Dictionary<Brush, int> _hatchTextureCache = new();
        private readonly Dictionary<int, Size> _hatchTextureSizes = new();

        public int? GetTextureIdForBrush(DrawingBrush brush)
        {
            if (_hatchTextureCache.TryGetValue(brush, out int id))
                return id;

            var bitmap = RenderBrushToBitmap(brush);
            int texId = CreateTextureFromBitmap(bitmap);
            if (texId != 0)
            {
                _hatchTextureCache[brush] = texId;
            }
            return texId;
        }

        public Size? GetTextureSize(int textureId)
        {
            return _hatchTextureSizes.TryGetValue(textureId, out var size) ? size : null;
        }

        private static BitmapSource RenderBrushToBitmap(DrawingBrush brush)
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

        public int CreateTextureFromBitmap(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            bmp.CopyPixels(pixels, stride, 0);

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            ApplyTextureParameters();

            _hatchTextureSizes[texId] = new Size(width, height);

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"OpenGL Error in CreateTextureFromBitmap: {err}");
                return 0;
            }

            return texId;
        }

        public int CreateTextTextureFromBitmap(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            bmp.CopyPixels(pixels, stride, 0);

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            ApplyTextTextureParameters();

            _hatchTextureSizes[texId] = new Size(width, height);

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"OpenGL Error in CreateTextureFromBitmap: {err}");
                return 0;
            }

            return texId;
        }

        private static void ApplyTextureParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        private static void ApplyTextTextureParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public void Dispose()
        {
            foreach (var texId in _hatchTextureCache.Values)
            {
                GL.DeleteTexture(texId);
            }
            _hatchTextureCache.Clear();
            _hatchTextureSizes.Clear();
        }
    }
}
