using BauphysikToolWPF.Models.Domain;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Handles rendering of multiple rectangular Layers in a single batched draw call using OpenGL.
    /// Each Layer is represented as a rectangle made of two triangles (6 vertices),
    /// with position and color attributes packed into a vertex buffer.
    /// The geometry is rebuilt whenever the list of layers changes.
    /// </summary>
    public class LayerRenderer : IDisposable
    {
        private readonly Shader _shader;
        private readonly int _vertexBufferObject, _vertexArrayObject;
        private int _vertexCount; // Number of vertices currently buffered (6 vertices per rectangle)

        /// <summary>
        /// Creates a LayerRenderer with a specified Shader.
        /// Initializes VAO and VBO with position and color vertex attributes.
        /// </summary>
        /// <param name="shader">The shader program used for rendering layers.</param>
        public LayerRenderer(Shader shader)
        {
            _shader = shader;

            // Set background clear color for the rendering context
            GL.ClearColor(0f, 0f, 0f, 1f);

            // Generate and bind the vertex buffer object (VBO)
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Generate and bind the vertex array object (VAO)
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Define the layout of the vertex buffer:
            // Each vertex consists of:
            // - Position: 3 floats (x, y, z)
            // - Color: 4 floats (r, g, b, a)
            // Total stride: 7 floats per vertex
            int stride = 7 * sizeof(float);

            // Position attribute (location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute (location = 1)
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Unbind VAO to avoid accidental modification
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Rebuilds the vertex buffer by batching all layers into one vertex array.
        /// This method should be called whenever the list of layers changes (added, removed, or modified).
        /// Positions are stored in screen or world coordinates, colors come from each layer's background color.
        /// </summary>
        /// <param name="layers">The list of layers to batch into the vertex buffer.</param>
        public void RebuildGeometry(List<Layer> layers)
        {
            List<float> vertexData = new();

            foreach (var layer in layers)
            {
                // Update cached brush color vector from layer's BackgroundColor property
                layer.UpdateBrushCache();

                var x = layer.RectangleF.X;
                var y = layer.RectangleF.Y;
                var w = layer.RectangleF.Width;
                var h = layer.RectangleF.Height;

                var c = layer.BackgroundColorVector;

                // Each rectangle is composed of two triangles (6 vertices),
                // each vertex has position (x,y,z) and color (r,g,b,a).
                float[] rect = new float[]
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

            // Calculate total vertex count (7 floats per vertex)
            _vertexCount = vertexData.Count / 7;

            // Bind VAO and VBO to upload new vertex data to GPU
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Upload vertex data (dynamic draw since data changes occasionally)
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(float), vertexData.ToArray(), BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        /// Renders all batched layers using a single draw call.
        /// The projection matrix is applied uniformly to all vertices.
        /// </summary>
        /// <param name="projection">Projection matrix (typically orthographic or perspective) to transform vertex positions.</param>
        public void Render(Matrix4 projection)
        {
            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);

            // Upload the projection matrix (no separate model matrices since positions are pre-transformed)
            int mvpLocation = _shader.GetUniformLocation("uMVP");
            GL.UniformMatrix4(mvpLocation, false, ref projection);

            // Draw all vertices as triangles in a single call
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);

            // Optional: Unbind VAO (not strictly required)
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            // Clean up OpenGL resources
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            _shader?.Dispose(); // If Shader implements IDisposable
        }
    }
}
