using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class ElementRenderer : IDisposable
    {
        private readonly ElementSceneController _parent;

        private int _shaderProgram;
        private int _rectVao, _rectVbo;
        private int _lineVao, _lineVbo;
        private bool _initialized;
        private Brush _bgColor = Brushes.Transparent;

        private TextureManager TextureManager => _parent.TextureManager;
        private int SdfFontTextureId => TextureManager.SdfFont?.TextureId ?? -1;

        public ElementRenderer(ElementSceneController parent)
        {
            _parent = parent;
        }

        public void Initialize()
        {
            if (_initialized) return;

            // Load Fonts
            TextureManager.SetDefaultFont();

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
                           Matrix4 projectionMatrix)
        {
            // Clear screen
            var bg = SceneBuilder.GetColorFromBrush(_bgColor);
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
