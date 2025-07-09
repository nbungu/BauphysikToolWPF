using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Line = BT.Geometry.Line;
using Point = BT.Geometry.Point;
using Rectangle = BT.Geometry.Rectangle;

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

        private List<float> _lineVertices = new(); // Interleaved: position + color

        private const double ZoomStep = 0.1;
        private const double MinZoom = 0.5;
        private const double MaxZoom = 5.0;

        #region Public Properties
        
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public double ZoomFactor { get; set; } = 1.0;

        public GLWpfControl View { get; private set; }
        public GLWpfControlSettings ViewSettings { get; private set; }

        #endregion

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
            ErrorCode error = ErrorCode.NoError;

            var bgColor = GetColorFromBrush(BackgroundColor);
            GL.ClearColor(bgColor.X, bgColor.Y, bgColor.Z, bgColor.W);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_prgHandle);

            int projLocation = GL.GetUniformLocation(_prgHandle, "uProjection");
            GL.UniformMatrix4(projLocation, false, ref _projectionMatrix);

            GL.BindVertexArray(_vertexArrayObject);

            // Draw rectangles
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            //int stride = 9 * sizeof(float);

            //// Position
            //GL.EnableVertexAttribArray(positionLocation);
            //GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, stride, 0);

            //// Color
            //GL.EnableVertexAttribArray(colorLocation);
            //GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            //// TexCoord
            //int texCoordLocation = GL.GetAttribLocation(_prgHandle, "vTexCoord");
            //GL.EnableVertexAttribArray(texCoordLocation);
            //GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, stride, 7 * sizeof(float));


            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
            error = GL.GetError();
            if (error != ErrorCode.NoError)
                Debug.WriteLine($"OpenGL Error: {error}");

            // Draw lines
            if (_lineVertexCount > 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVertexBuffer);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                GL.LineWidth(1.0f); // Dynamic thickness
                GL.DrawArrays(PrimitiveType.Lines, 0, _lineVertexCount);
                error = GL.GetError();
                if (error != ErrorCode.NoError)
                    Debug.WriteLine($"OpenGL Error: {error}");
            }
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
            }

            Session.SelectedLayerChanged -= View_OnElementChanged;
            Session.SelectedElementChanged -= View_OnElementChanged;

            if (_vertexBufferObject != 0)
                GL.DeleteBuffer(_vertexBufferObject);
            if (_lineVertexBuffer != 0)
                GL.DeleteBuffer(_lineVertexBuffer);
            if (_vertexArrayObject != 0)
                GL.DeleteVertexArray(_vertexArrayObject);
            if (_prgHandle != 0)
                GL.DeleteProgram(_prgHandle);

            _disposed = true;
        }

        #region private

        private void Initialize()
        {
            _prgHandle = GL.CreateProgram();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

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
            _vertices.Clear();
            _lineVertices.Clear();

            foreach (var geom in _crossSectionBuilder.DrawingGeometries)
            {
                // TODO: HERE?
                geom.ShapeId = new ShapeId(ShapeType.Layer, geom.InternalId);

                // Testing
                AddRectangle(geom.Rectangle, geom.BackgroundColor);

                AddLine(geom.Rectangle.TopLine, Brushes.Black);

            }

            // Push rectangle data
            _vertexCount = _vertices.Count / 7;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.DynamicDraw);

            // Push line data
            _lineVertexCount = _lineVertices.Count / 7;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _lineVertices.Count * sizeof(float), _lineVertices.ToArray(), BufferUsageHint.DynamicDraw);
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

        private void AddRectangle(Rectangle rectangle, Brush backgroundColor)
        {
            Vector4 c = GetColorFromBrush(backgroundColor);

            float x = (float)rectangle.X;
            float y = (float)rectangle.Y;
            float w = (float)rectangle.Width;
            float h = (float)rectangle.Height;
            
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
            _vertices.AddRange(rect);

            // Texture coordinates: (0,0) top-left to (1,1) bottom-right
            //float[] rect =
            //{
            //    // First triangle
            //    x,     y,     0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,
            //    x + w, y,     0f,  c.X, c.Y, c.Z, c.W,  1f, 0f,
            //    x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f, 1f,

            //    // Second triangle
            //    x + w, y,     0f,  c.X, c.Y, c.Z, c.W,  1f, 0f,
            //    x + w, y + h, 0f,  c.X, c.Y, c.Z, c.W,  1f, 1f,
            //    x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f, 1f,
            //};
        }

        private void AddLine(Line line, Brush backgroundColor)
        {
            Vector4 color = GetColorFromBrush(backgroundColor);

            var p1 = line.Start;
            var p2 = line.End;

            _lineVertices.AddRange(new float[]
            {
                (float)p1.X, (float)p1.Y, 0f,  color.X, color.Y, color.Z, color.W,
                (float)p2.X, (float)p2.Y, 0f,  color.X, color.Y, color.Z, color.W
            });
        }

        private void AddCircle(Point center, float radius, Brush brush, int segments = 32)
        {
            Vector4 color = GetColorFromBrush(brush);
            float cx = (float)center.X;
            float cy = (float)center.Y;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * MathF.PI * 2f / segments;
                float angle2 = (i + 1) * MathF.PI * 2f / segments;

                float x1 = cx + radius * MathF.Cos(angle1);
                float y1 = cy + radius * MathF.Sin(angle1);
                float x2 = cx + radius * MathF.Cos(angle2);
                float y2 = cy + radius * MathF.Sin(angle2);

                // Triangle fan with center
                _vertices.AddRange(new float[]
                {
                    cx, cy, 0f, color.X, color.Y, color.Z, color.W,
                    x1, y1, 0f, color.X, color.Y, color.Z, color.W,
                    x2, y2, 0f, color.X, color.Y, color.Z, color.W,
                });
            }
        }

        private void AddCircleLine(Point center, float radius, Brush brush, int segments = 64)
        {
            Vector4 color = GetColorFromBrush(brush);
            float cx = (float)center.X;
            float cy = (float)center.Y;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * MathF.PI * 2f / segments;
                float angle2 = (i + 1) * MathF.PI * 2f / segments;

                float x1 = cx + radius * MathF.Cos(angle1);
                float y1 = cy + radius * MathF.Sin(angle1);
                float x2 = cx + radius * MathF.Cos(angle2);
                float y2 = cy + radius * MathF.Sin(angle2);

                _lineVertices.AddRange(new float[]
                {
                    x1, y1, 0f, color.X, color.Y, color.Z, color.W,
                    x2, y2, 0f, color.X, color.Y, color.Z, color.W,
                });
            }
        }

        #region Event Handlers

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
            if (e.ChangedButton == MouseButton.Middle) _isDragging = false;
            View.ReleaseMouseCapture();
            Mouse.OverrideCursor = null;
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
            //ToolTipService.SetToolTip(View, null);
        }

        #endregion

        #region Helper/Converter

        private Point ConvertMouseToScene(System.Windows.Point mousePos)
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

            return new Point(x, y);
        }

        private Vector4 GetColorFromBrush(Brush brush)
        {
            if (brush is SolidColorBrush solid)
            {
                var c = solid.Color;
                return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            }
            return new Vector4(1f, 1f, 1f, 1f);
        }

        private static BitmapSource RenderBrushToBitmap(DrawingBrush brush, int width = 64, int height = 64)
        {
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            return rtb;
        }

        private static int CreateTextureFromBitmap(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            bmp.CopyPixels(pixels, stride, 0);

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            return texId;
        }
        //var bitmap = RenderBrushToBitmap(myDrawingBrush);
        //int textureId = CreateTextureFromBitmap(bitmap);

        #endregion

        #endregion

    }
}
