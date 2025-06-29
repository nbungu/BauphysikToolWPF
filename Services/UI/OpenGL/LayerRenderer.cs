using BauphysikToolWPF.Models.Domain;
using LiveChartsCore.Measure;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class LayerRenderer
    {
        private readonly Shader _shader;
        private readonly int _vertexBufferObject, _vertexArrayObject;
        // the amount of triangle rotation at each frame
        private double _rotationDegrees;
        private float _totalScaleFactor = 1f;
        private double _translateX, _translateY;
        private Matrix4 _scale = Matrix4.Identity;
        private Matrix4 _translate = Matrix4.Identity;

        private bool _isSpinStopped = true;

        // OpenGL objects TESTIJG ONLY
        private readonly float[] _vertices =
        {
            // positions        // colors
            0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
            0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
        };

        public LayerRenderer(Shader shader)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.5f);

            _shader = shader;


            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // how to interpret the vertex buffer data
            // You should do this setup once (e.g., in constructor) after generating the VAO and VBO, or each frame before drawing if you’re uploading dynamic data.

            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            //GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shader.Use();
        }

        public void Render(Layer layer, Matrix4 projection)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0f, 0f, 0f, 1f);

            if (_shader is null)
            {
                return;
            }
            _shader.Use();

            
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            //// Generate rectangle vertices for the given layer
            //var vertices = CreateRectangleVertices(layer);
            //GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);


            //Matrix4 model = Matrix4.CreateScale(layer.RectangleF.Width, layer.RectangleF.Height, 1f) *
            //                Matrix4.CreateTranslation(layer.RectangleF.X, layer.RectangleF.Y, 0);

            //Matrix4 mvp = projection * model; //model * projection;
            //GL.UniformMatrix4(_shader.GetUniformLocation("uMVP"), false, ref mvp);

            //int colorUniformLocation = GL.GetUniformLocation(_shader.Handle, "uColor");
            //GL.Uniform4(colorUniformLocation, layer.BackgroundColorVector);
            

            //GL.BindVertexArray(_vertexArrayObject);

            // Draw
            //GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);



            //SetRotationDegrees(args.Time);
            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)_rotationDegrees));
            Matrix4 transform = rotation * _scale * _translate;
            int transformLocation = GL.GetUniformLocation(_shader.Handle, "aTransform");
            GL.UniformMatrix4(transformLocation, true, ref transform);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            // Optional: Render borders with line shader or same shader using GL_LINE_LOOP

            //GL.BindVertexArray(0);
        }

        private float[] CreateRectangleVertices(Layer layer)
        {
            // Define 4 corner vertices of the rectangle
            return new float[]
            {
                0, 0, 0,
                1, 0, 0,
                0, 1, 0,
                1, 1, 0
            };
        }

        private void SetRotationDegrees(double deltaTime)
        {
            if (_isSpinStopped)
            {
                return;
            }
            // the triangle should rotate 20 degrees every second; based on the time elapsed since last frame,
            // calculate the rotation amount
            _rotationDegrees += deltaTime * 20.0;
            _rotationDegrees -= Math.Truncate(_rotationDegrees / 360.0) * 360.0; // take remainder of div by 360
        }
    }
}
