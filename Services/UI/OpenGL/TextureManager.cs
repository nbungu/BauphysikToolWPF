using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Size = System.Drawing.Size;
using Rectangle = BT.Geometry.Rectangle;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Manages the creation, caching, and disposal of OpenGL textures created from WPF DrawingBrush instances.
    /// This class prevents redundant texture generation and ensures correct texture settings.
    /// </summary>
    public class TextureManager : IDisposable
    {
        private readonly OglController _parent;

        private readonly Dictionary<Brush, int> _hatchTextureCache = new();
        private readonly Dictionary<int, Size> _hatchTextureSizes = new();

        public SdfFont? SdfFont;

        public TextureManager(OglController parent)
        {
            _parent = parent;
        }

        public void SetDefaultFont()
        {
            SdfFont = SdfFontLoader.LoadFromFntFile("Resources/Fonts/segoeUI.fnt", this);
        }

        public int? GetTextureIdForBrush(DrawingBrush brush)
        {
            if (_hatchTextureCache.TryGetValue(brush, out int id))
                return id;

            var bitmap = brush.ToBitmapSource();
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

        public int CreateTextureFromBitmap(BitmapSource bmp, bool isText = false)
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

            if (isText) ApplyFontTextureParameters();
            else ApplyTextureParameters();

            _hatchTextureSizes[texId] = new Size(width, height);

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"OpenGL Error in CreateTextureFromBitmap: {err}");
                return 0;
            }

            return texId;
        }

        public int CreateFontTextureFromBitmap(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            // Lock bitmap data for direct memory access
            var rect = new Rectangle(0, 0, width, height).ToWpfRectangle();
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            bmp.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            ApplyFontTextureParameters();

            _hatchTextureSizes[texId] = new Size(width, height);

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"OpenGL Error in CreateTextTextureFromBitmap: {err}");
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
        private static void ApplyFontTextureParameters()
        {
            //Enable mipmaps for minification
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

            //Regular bilinear filtering for magnification
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //Clamp edges to avoid sampling outside the atlas
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
