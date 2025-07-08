using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using BT.Geometry;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Manages a collection of layers and handles their rendering lifecycle.
    /// Automatically rebuilds the geometry buffer when layers change.
    /// </summary>
    public class ElementScene : IDisposable
    {
        public event Action<ShapeId>? ShapeHovered;
        public event Action<ShapeId>? ShapeClicked;
        // ShapeClicked?.Invoke(shape.ShapeId);

        private Rectangle? _hoveredRectangle;
        private string? _hoveredTooltip;

        private readonly CrossSectionBuilder _crossSectionBuilder = new();
        private bool _needsRebuild = true;

        private bool _isDragging = false;
        private System.Windows.Point _lastMousePosition;
        private Vector2 _panOffset = Vector2.Zero;
        private bool _disposed;
        private bool _drawLines;
        private Matrix4 _projectionMatrix;
        private int _prgHandle;

        // VAO can stay as-is for both your geometry (rectangles) and your new dashed lines, as long as:
        // All attributes(position, color, dashCoord) are enabled and configured before each draw call.
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private List<float> _vertices = new(); // Interleaved position + color
        private int _vertexCount;
        private int _lineVertexBuffer;
        private int _lineVertexCount;

        private const double ZoomStep = 0.1;
        private const double MinZoom = 0.5;
        private const double MaxZoom = 5.0;

        public double ZoomFactor { get; private set; } = 1.0;
        public GLWpfControl View { get; private set; }
        public GLWpfControlSettings ViewSettings { get; private set; }

        public void ConnectToView(GLWpfControl view, GLWpfControlSettings? settings = null)
        {
            Session.SelectedLayerChanged += View_OnElementChanged;
            Session.SelectedElementChanged += View_OnElementChanged;

            View = view;
            View.Render += View_OnRender;
            View.MouseWheel += View_OnMouseWheel;
            View.MouseRightButtonUp += View_OnMouseRightClick;
            View.MouseDown += View_OnMouseDown;
            View.MouseUp += View_OnMouseUp;
            View.MouseMove += View_OnMouseMove;
            //View.MouseLeftButtonDown += View_OnMouseLeftClick;

            ViewSettings = settings ?? new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3,
                RenderContinuously = false,
            };
            View.Start(ViewSettings);
            
            Initialize();
        }

        public void UseElement(Element element)
        {
            _crossSectionBuilder.Element = element;
            _crossSectionBuilder.DrawWithLayerLabels = false;
            _crossSectionBuilder.RebuildCrossSection();
            _needsRebuild = true;
        }
        
        /// <summary>
        /// Renders the scene.
        /// Rebuilds the geometry if layers have changed.
        /// </summary>
        public void Render()
        {
            if (_needsRebuild)
            {
                _crossSectionBuilder.RebuildCrossSection();
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

            // TODO: Currently not workiung
            // Draw lines if any
            //if (_drawLines && _lineVertexCount > 1)
            //{
            //    GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVertexBuffer);
            

            //    int stride = 8 * sizeof(float); // 3 pos + 4 color + 1 dashCoord

            //    int positionLocation = GL.GetAttribLocation(_prgHandle, "vPosition");
            //    GL.EnableVertexAttribArray(positionLocation);
            //    GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, stride, 0);

            //    int colorLocation = GL.GetAttribLocation(_prgHandle, "vColor");
            //    GL.EnableVertexAttribArray(colorLocation);
            //    GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            //    // NEW: dashCoord attribute
            //    int dashCoordLocation = GL.GetAttribLocation(_prgHandle, "vDashCoord");
            //    GL.EnableVertexAttribArray(dashCoordLocation);
            //    GL.VertexAttribPointer(dashCoordLocation, 1, VertexAttribPointerType.Float, false, stride, 7 * sizeof(float));


            //    GL.LineWidth(0.8f);
            //    GL.DrawArrays(PrimitiveType.Lines, 0, _lineVertexCount);
            //    // LineStrip: Connects points in sequence (0→1→2→...).
            //    // Lines: Treats every pair of vertices as an independent line (0→1, 2→3, ...)
            //    //LineLoop: Like LineStrip, but connects the last point back to the first.
            //}
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
            //float scale = MathF.Min(scaleX, scaleY); // Uniform scale
            float scale = MathF.Min(scaleX, scaleY) * (float)ZoomFactor;

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

            // Mouse wheel click panning offset
            view *= Matrix4.CreateTranslation(_panOffset.X, _panOffset.Y, 0);

            #endregion

            // Final projection maps screen units to [-1, 1] NDC
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, controlWidth, controlHeight, 0, -1, 1) * view;
        }

        public void ZoomIn()
        {
            ZoomFactor = Math.Min(ZoomFactor + ZoomStep, MaxZoom);
            UpdateProjection(new Size(View.ActualWidth, View.ActualHeight));
            View.InvalidateVisual();
        }

        public void ZoomOut()
        {
            ZoomFactor = Math.Max(ZoomFactor - ZoomStep, MinZoom);
            UpdateProjection(new Size(View.ActualWidth, View.ActualHeight));
            View.InvalidateVisual();
        }

        public void ResetZoom()
        {
            ZoomFactor = 1.0; // Reset Zoom
            _panOffset = Vector2.Zero; // Reset pan offset
            UpdateProjection(new Size(View.ActualWidth, View.ActualHeight));
            View.InvalidateVisual();
        }

        /// <summary>
        /// Disposes resources used by the scene and its renderer.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (View != null)
            {
                View.Render -= View_OnRender;
                View.MouseWheel -= View_OnMouseWheel;
                View.MouseRightButtonUp -= View_OnMouseRightClick;
                View.MouseDown -= View_OnMouseDown;
                View.MouseUp -= View_OnMouseUp;
                View.MouseMove -= View_OnMouseMove;
                //View.MouseLeftButtonUp -= View_OnMouseLeftClick;
            }

            Session.SelectedLayerChanged -= View_OnElementChanged;
            Session.SelectedElementChanged -= View_OnElementChanged;

            if (_vertexBufferObject != 0)
                GL.DeleteBuffer(_vertexBufferObject);
            if (_lineVertexBuffer != 0)
                GL.DeleteBuffer(_lineVertexBuffer);
            if (_vertexArrayObject != 0)
                GL.DeleteVertexArray(_vertexArrayObject);
            //if (_lineVertexArrayObject != 0)
            //    GL.DeleteVertexArray(_lineVertexArrayObject);
            if (_prgHandle != 0)
                GL.DeleteProgram(_prgHandle);

            _disposed = true;
        }

        #region private

        private void Initialize()
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

            _lineVertexBuffer = GL.GenBuffer();

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

        private void RebuildOglGeometry()
        {
            List<float> vertexData = new();

            foreach (var layer in _crossSectionBuilder.DrawingGeometries)
            {
                // TODO: HERE?
                layer.ShapeId = new ShapeId(ShapeType.Layer, layer.InternalId);

                var rect = SetRectangle(layer.Rectangle, layer.BackgroundColor);
                vertexData.AddRange(rect);
            }

            _vertices = vertexData;
            _vertexCount = vertexData.Count / 7;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.DynamicDraw);

            Vector2[] points = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 1),
            };
            SetLines(points, Brushes.Blue);
        }

        private Rect GetContentBounds()
        {
            if (_crossSectionBuilder.DrawingGeometries.Count == 0)
                return new Rect(0, 0, 1, 1); // Prevent divide by zero

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var layer in _crossSectionBuilder.DrawingGeometries)
            {
                var r = layer.Rectangle;
                minX = MathF.Min(minX, (float)r.X);
                minY = MathF.Min(minY, (float)r.Y);
                maxX = MathF.Max(maxX, (float)r.X + (float)r.Width);
                maxY = MathF.Max(maxY, (float)r.Y + (float)r.Height);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private float[] SetRectangle(Rectangle rectangle, Brush backgroundColor)
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

        //private void AddDashedRect(List<float> vertices, Rectangle rect, Vector4 color)
        //{
        //    // Add vertices for the four edges with dashes
        //    // We can create multiple short line segments to simulate dashes

        //    // Helper: Add line segment
        //    void AddLineSegment(float x1, float y1, float x2, float y2)
        //    {
        //        vertices.AddRange(new float[] { x1, y1, 0f, color.X, color.Y, color.Z, color.W });
        //        vertices.AddRange(new float[] { x2, y2, 0f, color.X, color.Y, color.Z, color.W });
        //    }

        //    float dashLength = 10f;
        //    float gapLength = 5f;

        //    // Top edge: from Left to Right
        //    for (float x = (float)rect.Left; x < (float)rect.Right; x += dashLength + gapLength)
        //    {
        //        float x2 = Math.Min(x + dashLength, (float)rect.Right);
        //        AddLineSegment(x, (float)rect.Top, x2, (float)rect.Top);
        //    }

        //    // Right edge: from Top to Bottom
        //    for (float y = (float)rect.Top; y > (float)rect.Bottom; y -= dashLength + gapLength)
        //    {
        //        float y2 = Math.Max(y - dashLength, (float)rect.Bottom);
        //        AddLineSegment((float)rect.Right, y, (float)rect.Right, y2);
        //    }

        //    // Bottom edge: from Right to Left
        //    for (float x = (float)rect.Right; x > (float)rect.Left; x -= dashLength + gapLength)
        //    {
        //        float x2 = Math.Max(x - dashLength, (float)rect.Left);
        //        AddLineSegment(x, (float)rect.Bottom, x2, (float)rect.Bottom);
        //    }

        //    // Left edge: from Bottom to Top
        //    for (float y = (float)rect.Bottom; y < (float)rect.Top; y += dashLength + gapLength)
        //    {
        //        float y2 = Math.Min(y + dashLength, (float)rect.Top);
        //        AddLineSegment((float)rect.Left, y, (float)rect.Left, y2);
        //    }
        //}

        public void SetLines(Vector2[] lineVertices, Brush lineColor)
        {
            _lineVertexCount = lineVertices.Length;
            _drawLines = true;

            var c = new Vector4(0, 0, 0, 0);
            if (lineColor is SolidColorBrush solidColor)
            {
                var solidColorc = solidColor.Color;
                c = new Vector4(solidColorc.R / 255f, solidColorc.G / 255f, solidColorc.B / 255f, solidColorc.A / 255f);
            }

            float[] interleaved = new float[_lineVertexCount * 8]; // 3 pos, 4 color, 1 dashCoord

            float totalDistance = 0f;
            for (int i = 0; i < lineVertices.Length; i++)
            {
                if (i > 0)
                {
                    totalDistance += (lineVertices[i] - lineVertices[i - 1]).Length;
                }

                interleaved[i * 8 + 0] = lineVertices[i].X;
                interleaved[i * 8 + 1] = lineVertices[i].Y;
                interleaved[i * 8 + 2] = 0f; // Z

                interleaved[i * 8 + 3] = c.X;
                interleaved[i * 8 + 4] = c.Y;
                interleaved[i * 8 + 5] = c.Z;
                interleaved[i * 8 + 6] = c.W;

                interleaved[i * 8 + 7] = totalDistance; // dash progress
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, interleaved.Length * sizeof(float), interleaved, BufferUsageHint.StreamDraw);
        }

        private void View_OnElementChanged()
        {
            this.UseElement(Session.SelectedElement);
            this.UpdateProjection(new Size(View.ActualWidth, View.ActualHeight));
            View.InvalidateVisual(); // Force re-render of OpenGL Control programatically
        }

        private void View_OnRender(TimeSpan delta)
        {
            this.UpdateProjection(new Size(View.ActualWidth, View.ActualHeight));
            this.Render();
        }

        private void View_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ZoomIn();
            else ZoomOut();
        }
        private void View_OnMouseRightClick(object sender, MouseButtonEventArgs e)
        {
            ResetZoom();
        }

        private void View_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(View);
                View.CaptureMouse();
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }

            if (e.ChangedButton == MouseButton.Left && !_isDragging)
            {
               var scenePoint = ConvertMouseToScene(e.GetPosition(View));

                foreach (var shape in _crossSectionBuilder.DrawingGeometries)
                {
                    if (shape.Rectangle.Contains(scenePoint))
                    {
                        Debug.WriteLine($"Clicked shape: {shape.ShapeId}");
                        ShapeClicked?.Invoke(shape.ShapeId);
                        break;
                    }
                }
            }
            if (e.ChangedButton == MouseButton.Right && !_isDragging)
            {
                var scenePoint = ConvertMouseToScene(e.GetPosition(View));

                foreach (var shape in _crossSectionBuilder.DrawingGeometries)
                {
                    if (shape.Rectangle.Contains(scenePoint))
                    {
                        Debug.WriteLine($"Right Clicked shape: {shape.ShapeId}");
                        ShapeClicked?.Invoke(shape.ShapeId);
                        break;
                    }
                }
            }
        }

        private void View_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _isDragging = false;
                View.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }

        private void View_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                System.Windows.Point currentPos = e.GetPosition(View);
                System.Windows.Vector delta = currentPos - _lastMousePosition;
                _lastMousePosition = currentPos;

                // Convert delta from screen pixels to OpenGL NDC units:
                float ndcX = (float)(2.0 * delta.X / View.ActualWidth);
                float ndcY = (float)(-2.0 * delta.Y / View.ActualHeight); // Y is inverted in OpenGL

                _panOffset += new Vector2(ndcX, ndcY);

                UpdateProjection(new Size(View.ActualWidth, View.ActualHeight));
                View.InvalidateVisual();
            }


            // Hit testing for Interactive View

            var scenePoint = ConvertMouseToScene(e.GetPosition(View));

            foreach (var shape in _crossSectionBuilder.DrawingGeometries)
            {
                if (shape.Rectangle.Contains(scenePoint))
                {
                    _hoveredRectangle = shape.Rectangle;
                    _hoveredTooltip = shape.ShapeId.ToString();

                    Mouse.OverrideCursor = Cursors.Hand;
                    ToolTip tooltip = new ToolTip { Content = shape.ShapeId.ToString() };
                    ToolTipService.SetToolTip(View, tooltip);

                    ShapeHovered?.Invoke(shape.ShapeId);
                    return;
                }
            }

            Mouse.OverrideCursor = null;
            ToolTipService.SetToolTip(View, null);
        }

        private BT.Geometry.Point ConvertMouseToScene(System.Windows.Point mousePos)
        {
            // Reverse the projection used in UpdateProjection()
            var bounds = GetContentBounds();
            float contentWidth = (float)bounds.Width;
            float contentHeight = (float)bounds.Height;
            float controlWidth = (float)View.ActualWidth;
            float controlHeight = (float)View.ActualHeight;

            float scaleX = controlWidth / contentWidth;
            float scaleY = controlHeight / contentHeight;
            float scale = MathF.Min(scaleX, scaleY) * (float)ZoomFactor;

            // Apply pan offset (converted to pixels)
            float panX = _panOffset.X * controlWidth / 2f;
            float panY = _panOffset.Y * controlHeight / 2f;

            float offsetX = (controlWidth - contentWidth * scale) / 2f + panX;
            float offsetY = (controlHeight - contentHeight * scale) / 2f + panY;

            // Map screen to element
            float x = (float)((mousePos.X - offsetX) / scale + bounds.X);
            float y = (float)((mousePos.Y - offsetY) / scale + bounds.Y);

            return new BT.Geometry.Point(x, y);
        }

        #endregion

    }
}
