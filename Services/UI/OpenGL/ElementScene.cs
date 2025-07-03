using BauphysikToolWPF.Models.Domain;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using BauphysikToolWPF.Models.UI;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Manages a collection of layers and handles their rendering lifecycle.
    /// Automatically rebuilds the geometry buffer when layers change.
    /// </summary>
    public class ElementScene : IDisposable
    {
        private readonly List<IDrawingGeometry> _layers = new();
        private bool _needsRebuild = true;
        private bool _disposed;
        private Matrix4 _projectionMatrix;

        private int _prgHandle;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private List<float> _vertices = new(); // Interleaved position + color
        private int _vertexCount;

        /// <summary>
        /// Adds multiple layers to the scene and marks the geometry for rebuilding.
        /// </summary>
        /// <param name="layers">The layers to add.</param>
        public void UseLayers(IEnumerable<Layer> layers)
        {
            _layers.Clear();
            AddToLayerCollection(layers);
        }

        public void UseDrawingGeometries(IEnumerable<IDrawingGeometry> geometries)
        {
            _layers.Clear();
            AddToLayerCollection(geometries);
        }

        public void UseElement(Element element)
        {
            _layers.Clear();
            AddToLayerCollection(element.Layers);
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
                RebuildGeometry();
                _needsRebuild = false;
            }

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_prgHandle);

            int projLocation = GL.GetUniformLocation(_prgHandle, "uProjection");
            GL.UniformMatrix4(projLocation, false, ref _projectionMatrix);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
        }

        private void AddToLayerCollection(IEnumerable<IDrawingGeometry> layers)
        {
            _layers.AddRange(layers);
            _layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            _needsRebuild = true;
        }

        private void RebuildGeometry()
        {
            List<float> vertexData = new();

            foreach (var layer in _layers)
            {
                layer.UpdateBrushCache(); // Assuming this sets BackgroundColorVector

                float x = layer.RectangleF.X;
                float y = layer.RectangleF.Y;
                float w = layer.RectangleF.Width;
                float h = layer.RectangleF.Height;

                var c = layer.BackgroundColorVector;

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

                vertexData.AddRange(rect);
            }

            _vertices = vertexData;
            _vertexCount = vertexData.Count / 7;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        /// Updates the projection matrix used to transform screen-space layer positions
        /// into Normalized Device Coordinates (NDC) for rendering. This ensures that
        /// layer rectangles (defined in pixel units) appear at the correct positions
        /// when rendered by the GPU.
        /// </summary>
        public void UpdateProjection(Rect bounds)
        {
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(
                0, (float)bounds.X,   // left to right
                (float)bounds.Y, 0,  // bottom to top (flipped Y)
                -1, 1       // near to far
            );
        }
    }
}
