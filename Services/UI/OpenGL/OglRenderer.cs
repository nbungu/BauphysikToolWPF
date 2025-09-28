using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class OglRenderer : IDisposable
    {
        private int _shaderProgram;
        private int _rectVao, _rectVbo;
        private int _lineVao, _lineVbo;
        private bool _initialized;
        private Brush _bgColor = Brushes.Transparent;
        private readonly TextureManager _textureManager;
        private int SdfFontTextureId => _textureManager.SdfFont?.TextureId ?? -1;

        public BitmapSource LastCapturedImage { get; private set; }

        public OglRenderer(TextureManager texManager)
        {
            _textureManager = texManager ?? throw new ArgumentNullException(nameof(texManager));
        }

        public void Initialize()
        {
            if (_initialized) return;

            // Load Fonts
            _textureManager.SetDefaultFont();

            // Load Shader
            _shaderProgram = CompileShaderProgram();

            // Set Background Color
            _bgColor = (Brush)System.Windows.Application.Current.Resources["PrimaryLightBrush"];

            // Line Smoothing and Opacity
            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // depth testing (zIndex)
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal); // Lower Z = further, higher Z = in front

            // Setup rectangle VAO/VBO
            GL.GenVertexArrays(1, out _rectVao);
            GL.BindVertexArray(_rectVao);
            GL.GenBuffers(1, out _rectVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _rectVbo);

            int strideR = 9 * sizeof(float); // pos (3) + color (4) + texCoord (2)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, strideR, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, strideR, 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, strideR, 7 * sizeof(float));

            GL.BindVertexArray(0);

            // Setup line VAO/VBO
            GL.GenVertexArrays(1, out _lineVao);
            GL.BindVertexArray(_lineVao);
            GL.GenBuffers(1, out _lineVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);

            int strideL = 10 * sizeof(float); // pos (3) + color (4) + dash (2) + distance (1)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, strideL, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, strideL, 3 * sizeof(float));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, strideL, 7 * sizeof(float));
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, strideL, 9 * sizeof(float));

            GL.BindVertexArray(0);

            _initialized = true;
        }

        public void Render(float[] rectVertices, float[] lineVertices,
                           List<(int? TextureId, int Count)> rectBatches,
                           List<(float LineWidth, int Count)> lineBatches,
                           Matrix4 projectionMatrix,
                           Vector4? clearColorOverride = null)
        {
            // Clear screen
            var bg = clearColorOverride ?? _bgColor.ToVectorColor();
            GL.ClearColor(bg.X, bg.Y, bg.Z, bg.W);

            // Use Z-Buffer for depth Sorting
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uProjection"), false, ref projectionMatrix);
            int uUseHatch = GL.GetUniformLocation(_shaderProgram, "useHatchPattern");
            int uHatchScale = GL.GetUniformLocation(_shaderProgram, "hatchScale");
            int uTex0 = GL.GetUniformLocation(_shaderProgram, "texture0");
            GL.Uniform1(uTex0, 0);

            int uIsSdfText = GL.GetUniformLocation(_shaderProgram, "isSdfText");
            int uSdfFont = GL.GetUniformLocation(_shaderProgram, "sdfFont");
            GL.Uniform1(uSdfFont, 1); // Bind SDF to texture unit 1

            // Rectangles
            if (rectVertices.Length > 0)
            {
                GL.BindVertexArray(_rectVao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _rectVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, rectVertices.Length * sizeof(float), rectVertices, BufferUsageHint.DynamicDraw);

                int offset = 0;
                foreach (var batch in rectBatches)
                {
                    if (batch.TextureId.HasValue)
                    {
                        if (batch.TextureId.Value == SdfFontTextureId)
                        {
                            GL.ActiveTexture(TextureUnit.Texture1);
                            GL.BindTexture(TextureTarget.Texture2D, batch.TextureId.Value); // sdfFont goes to unit 1
                            GL.Uniform1(uUseHatch, 0);
                            GL.Uniform1(uIsSdfText, 1);
                        }
                        else
                        {
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, batch.TextureId.Value);
                            GL.Uniform1(uUseHatch, 1);
                            GL.Uniform1(uHatchScale, 1.0f);
                        }
                    }
                    else
                    {
                        GL.Uniform1(uUseHatch, 0);
                        GL.Uniform1(uIsSdfText, 0);
                    }
                    GL.DrawArrays(PrimitiveType.Triangles, offset, batch.Count);
                    offset += batch.Count;
                }
                GL.BindVertexArray(0);
            }

            // Lines
            if (lineVertices.Length > 0)
            {
                GL.BindVertexArray(_lineVao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, lineVertices.Length * sizeof(float), lineVertices, BufferUsageHint.DynamicDraw);
                GL.Uniform1(uUseHatch, 0);
                GL.Uniform1(uIsSdfText, 0);

                int vertexOffset = 0;
                foreach (var batch in lineBatches)
                {
                    GL.LineWidth(batch.LineWidth);
                    GL.DrawArrays(PrimitiveType.Lines, vertexOffset, batch.Count);
                    vertexOffset += batch.Count;
                }
                GL.BindVertexArray(0);
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_rectVbo);
            GL.DeleteBuffer(_lineVbo);
            GL.DeleteVertexArray(_rectVao);
            GL.DeleteVertexArray(_lineVao);
            GL.DeleteProgram(_shaderProgram);
        }

        #region Image capturing

        //public BitmapSource CaptureRendering(
        //    float[] rectVertices,
        //    float[] lineVertices,
        //    List<(int? TextureId, int Count)> rectBatches,
        //    List<(float LineWidth, int Count)> lineBatches,
        //    Matrix4 projection,
        //    int width,
        //    int height,
        //    float dpi = 96f)
        //{
        //    // FBO setup block

        //    int fbo, tex;

        //    // Generate framebuffer
        //    GL.GenFramebuffers(1, out fbo);
        //    GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        //    // Create color texture
        //    GL.GenTextures(1, out tex);
        //    GL.BindTexture(TextureTarget.Texture2D, tex);
        //    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        //    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, tex, 0);

        //    if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        //        throw new Exception("Failed to create framebuffer");
        
        //    GL.Viewport(0, 0, width, height);
        //    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        //    // Render to FBO using the existing Render logic
        //    Render(rectVertices, lineVertices, rectBatches, lineBatches, projection, new Vector4(0f, 0f, 0f, 0f));

        //    // Read pixels back
        //    byte[] pixels = new byte[width * height * 4];
        //    GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        //    // Cleanup
        //    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        //    GL.DeleteFramebuffer(fbo);
        //    GL.DeleteTexture(tex);

        //    var bmp = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixels, width * 4);
        //    bmp.Freeze();
        //    LastCapturedImage = FlipVertically(bmp);
        //    return LastCapturedImage;
        //}

        public BitmapSource CaptureRendering(
            float[] rectVertices,
            float[] lineVertices,
            List<(int? TextureId, int Count)> rectBatches,
            List<(float LineWidth, int Count)> lineBatches,
            Matrix4 projection,
            int width,
            int height,
            float dpi = 96f)
        {
            // FBO setup block

            int fbo, tex, rbo;

            // Generate framebuffer
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            // Create color texture
            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            // Use a specific internal format
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, tex, 0);

            // Create and attach a depth renderbuffer (required for depth testing)
            GL.GenRenderbuffers(1, out rbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rbo);

            // Ensure we draw to color attachment 0
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // Check completeness
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                // Clean up any allocated resources before throwing
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                if (tex != 0) GL.DeleteTexture(tex);
                if (rbo != 0) GL.DeleteRenderbuffer(rbo);
                if (fbo != 0) GL.DeleteFramebuffer(fbo);
                throw new Exception($"Failed to create framebuffer: {status}");
            }

            // Setup viewport and clear (explicit depth clear)
            GL.Viewport(0, 0, width, height);
            GL.ClearDepth(1.0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Render to FBO using the existing Render logic
            Render(rectVertices, lineVertices, rectBatches, lineBatches, projection, new Vector4(0f, 0f, 0f, 0f));

            // sets the pack alignment to 1 for safety (avoids row-padding surprises)
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            // Read pixels back
            byte[] pixels = new byte[width * height * 4];
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            // clean up textures/renderbuffers
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(fbo);
            GL.DeleteTexture(tex);
            GL.DeleteRenderbuffer(rbo);

            var bmp = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixels, width * 4);
            bmp.Freeze();
            LastCapturedImage = FlipVertically(bmp);
            return LastCapturedImage;
        }

        private static BitmapSource FlipVertically(BitmapSource source)
        {
            var transform = new ScaleTransform(1, -1, 0.5, 0.5);
            var tb = new TransformedBitmap(source, transform);
            tb.Freeze();
            return tb;
        }

        #endregion


        private int CompileShaderProgram()
        {
            string vertexShaderSource = File.ReadAllText("./Services/UI/OpenGL/layer.vert");
            string fragmentShaderSource = File.ReadAllText("./Services/UI/OpenGL/layer.frag");

            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, vertexShaderSource);
            GL.CompileShader(vertShader);

            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragmentShaderSource);
            GL.CompileShader(fragShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertShader);
            GL.AttachShader(program, fragShader);
            GL.LinkProgram(program);

            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);

            return program;
        }
    }
}
