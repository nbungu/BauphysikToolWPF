using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public enum HatchFitMode
        {
            OriginalPixelSize, // Use original size of the hatch pattern
            FitToWidth,
            FitToHeight,
            StretchToFill
        }

        public event Action<ShapeId>? ShapeHovered;
        public event Action<ShapeId>? ShapeClicked;

        private Rectangle? _hoveredRectangle;
        private string? _hoveredTooltip;
        private readonly Dictionary<Brush, int> _hatchTextureCache = new();
        private readonly Dictionary<int, Size> _hatchTextureSizes = new();

        private readonly CrossSectionBuilder _crossSectionBuilder = new()
        {
            CanvasSize = new Rectangle(new Point(0, 0), 880, 400), // Default size, will be updated later
        };
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

        private struct RenderRect
        {
            public Rectangle Rect;
            public Vector4 Color;
            public int? TextureId;
        }
        private List<RenderRect> _renderRects = new();

        #region Public Properties

        public Rectangle ElementBounds => GetContentBounds();

        private Brush _bgColor = Brushes.Transparent;
        public Brush BackgroundColor
        {
            get => _bgColor;
            set
            {
                _bgColor = value;
                RefreshView();
            }
        }

        public double ZoomFactor { get; private set; } = 1.0;
        public GLWpfControl OglView { get; private set; }
        public GLWpfControlSettings OglViewSettings { get; private set; }
        public Size OglViewCurrentSize => new Size(OglView.ActualWidth, OglView.ActualHeight);

        #endregion

        public void ConnectToView(GLWpfControl view, GLWpfControlSettings? settings = null)
        {
            Session.SelectedLayerChanged += View_OnElementChanged;
            Session.SelectedElementChanged += View_OnElementChanged;

            OglView = view;
            OglView.Render += View_OnRender;
            OglView.MouseWheel += View_OnMouseWheel;
            OglView.MouseRightButtonUp += View_OnMouseRightClick;
            OglView.MouseDown += View_OnMouseDown;
            OglView.MouseUp += View_OnMouseUp;
            OglView.MouseMove += View_OnMouseMove;
            OglView.MouseLeave += View_OnMouseLeave;

            OglViewSettings = settings ?? new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3,
                RenderContinuously = false,
            };
            OglView.Start(OglViewSettings);
            
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
                Debug.WriteLine("Rebuilding scene...");
                _crossSectionBuilder.RebuildCrossSection();
                RebuildOglGeometry();
                _needsRebuild = false;
            }

            ErrorCode error;

            var bgColor = GetColorFromBrush(BackgroundColor);
            GL.ClearColor(bgColor.X, bgColor.Y, bgColor.Z, bgColor.W);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_prgHandle);
            error = GL.GetError();
            Debug.WriteLine($"UseProgram -> {error}");

            int projLocation = GL.GetUniformLocation(_prgHandle, "uProjection");
            GL.UniformMatrix4(projLocation, false, ref _projectionMatrix);
            error = GL.GetError();
            Debug.WriteLine($"Set uProjection -> {error}, location: {projLocation}");

            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            error = GL.GetError();
            Debug.WriteLine($"Bind VAO/VBO -> {error}");

            int stride = 9 * sizeof(float); // 3 for position, 4 for color, 2 for texture coords

            int positionLocation = 0;
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, stride, 0);

            int colorLocation = 1;
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            int texCoordLocation = 2;
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, stride, 7 * sizeof(float));

            error = GL.GetError();
            Debug.WriteLine($"Set VertexAttribPointers -> {error}");

            int vertexStart = 0;
            Debug.WriteLine($"_texturedRects.Count = {_renderRects.Count}");
            foreach (var rect in _renderRects)
            {
                if (rect.TextureId.HasValue)
                {
                    int texId = rect.TextureId ?? -1;
                    Debug.WriteLine($"Drawing rect with TextureId = {texId}");

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, rect.TextureId.Value);

                    //GL.Uniform1(GL.GetUniformLocation(_prgHandle, "texture0"), 0);
                    GL.Uniform1(GL.GetUniformLocation(_prgHandle, "useHatchPattern"), 1); // Sets useHatchPattern to true
                    GL.Uniform1(GL.GetUniformLocation(_prgHandle, "hatchScale"), 1.0f); // Set hatch scale to 1.0
                    
                    error = GL.GetError();
                    Debug.WriteLine($"BindTexture + Uniforms -> {error}");
                }
                else
                {
                    GL.Uniform1(GL.GetUniformLocation(_prgHandle, "useHatchPattern"), 0);
                }

                GL.DrawArrays(PrimitiveType.Triangles, vertexStart, 6);
                error = GL.GetError();
                Debug.WriteLine($"Draw rectangle from vertex {vertexStart} -> {error}");

                vertexStart += 6;
            }

            if (_lineVertexCount > 0)
            {
                Debug.WriteLine($"Drawing {_lineVertexCount} line vertices...");
                GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVertexBuffer);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                GL.LineWidth(1.0f);
                GL.DrawArrays(PrimitiveType.Lines, 0, _lineVertexCount);
                error = GL.GetError();
                Debug.WriteLine($"Draw lines -> {error}");
            }
        }

        public void ZoomIn()
        {
            ZoomFactor = Math.Min(ZoomFactor + ZoomStep, MaxZoom);
            UpdateProjection(OglViewCurrentSize);
            RefreshView();
        }

        public void SetZoomFactor(double fac)
        {
            if (fac < MinZoom) fac = MinZoom; // Prevent zooming out too far
            if (fac > MaxZoom) fac = MaxZoom; // Prevent zooming in too far
            ZoomFactor = fac;
            UpdateProjection(OglViewCurrentSize);
            RefreshView();
        }

        public void ZoomOut()
        {
            ZoomFactor = Math.Max(ZoomFactor - ZoomStep, MinZoom);
            UpdateProjection(OglViewCurrentSize);
            RefreshView();
        }

        public void ResetZoom()
        {
            ZoomFactor = 1.0; // Reset Zoom
            _panOffset = Vector2.Zero; // Reset pan offset
            UpdateProjection(OglViewCurrentSize);
            RefreshView();
        }

        public void RefreshView()
        {
            OglView.InvalidateVisual(); // Force re-render of OpenGL Control programatically
        }

        /// <summary>
        /// Disposes resources used by the scene and its renderer.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (OglView != null)
            {
                OglView.Render -= View_OnRender;
                OglView.MouseWheel -= View_OnMouseWheel;
                OglView.MouseRightButtonUp -= View_OnMouseRightClick;
                OglView.MouseDown -= View_OnMouseDown;
                OglView.MouseUp -= View_OnMouseUp;
                OglView.MouseMove -= View_OnMouseMove;
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
        }

        /// <summary>
        /// Updates the projection matrix used to transform screen-space layer positions
        /// into Normalized Device Coordinates (NDC) for rendering. This ensures that
        /// layer rectangles (defined in pixel units) appear at the correct positions
        /// when rendered by the GPU.
        /// </summary>
        private void UpdateProjection(Size controlSize)
        {
            float contentWidth = (float)ElementBounds.Width;
            float contentHeight = (float)ElementBounds.Height;
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
            Matrix4 view = Matrix4.CreateTranslation(1.0f, -1.0f, 0) *
                           Matrix4.CreateScale(scale, scale, 1) *
                           Matrix4.CreateTranslation(-1.0f, 1.0f, 0);

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

        private void RebuildOglGeometry()
        {
            _vertices.Clear();
            _lineVertices.Clear();

            foreach (var geom in _crossSectionBuilder.DrawingGeometries)
            {
                geom.ShapeId = new ShapeId(ShapeType.Layer, geom.InternalId);

                // Add rectangle with hatch texture
                if (geom.DrawingBrush is DrawingBrush hatch)
                    //AddRectangle(geom.Rectangle, geom.BackgroundColor);
                    AddTexturedRectangle(geom.Rectangle, geom.BackgroundColor, hatch, geom.HatchFitMode);
                else
                    AddRectangle(geom.Rectangle, geom.BackgroundColor);

                ////AddLine(geom.Rectangle.TopLine, Brushes.Black);
                //var pen = BrushesRepo.CreateDottedPen(Brushes.Blue, 1.0);
                //AddLine(geom.Rectangle.TopLine, pen);
                //AddLine(geom.Rectangle.TopLine, Brushes.Black);
            }

            // Push rectangle data (now 9 floats per vertex)
            _vertexCount = _vertices.Count / 9;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.DynamicDraw);

            // Push line data
            _lineVertexCount = _lineVertices.Count / 7;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _lineVertices.Count * sizeof(float), _lineVertices.ToArray(), BufferUsageHint.DynamicDraw);
        }

        private Rectangle GetContentBounds()
        {
            if (_crossSectionBuilder.DrawingGeometries.Count == 0)
                return new Rectangle(0, 0, 1, 1); // Prevent divide by zero

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

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private void AddTexturedRectangle(Rectangle rectangle, Brush backgroundColor, DrawingBrush hatchBrush, HatchFitMode hatchFitMode = HatchFitMode.OriginalPixelSize, double texScale = 1.0)
        {
            Vector4 c = GetColorFromBrush(backgroundColor);

            float x = (float)rectangle.X;
            float y = (float)rectangle.Y;
            float w = (float)rectangle.Width;
            float h = (float)rectangle.Height;

            // Generate or retrieve texture
            int? textureId = null;

            if (!_hatchTextureCache.TryGetValue(hatchBrush, out int texId))
            {
                var bitmap = RenderBrushToBitmap(hatchBrush);

                // B: Load from resources
                //var image = new BitmapImage(new Uri("pack://application:,,,/Resources/test-hatch.png")); // pack is a WPF URI for XAML resource loading,
                //var bmp = new FormatConvertedBitmap(image, PixelFormats.Pbgra32, null, 0);
                //var bitmap = bmp;

                // C: Load from disk
                //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "test-hatch.png");
                //var image = new BitmapImage(new Uri(path, UriKind.Absolute));
                //var bmp = new FormatConvertedBitmap(image, PixelFormats.Pbgra32, null, 0);
                //var bitmap = bmp;

                texId = CreateTextureFromBitmap(bitmap);
                if (texId == 0)
                    Debug.WriteLine("Failed to create texture!");
                else
                    Debug.WriteLine($"Created texture with ID: {texId}");
                _hatchTextureCache[hatchBrush] = texId;
            }
            textureId = texId;
            
            // Store textured rectangle info for drawing
            _renderRects.Add(new RenderRect
            {
                Rect = rectangle,
                Color = c,
                TextureId = textureId
            });

            // Texture coordinates scaling based on texture size
            float texRepeatX = 1f, texRepeatY = 1f;

            if (textureId.HasValue && _hatchTextureSizes.TryGetValue(textureId.Value, out var texSize))
            {
                double texWidth = texSize.Width * texScale;
                double texHeight = texSize.Height * texScale;

                if (hatchBrush is DrawingBrush db && db.Transform is TransformGroup group &&
                    group.Children.OfType<ScaleTransform>().FirstOrDefault() is ScaleTransform scale)
                {
                    // Optional: honor WPF transform scale if used
                    texWidth /= scale.ScaleX;
                    texHeight /= scale.ScaleY;
                }

                var aspect = texWidth / texHeight;

                switch (hatchFitMode)
                {
                    case HatchFitMode.FitToWidth:
                        texRepeatX = 1.0f;
                        texRepeatY = (float)(h / (w / aspect));
                        break;
                    case HatchFitMode.FitToHeight:
                        texRepeatY = 1.0f;
                        texRepeatX = (float)(w / (h * aspect));
                        break;
                    case HatchFitMode.StretchToFill:
                        texRepeatY = 1.0f;
                        texRepeatX = 1.0f;
                        break;
                    default:
                        texRepeatX = (float)(w / texWidth); // raw tiling based on physical texture size
                        texRepeatY = (float)(h / texHeight);
                        break;
                }
            }

            float[] rect =
            {
                // First triangle
                x,     y,     0f,  c.X, c.Y, c.Z, c.W,  0f,         0f,
                x + w, y,     0f,  c.X, c.Y, c.Z, c.W,  texRepeatX, 0f,
                x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f,         texRepeatY,

                // Second triangle
                x + w, y,     0f,  c.X, c.Y, c.Z, c.W,  texRepeatX, 0f,
                x + w, y + h, 0f,  c.X, c.Y, c.Z, c.W,  texRepeatX, texRepeatY,
                x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f,         texRepeatY,
            };
            _vertices.AddRange(rect);
        }

        private void AddRectangle(Rectangle rectangle, Brush backgroundColor)
        {
            Vector4 c = GetColorFromBrush(backgroundColor);

            float x = (float)rectangle.X;
            float y = (float)rectangle.Y;
            float w = (float)rectangle.Width;
            float h = (float)rectangle.Height;

            // Generate or retrieve texture
            int? textureId = null;
            
            // Store textured rectangle info for drawing
            _renderRects.Add(new RenderRect
            {
                Rect = rectangle,
                Color = c,
                TextureId = textureId
            });

            float[] rect =
            {
                // First triangle
                x,     y,     0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,
                x + w, y,     0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,
                x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,

                // Second triangle
                x + w, y,     0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,
                x + w, y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,
                x,     y + h, 0f,  c.X, c.Y, c.Z, c.W,  0f, 0f,
            };
            _vertices.AddRange(rect);
        }

        private void AddLine(Line line, Brush lineColor)
        {
            Vector4 color = GetColorFromBrush(lineColor);

            var p1 = line.Start;
            var p2 = line.End;

            _lineVertices.AddRange(new float[]
            {
                (float)p1.X, (float)p1.Y, 0f,  color.X, color.Y, color.Z, color.W,
                (float)p2.X, (float)p2.Y, 0f,  color.X, color.Y, color.Z, color.W
            });
        }

        private void AddLine(Line line, Pen pen)
        {
            Vector4 color = GetColorFromBrush(pen.Brush);
            var dashStyle = pen.DashStyle;
            double thickness = pen.Thickness;

            var p1 = line.Start;
            var p2 = line.End;

            if (dashStyle == null || dashStyle.Dashes.Count == 0)
            {
                // Solid line
                _lineVertices.AddRange(new float[]
                {
                    (float)p1.X, (float)p1.Y, 0f,  color.X, color.Y, color.Z, color.W,
                    (float)p2.X, (float)p2.Y, 0f,  color.X, color.Y, color.Z, color.W
                });
                return;
            }

            // Vector direction
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0) return;

            double ux = dx / length;
            double uy = dy / length;

            double pos = 0;
            int dashIndex = 0;
            bool draw = true;

            while (pos < length)
            {
                double dashLength = dashStyle.Dashes[dashIndex % dashStyle.Dashes.Count];
                double segmentLength = Math.Min(dashLength, length - pos);

                if (draw)
                {
                    var sx = p1.X + ux * pos;
                    var sy = p1.Y + uy * pos;
                    var ex = p1.X + ux * (pos + segmentLength);
                    var ey = p1.Y + uy * (pos + segmentLength);

                    _lineVertices.AddRange(new float[]
                    {
                        (float)sx, (float)sy, 0f, color.X, color.Y, color.Z, color.W,
                        (float)ex, (float)ey, 0f, color.X, color.Y, color.Z, color.W
                    });
                }

                pos += segmentLength;
                dashIndex++;
                draw = !draw;
            }
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
            this.UpdateProjection(OglViewCurrentSize); 
            RefreshView(); // Force re-render of OpenGL Control programatically
        }

        private void View_OnRender(TimeSpan delta)
        {
            this.UpdateProjection(OglViewCurrentSize);
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
                _lastMousePosition = e.GetPosition(OglView);
                OglView.CaptureMouse();
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }

            if (e.ChangedButton == MouseButton.Left && !_isDragging)
            {
                var scenePoint = ConvertMouseToScene(e.GetPosition(OglView));

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
                var scenePoint = ConvertMouseToScene(e.GetPosition(OglView));

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
            OglView.ReleaseMouseCapture();
        }

        private void View_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                System.Windows.Point currentPos = e.GetPosition(OglView);
                Vector delta = currentPos - _lastMousePosition;
                _lastMousePosition = currentPos;

                // Convert delta from screen pixels to OpenGL NDC units:
                float ndcX = (float)(2.0 * delta.X / OglView.ActualWidth);
                float ndcY = (float)(-2.0 * delta.Y / OglView.ActualHeight); // Y is inverted in OpenGL

                _panOffset += new Vector2(ndcX, ndcY);

                UpdateProjection(OglViewCurrentSize);
                RefreshView();
            }
            else
            {
                // Hit testing for Interactive View

                var scenePoint = ConvertMouseToScene(e.GetPosition(OglView));

                foreach (var shape in _crossSectionBuilder.DrawingGeometries)
                {
                    if (shape.Rectangle.Contains(scenePoint))
                    {
                        _hoveredRectangle = shape.Rectangle;
                        _hoveredTooltip = shape.ShapeId.ToString();

                        Mouse.OverrideCursor = Cursors.Hand;
                        ToolTip tooltip = new ToolTip { Content = shape.ShapeId.ToString() };
                        ToolTipService.SetToolTip(OglView, tooltip);

                        ShapeHovered?.Invoke(shape.ShapeId);
                        return;
                    }
                }
                Mouse.OverrideCursor = null;
                ToolTipService.SetToolTip(OglView, null);
            }
        }

        private void View_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
            ToolTipService.SetToolTip(OglView, null);
            _hoveredRectangle = null;
            _hoveredTooltip = null;
        }

        #endregion

        #region Helper/Converter

        private Point ConvertMouseToScene(System.Windows.Point mousePos)
        {
            // Reverse the projection used in UpdateProjection()
            float contentWidth = (float)ElementBounds.Width;
            float contentHeight = (float)ElementBounds.Height;
            float controlWidth = (float)OglView.ActualWidth;
            float controlHeight = (float)OglView.ActualHeight;

            float scaleX = controlWidth / contentWidth;
            float scaleY = controlHeight / contentHeight;
            float scale = MathF.Min(scaleX, scaleY) * (float)ZoomFactor;

            // Apply pan offset (converted to pixels)
            float panX = _panOffset.X * controlWidth / 2f;
            float panY = _panOffset.Y * controlHeight / 2f;

            float offsetX = (controlWidth - contentWidth * scale) / 2f + panX;
            float offsetY = (controlHeight - contentHeight * scale) / 2f + panY;

            // Map screen to element
            float x = (float)((mousePos.X - offsetX) / scale + ElementBounds.X);
            float y = (float)((mousePos.Y - offsetY) / scale + ElementBounds.Y);

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

        private static BitmapSource RenderBrushToBitmap(DrawingBrush brush)
        {
            int width = (int)brush.Viewbox.Width; // 64
            int height = (int)brush.Viewbox.Height; // 64
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            return rtb;
        }

        private int CreateTextureFromBitmap(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            bmp.CopyPixels(pixels, stride, 0);

            int texIdHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texIdHandle);

            // Upload texture data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            ApplyTextureParameters();

            // Save texture size
            _hatchTextureSizes[texIdHandle] = new Size(width, height);

            // Debug output
            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"OpenGL Error in CreateTextureFromBitmap: {err}");
                return 0;
            }

            Debug.WriteLine($"Texture created successfully: ID = {texIdHandle}, Size = {width}x{height}");
            return texIdHandle;
        }
        /// <summary>
        /// Converts a pixels (e.g. 64 px texture) to normalized screen size,
        /// where 1.0 = full screen width or height, and 2.0 = half, etc.
        /// </summary>
        /// <returns>Normalized size in OpenGL screen units</returns>
        private static float ConvertPixelsToNormalized(float pixel, float containerPixel, double zoomFactor)
        {
            // Adjust for zoom
            float scaledPixels = pixel * (float)zoomFactor;
            float baseDivisor = scaledPixels == 0 ? containerPixel : scaledPixels;
            return containerPixel / baseDivisor;
        }

        /// <summary>
        /// Applies consistent OpenGL parameters to a bound texture (e.g. filtering and wrap modes).
        /// Call this after GL.BindTexture().
        /// </summary>
        private static void ApplyTextureParameters()
        {
            // Filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Don't repeat
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // Repeat pattern tiling
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Optional: Enable this if you generate mipmaps
            // GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        #endregion

        #endregion

    }
}
