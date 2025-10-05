using BT.Geometry;
using BT.Logging;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
using Point = BT.Geometry.Point;
using Size = BT.Geometry.Size;
using Vector = BT.Geometry.Vector;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Central controller for the OpenGL scene: manages view state, connects OpenTK to WPF control,
    /// handles mouse interactions (zoom, pan, hover, click), and delegates rendering to submodules.
    /// </summary>
    public class OglController : IDisposable
    {
        public event Action<ShapeId>? ShapeHovered;
        public event Action<ShapeId>? ShapeClicked;
        public event Action<ShapeId>? ShapeDoubleClicked;
        public event Action<ShapeId>? ShapeRightClicked;
        
        #region private fields

        private readonly OglRenderer _oglRenderer;
        private readonly TextureManager _textureManager;
        private bool _disposed;
        private bool _dragging;
        private float _zoomFactor = 1.0f; // Used to track zoom changes
        //private float _scale = 1.0f; // Used to track scale changes for text rendering
        private Vector _pan = Vector.Empty;
        private Point _lastMousePos = Point.Empty;
        private DateTime _lastLeftClickTime = DateTime.MinValue;
        private const int DoubleClickThresholdMs = 250;

        private SizeChangedEventHandler _sizeChangedHandler;
        private MouseButtonEventHandler _resetViewHandler;

        #endregion

        #region public properties

        public IOglSceneBuilder SceneBuilder { get; private set; }
        public GLWpfControl View { get; private set; }
        public bool ForceFlushTexturesOnRender { get; set; } = false; // Forces a flush of the OpenGL context on each render call
        public bool IsSceneInteractive { get; set; } = true;
        public bool IsViewConnected => View != null;
        public bool IsTextSizeZoomable
        {
            get => SceneBuilder.IsTextSizeZoomable;
            set
            {
                SceneBuilder.IsTextSizeZoomable = value;
                Redraw();
            }
        }
        public bool ShowSceneDecoration
        {
            get => SceneBuilder.ShowSceneDecoration;
            set
            {
                SceneBuilder.ShowSceneDecoration = value;
                Redraw();
            }
        }
        public float ZoomFactor => _zoomFactor;
        public Size CurrentSceneSize => new Size(SceneBuilder.SceneBounds.Width, SceneBuilder.SceneBounds.Height);
        public Size CurrentViewSize => new Size((int)View.ActualWidth, (int)View.ActualHeight);

        /// <summary>
        /// Conversion factor between screen pixels and world units. 
        /// Multiply by a pixel size to get the equivalent size in world units,
        /// or divide a world size by this factor to get its screen size in pixels.
        /// </summary>
        public float WorldUnitsPerPixel => GetWorldUnitsPerPixel();

        #endregion

        public OglController(GLWpfControl view, IOglSceneBuilder sceneBuilder)
        {
            Logger.LogInfo("[OGL] Starting OglController");

            _textureManager = new TextureManager();
            _oglRenderer = new OglRenderer(_textureManager);
            // Order is relevant!
            ConnectToView(view);
            SetNewSceneBuilder(sceneBuilder);

            Logger.LogInfo("[OGL] Successfully constructed OglController");
        }

        public void ConnectToView(GLWpfControl view, GLWpfControlSettings? settings = null)
        {
            View = view ?? throw new InvalidOperationException("No view was passed.");
            settings ??= new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3,
                RenderContinuously = false,
                Samples = 0, // [0, 1] turns MSAA Off. Max = 16
            };

            view.Render += OnRender;
            view.MouseWheel += OnWheel;
            view.MouseDown += OnMouseDown;
            view.MouseUp += OnMouseUp;
            view.MouseMove += OnMouseMove;
            view.MouseLeave += OnMouseLeave;
            view.KeyDown += OnKeyDown;

            _sizeChangedHandler = (_, __) => Redraw();
            _resetViewHandler = (_, __) => ResetView();

            view.SizeChanged += _sizeChangedHandler;
            view.MouseRightButtonUp += _resetViewHandler;
            view.Start(settings);

            Logger.LogInfo("[OGL] Successfully connected to GLWpfControl view");

            _oglRenderer.Initialize();
            //_oglRenderer.SetupFBOs((int)CurrentViewSize.Width, (int)CurrentViewSize.Height, settings.Samples);
        }

        /// <summary>
        /// Set the SceneBuilder after a View has been connected.
        /// </summary>
        /// <param name="sceneBuilder"></param>
        public void SetNewSceneBuilder(IOglSceneBuilder sceneBuilder)
        {
            if (sceneBuilder is null)
                throw new ArgumentNullException(nameof(sceneBuilder), "SceneBuilder cannot be null.");
            if (!IsViewConnected)
                throw new InvalidOperationException("View must be connected before setting a new SceneBuilder.");

            sceneBuilder.TextureManager = _textureManager;
            SceneBuilder = sceneBuilder;
            SceneBuilder.Dpi = 96; // 96 DPI means 1 pt = 1 px at 100% zoom. (150/300 DPI for print quality)

            Logger.LogInfo("[OGL] Successfully set new sceneBuilder");
        }

        public void Invalidate() => View.InvalidateVisual();
        public void ZoomIn() => SetZoom(ZoomFactor + 0.2f);
        public void ZoomOut() => SetZoom(ZoomFactor - 0.2f);

        public void ResetView()
        {
            _pan = Vector.Empty;
            SetZoom(1.0);
        }

        public void SetZoom(double zoom)
        {
            _zoomFactor = (float)Math.Clamp(zoom, 0.5, 3.0);
            if (SceneBuilder.IsTextSizeZoomable) Invalidate();
            else Redraw();
        }

        public void Redraw()
        {
            Logger.LogInfo("[OGL] Redrawing scene");

            if (ForceFlushTexturesOnRender) _textureManager.Dispose();
            SceneBuilder.ZoomFactor = ZoomFactor;
            SceneBuilder.WorldUnitsPerPixel = WorldUnitsPerPixel;
            // TODO: WorldUnitsPerPixel (because of SceneBuilder.SceneBounds) Changes between two consecutive right clicks (ResetView)

            // IOglSceneBuilder
            SceneBuilder.RectVertices.Clear();
            SceneBuilder.LineVertices.Clear();
            SceneBuilder.RectBatches.Clear();
            SceneBuilder.LineBatches.Clear();
            SceneBuilder.SceneShapes.Clear();
            SceneBuilder.BuildScene();

            // Sort the list in place by z-Index for correct hit testing
            SceneBuilder.SceneShapes.Sort((a, b) => b.ZIndex.CompareTo(a.ZIndex));

            Invalidate();
        }

        private void OnRender(TimeSpan _)
        {
            var size = CurrentViewSize;
            var bounds = SceneBuilder.SceneBounds;
            var proj = BuildProjection(size, bounds, ZoomFactor, _pan);

            //_oglRenderer.RenderToScreen(
            //    SceneBuilder.RectVertices.ToArray(),
            //    SceneBuilder.LineVertices.ToArray(),
            //    SceneBuilder.RectBatches,
            //    SceneBuilder.LineBatches,
            //    proj);
            _oglRenderer.Render(
                SceneBuilder.RectVertices.ToArray(),
                SceneBuilder.LineVertices.ToArray(),
                SceneBuilder.RectBatches,
                SceneBuilder.LineBatches,
                proj);
            Console.WriteLine("[OGL] Render called");
        }

        /// <summary>
        /// Conversion factor between screen pixels and world units. 
        /// Multiply by a pixel size to get the equivalent size in world units,
        /// or divide a world size by this factor to get its screen size in pixels.
        /// </summary>
        private float GetWorldUnitsPerPixel()
        {
            if (View.ActualHeight <= 0 || SceneBuilder.SceneBounds.Height <= 0)
                return 1f; // Safe fallback

            // pixel dimensions of the control.
            float ctrlH = (float)View.ActualHeight, ctrlW = (float)View.ActualWidth;
            // scene bounds in whatever coordinate space or unit your drawing shapes use
            float contH = (float)SceneBuilder.SceneBounds.Height, contW = (float)SceneBuilder.SceneBounds.Width;
            float scaleY = ctrlH / contH, scaleX = ctrlW / contW;
            float pixelsPerWorldUnit = MathF.Min(scaleX, scaleY) * ZoomFactor;

            return 1f / pixelsPerWorldUnit;
        }

        public static Matrix4 BuildProjection(Size viewportSize, Rectangle sceneContent, float zoom = 1f, Vector? pan = null)
        {
            Vector panVect = pan ?? Vector.Empty;
            
            // pixel dimensions of the control.
            float ctrlW = (float)viewportSize.Width, ctrlH = (float)viewportSize.Height;
            // scene bounds in whatever coordinate space or unit your drawing shapes use
            float contW = (float)sceneContent.Width, contH = (float)sceneContent.Height;

            float scaleX = ctrlW / contW;
            float scaleY = ctrlH / contH;
            float scale = MathF.Min(scaleX, scaleY) * zoom;

            float baseDivisorWidth = ctrlW / 2f;
            float baseDivisorHeight = ctrlH / 2f;

            // Inital offsets set to the top-left corner of the content rectangle
            float originOffsetX = (float)sceneContent.X / baseDivisorWidth;
            float originOffsetY = (float)sceneContent.Y / baseDivisorHeight;

            var dx = 0f;
            var dy = 0f;
            if (contW * scale < ctrlW) dx = (ctrlW - contW * scale) / 2f / baseDivisorWidth;
            if (contH * scale < ctrlH) dy = (ctrlH - contH * scale) / 2f / baseDivisorHeight;

            var zIndexStart = -100f;
            var zIndexEnd = 100f;

            Matrix4 view = Matrix4.CreateTranslation(-originOffsetX, originOffsetY, 0) *
                           Matrix4.CreateTranslation(1f, -1f, 0) *
                           Matrix4.CreateScale(scale) *
                           Matrix4.CreateTranslation(-1f + dx + (float)panVect.X, 1f - dy + (float)panVect.Y, 0);

            return Matrix4.CreateOrthographicOffCenter(0, ctrlW, ctrlH, 0, zIndexStart, zIndexEnd) * view;
        }

        public void Dispose()
        {
            if (_disposed) return;

            Logger.LogInfo("[OGL] Disposing controller");

            _oglRenderer.Dispose();
            _textureManager.Dispose();

            View.Render -= OnRender;
            View.MouseWheel -= OnWheel;
            View.MouseDown -= OnMouseDown;
            View.MouseUp -= OnMouseUp;
            View.MouseMove -= OnMouseMove;
            View.MouseLeave -= OnMouseLeave;
            View.KeyDown -= OnKeyDown;

            View.SizeChanged -= _sizeChangedHandler;
            View.MouseRightButtonUp -= _resetViewHandler;

            _disposed = true;
        }

        #region Scene Image Capture

        /// <summary>
        /// Captures an image of the current OpenGL scene at the given resolution and zoom factor.
        /// </summary>
        public BitmapSource GetSceneImage(int width, int height, double zoom = 1.0)
        {
            if (!SceneBuilder.IsValid) return new BitmapImage(); // Return empty image if scene is invalid
            
            var originalZoom = ZoomFactor;
            SetZoom(zoom); // Temporarily apply the zoom
            try
            {
                var projection = BuildProjection(new Size(width, height), SceneBuilder.SceneBounds, ZoomFactor, _pan);
                return _oglRenderer.CaptureRendering(
                    SceneBuilder.RectVertices.ToArray(),
                    SceneBuilder.LineVertices.ToArray(),
                    SceneBuilder.RectBatches,
                    SceneBuilder.LineBatches,
                    projection,
                    width,
                    height,
                    SceneBuilder.Dpi);
            }
            finally
            {
                SetZoom(originalZoom); // Restore previous zoom
            }
        }
        /// <summary>
        /// Captures an image of the current OpenGL scene at the given resolution and zoom factor.
        /// </summary>
        public BitmapSource GetSceneImage(double zoom = 1.0)
        {
            if (!SceneBuilder.IsValid) return new BitmapImage(); // Return empty image if scene is invalid

            var size = CurrentSceneSize;
            var originalZoom = ZoomFactor;
            SetZoom(zoom); // Temporarily apply the zoom
            try
            {
                var projection = BuildProjection(size, SceneBuilder.SceneBounds, ZoomFactor, _pan);
                return _oglRenderer.CaptureRendering(
                    SceneBuilder.RectVertices.ToArray(),
                    SceneBuilder.LineVertices.ToArray(),
                    SceneBuilder.RectBatches,
                    SceneBuilder.LineBatches,
                    projection,
                    (int)size.Width,
                    (int)size.Height,
                    SceneBuilder.Dpi);
            }
            finally
            {
                SetZoom(originalZoom); // Restore previous zoom
            }
        }

        public byte[] GetSceneImageAsBytes(int width, int height, double zoom = 1.0)
        {
            var bmp = GetSceneImage(width, height, zoom);
            return bmp.ToByteArray();
        }
        public byte[] GetSceneImageAsBytes(double zoom = 1.0)
        {
            var bmp = GetSceneImage(zoom);
            return bmp.ToByteArray();
        }

        #endregion

        #region Mouse Events

        /// <summary>
        /// Converts a mouse position in screen (pixel) coordinates into scene (world) coordinates, 
        /// taking into account the current projection, zoom factor, and pan offset.
        /// 
        /// Steps:
        /// 1. Builds the current projection matrix with zoom and pan.
        /// 2. Inverts the projection to map from Normalized Device Coordinates (NDC) back to world space.
        /// 3. Converts the mouse position from pixel space into NDC [-1, 1].
        /// 4. Transforms the NDC position with the inverted projection to obtain the world coordinate.
        /// 
        /// This ensures hit-testing and interactions remain correct even when the scene 
        /// is zoomed or panned.
        /// </summary>
        private Point ConvertMouseToScene(Point mouse)
        {
            if (SceneBuilder.SceneBounds.Area < 1E-06)
                return Point.Empty;

            var bounds = SceneBuilder.SceneBounds;
            var viewport = CurrentViewSize;

            var proj = BuildProjection(viewport, bounds, _zoomFactor, _pan);

            Matrix4.Invert(proj, out var invProj);

            float ndcX = (float)(2.0 * mouse.X / viewport.Width - 1.0);
            float ndcY = (float)(1.0 - 2.0 * mouse.Y / viewport.Height);

            var vec = new Vector4(ndcX, ndcY, 0, 1); // OpenTK.Mathematics.Vector4
            var world = vec * invProj;

            return new Point(world.X, world.Y);
        }

        private void OnWheel(object s, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ZoomIn();
            else ZoomOut();
        }

        //private void OnWheel(object s, MouseWheelEventArgs e)
        //{
        //    var cursorPos = e.GetPosition(View);
        //    var pt = new Point(cursorPos.X, cursorPos.Y);

        //    if (e.Delta > 0)
        //        SetZoom(ZoomFactor * 1.2, pt);
        //    else
        //        SetZoom(ZoomFactor / 1.2, pt);
        //}

        private void OnMouseDown(object s, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            var cur = e.GetPosition(View).ToPoint();

            if (e.ChangedButton == MouseButton.Middle)
            {
                _dragging = true;
                _lastMousePos = cur;
                View.CaptureMouse();
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }

            if (!IsSceneInteractive) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                var pt = ConvertMouseToScene(cur);
                // Double click
                if ((now - _lastLeftClickTime).TotalMilliseconds <= DoubleClickThresholdMs)
                {
                    foreach (var shp in SceneBuilder.SceneShapes)
                    {
                        if (shp.Rectangle.Contains(pt))
                        {
                            ShapeDoubleClicked?.Invoke(shp.ShapeId); // <-- Your new event
                            break;
                        }
                    }
                }
                // Single Click
                else
                {
                    foreach (var shp in SceneBuilder.SceneShapes)
                    {
                        if (shp.Rectangle.Contains(pt))
                        {
                            ShapeClicked?.Invoke(shp.ShapeId);
                            break;
                        }
                    }
                }

                _lastLeftClickTime = now;
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                var pt = ConvertMouseToScene(cur);
                foreach (var shp in SceneBuilder.SceneShapes)
                {
                    if (shp.Rectangle.Contains(pt))
                    {
                        ShapeRightClicked?.Invoke(shp.ShapeId);
                        break;
                    }
                }
            }
        }

        private void OnMouseUp(object s, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _dragging = false;
                View.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }

        private void OnMouseMove(object s, MouseEventArgs e)
        {
            var cur = e.GetPosition(View).ToPoint();
            if (_dragging)
            {
                Vector delta = cur - _lastMousePos;
                _lastMousePos = cur;

                float dx = (float)(2.0 * delta.X / View.ActualWidth);
                float dy = (float)(-2.0 * delta.Y / View.ActualHeight);

                _pan += new Vector(dx, dy);
                Invalidate();
            }
            else
            {
                if (!IsSceneInteractive) return;

                var pt = ConvertMouseToScene(cur);
                foreach (var shape in SceneBuilder.SceneShapes)
                {
                    if (shape.Rectangle.Contains(pt))
                    {
                        ShapeHovered?.Invoke(shape.ShapeId);
                        ToolTipService.SetToolTip(View, shape.ShapeId.ToString());
                        Mouse.OverrideCursor = Cursors.Hand;
                        return;
                    }
                }

                ToolTipService.SetToolTip(View, null);
                Mouse.OverrideCursor = null;
            }
        }

        private void OnMouseLeave(object s, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
            ToolTipService.SetToolTip(View, null);
        }

        #endregion

        #region Key Events

        private void OnKeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                ZoomIn();
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                ZoomOut();
                e.Handled = true;
            }
        }

        #endregion
    }
}
