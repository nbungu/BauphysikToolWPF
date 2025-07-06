using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BT.Geometry;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Manages a collection of layers and handles their rendering lifecycle.
    /// Automatically rebuilds the geometry buffer when layers change.
    /// </summary>
    public class ElementScene : IDisposable
    {
        private CrossSectionDrawing _crossSectionBuilder = new CrossSectionDrawing();
        private readonly List<IDrawingGeometry> _layers = new();
        private bool _needsRebuild = true;
        private bool _disposed;
        private Matrix4 _projectionMatrix;

        private int _prgHandle;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private List<float> _vertices = new(); // Interleaved position + color
        private int _vertexCount;

        public void UseElement(Element element)
        {
            _layers.Clear();
            _crossSectionBuilder.Element = element;
            _crossSectionBuilder.DrawWithLayerLabels = false;
            _crossSectionBuilder.UpdateDrawings();
            
            AddToLayerCollection(element.Layers);
        }

        public void Initialize()
        {
            _prgHandle = GL.CreateProgram();

            #region Shader

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "UI", "OpenGL");
            string vertexShaderSource = File.ReadAllText(Path.Combine(basePath, "layer.vert"));
            string fragmentShaderSource = File.ReadAllText(Path.Combine(basePath, "layer.frag"));

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(vertexShader);
                Debug.WriteLine($"Vertex shader compile error: {log}");
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(fragmentShader);
                Debug.WriteLine($"Fragment shader compile error: {log}");
            }

            GL.AttachShader(_prgHandle, vertexShader);
            GL.AttachShader(_prgHandle, fragmentShader);
            GL.LinkProgram(_prgHandle);
            GL.GetProgram(_prgHandle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string log = GL.GetProgramInfoLog(_prgHandle);
                Debug.WriteLine($"Program link error: {log}");
            }

            GL.DetachShader(_prgHandle, vertexShader);
            GL.DetachShader(_prgHandle, fragmentShader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            #endregion

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Initial dummy allocation
            GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            int positionLocation = GL.GetAttribLocation(_prgHandle, "vPosition"); // location = 0
            int colorLocation = GL.GetAttribLocation(_prgHandle, "vColor"); // location = 1

            int stride = 7 * sizeof(float); // 3 for position, 4 for color

            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        }

        /// <summary>
        /// Renders the scene.
        /// Rebuilds the geometry if layers have changed.
        /// </summary>
        public void Render()
        {
            if (_needsRebuild)
            {
                _crossSectionBuilder.UpdateDrawings();
                RebuildOglGeometry();
                _needsRebuild = false;
            }

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_prgHandle);

            int projLocation = GL.GetUniformLocation(_prgHandle, "uProjection");
            GL.UniformMatrix4(projLocation, false, ref _projectionMatrix);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
        }
        
        /// <summary>
        /// Updates the projection matrix used to transform screen-space layer positions
        /// into Normalized Device Coordinates (NDC) for rendering. This ensures that
        /// layer rectangles (defined in pixel units) appear at the correct positions
        /// when rendered by the GPU.
        /// </summary>
        public void UpdateProjection(Size controlSize)
        {
            var contentBounds = GetContentBounds();

            float contentWidth = (float)contentBounds.Width;
            float contentHeight = (float)contentBounds.Height;
            float controlWidth = (float)controlSize.Width;
            float controlHeight = (float)controlSize.Height;

            if (contentWidth <= 0 || contentHeight <= 0 || controlWidth <= 0 || controlHeight <= 0)
            {
                _projectionMatrix = Matrix4.Identity;
                return;
            }

            //Matrix4 view = Matrix4.CreateTranslation(1.0f, -1.0f, 0); // Aligns top Left of the whole element to the center point of the render area
            //Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, 0); // Aligns top Left of the whole element to the top Left of the render area
            //Matrix4 view = Matrix4.CreateScale(1, -1, 1); // flips Y axis
            //Matrix4 view = Matrix4.CreateScale(0.5f, 0.5f, 1f); // scales to 50%

            #region Scale to fit view

            // Calculate scale to fit (preserve aspect ratio)
            float scaleX = controlWidth / contentWidth;
            float scaleY = controlHeight / contentHeight;
            float scale = MathF.Min(scaleX, scaleY); // Uniform scale

            // Apply scaling only when origin of the element (top left) is in the center of the control and then translate back to the top left corner of the control
            Matrix4 view = Matrix4.CreateTranslation(1.0f, -1.0f, 0) * Matrix4.CreateScale(scale, scale, 1) * Matrix4.CreateTranslation(-1.0f, 1.0f, 0);

            #endregion

            #region Translating to center of content

            float offsetX = 0.0f;
            if (contentWidth < controlWidth)
            {
                var absoluteOffsetInControl = (controlWidth - contentWidth * scale) / 2f;
                var baseUnitDivisor = controlWidth / 2; // x-Axis: translation of +1.0f from origin is 50% of the control width
                var converted = absoluteOffsetInControl / baseUnitDivisor; // Convert to OpenGL units
                offsetX = converted; // Center content horizontally
            }

            float offsetY = 0.0f;
            if (contentHeight < controlHeight)
            {
                var absoluteOffsetInControl = (controlHeight - contentHeight * scale) / 2f;
                var baseUnitDivisor = controlHeight / 2; // x-Axis: translation of +1.0f from origin is 50% of the control width
                var converted = absoluteOffsetInControl / baseUnitDivisor; // Convert to OpenGL units
                offsetY = converted; // Center content horizontally
            }
            view *= Matrix4.CreateTranslation(offsetX, -offsetY, 0); // Move to control center

            #endregion
            
            // Final projection maps screen units to [-1, 1] NDC
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, controlWidth, controlHeight, 0, -1, 1) * view;
        }


        /// <summary>
        /// Disposes resources used by the scene and its renderer.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Clean up OpenGL resources
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            // Shader
            GL.DeleteProgram(_prgHandle);
            _disposed = true;
        }

        #region private

        private void AddToLayerCollection(IEnumerable<IDrawingGeometry> layers)
        {
            _layers.AddRange(layers);
            _layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            _needsRebuild = true;
        }

        private void RebuildOglGeometry()
        {
            List<float> vertexData = new();

            foreach (var layer in _layers)
            {
                var rect = DrawRectangle(layer.Rectangle, layer.BackgroundColor);
                
                vertexData.AddRange(rect);
            }

            _vertices = vertexData;
            _vertexCount = vertexData.Count / 7;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.DynamicDraw);
        }

        private Rect GetContentBounds()
        {
            if (_layers.Count == 0)
                return new Rect(0, 0, 1, 1); // Prevent divide by zero

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var layer in _layers)
            {
                var r = layer.Rectangle;
                minX = MathF.Min(minX, (float)r.X);
                minY = MathF.Min(minY, (float)r.Y);
                maxX = MathF.Max(maxX, (float)r.X + (float)r.Width);
                maxY = MathF.Max(maxY, (float)r.Y + (float)r.Height);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private float[] DrawRectangle(Rectangle rectangle, Brush backgroundColor)
        {
            float x = (float)rectangle.X;
            float y = (float)rectangle.Y;
            float w = (float)rectangle.Width;
            float h = (float)rectangle.Height;

            var c = new Vector4(0, 0, 0, 0);
            if (backgroundColor is SolidColorBrush solidColor)
            {
                var solidColorc = solidColor.Color;
                c = new Vector4(solidColorc.R / 255f, solidColorc.G / 255f, solidColorc.B / 255f, solidColorc.A / 255f);
            }

            float[] rect =
            {
                // First triangle
                x,     y,     0f,  c.X, c.Y, c.Z, c.W,
                x + w, y,     0f,  c.X, c.Y, c.Z, c.W,
                x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,

                // Second triangle
                x + w, y,     0f,  c.X, c.Y, c.Z, c.W,
                x + w, y + h, 0f,  c.X, c.Y, c.Z, c.W,
                x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,
            };
            return rect;
        }

        #endregion

    }
}
