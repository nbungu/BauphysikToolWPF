using BauphysikToolWPF.Services.Application;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
using Point = BT.Geometry.Point;
using Rectangle = BT.Geometry.Rectangle;
using Size = System.Windows.Size;
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

        #region public properties

        public IOglSceneBuilder SceneBuilder { get; private set; }
        public GLWpfControl View { get; private set; }
        public bool IsSceneInteractive { get; set; } = true;
        public bool IsViewConnected => View != null;

        public bool IsTextSizeZoomable
        {
            get => SceneBuilder.IsTextSizeZoomable;
            set => SceneBuilder.IsTextSizeZoomable = value;
        }

        public float ZoomFactor => _zoomFactor;

        #endregion

        #region private fields

        private readonly OglRenderer _oglRenderer;
        private readonly TextureManager _textureManager;
        private bool _disposed;
        private bool _dragging;
        private float _zoomFactor = 1.0f; // Used to track zoom changes
        private Vector _pan = Vector.Empty;
        private Point _lastMousePos = Point.Empty;
        private DateTime _lastLeftClickTime = DateTime.MinValue;
        private const int DoubleClickThresholdMs = 300;

        #endregion

        public OglController(GLWpfControl view, IOglSceneBuilder sceneBuilder)
        {
            _textureManager = new TextureManager();
            _oglRenderer = new OglRenderer(_textureManager);
            // Order is relevant!
            ConnectToView(view);
            SetNewSceneBuilder(sceneBuilder);
        }

        public void ConnectToView(GLWpfControl view, GLWpfControlSettings? settings = null)
        {
            View = view ?? throw new InvalidOperationException("View not connected.");
            settings ??= new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3,
                Profile = ContextProfile.Core,
                ContextFlags = ContextFlags.Default,
                RenderContinuously = false,
                //Samples = 8,
            };

            view.Render += OnRender;
            view.MouseWheel += OnWheel;
            view.MouseDown += OnMouseDown;
            view.MouseUp += OnMouseUp;
            view.MouseMove += OnMouseMove;
            view.MouseLeave += OnMouseLeave;
            view.KeyDown += OnKeyDown;
            view.MouseRightButtonUp += (_, __) => ResetView();

            view.Start(settings);

            // TODO: outsourcen
            Session.SelectedElementChanged += Redraw;
            Session.SelectedLayerChanged += Redraw;
            Session.SelectedLayerIndexChanged += Redraw;

            _oglRenderer.Initialize();
            Console.WriteLine("[OGL] Renderer initialized");
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
        }

        public void Invalidate() => View.InvalidateVisual();
        public void ZoomIn() => SetZoom(ZoomFactor + 0.2f);
        public void ZoomOut() => SetZoom(ZoomFactor - 0.2f);

        public void ResetView()
        {
            SetZoom(1.0);
            _pan = Vector.Empty;
            Invalidate();
        }

        public void SetZoom(double zoom)
        {
            _zoomFactor = (float)Math.Clamp(zoom, 0.4, 6.0);
            if (SceneBuilder.IsTextSizeZoomable) Redraw();
            else Invalidate();
        }

        public void Redraw()
        {
            _textureManager.Dispose();
            SceneBuilder.ZoomFactor = ZoomFactor;

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
            var size = new Size((int)View.ActualWidth, (int)View.ActualHeight);
            var bounds = SceneBuilder.SceneBounds;
            var proj = BuildProjection(size, bounds);

            _oglRenderer.Render(
                SceneBuilder.RectVertices.ToArray(),
                SceneBuilder.LineVertices.ToArray(),
                SceneBuilder.RectBatches,
                SceneBuilder.LineBatches,
                proj);
            Console.WriteLine("[OGL] Render called");
        }

        private Matrix4 BuildProjection(Size control, Rectangle content)
        {
            var rectF = content.ToRectangleF();

            float ctrlW = (float)control.Width, ctrlH = (float)control.Height;
            float contW = rectF.Width, contH = rectF.Height;

            float scaleX = ctrlW / contW;
            float scaleY = ctrlH / contH;
            float scale = MathF.Min(scaleX, scaleY) * ZoomFactor;

            float baseDivisorWidth = ctrlW / 2f;
            float baseDivisorHeight = ctrlH / 2f;

            // Inital offsets set to the top-left corner of the content rectangle
            float originOffsetX = rectF.X / baseDivisorWidth;
            float originOffsetY = rectF.Y / baseDivisorHeight;

            var dx = 0f;
            var dy = 0f;
            if (contW * scale < ctrlW) dx = (ctrlW - contW * scale) / 2f / baseDivisorWidth;
            if (contH * scale < ctrlH) dy = (ctrlH - contH * scale) / 2f / baseDivisorHeight;

            var zIndexStart = -100f;
            var zIndexEnd = 100f;

            Matrix4 view = Matrix4.CreateTranslation(-originOffsetX, originOffsetY, 0) *
                           Matrix4.CreateTranslation(1f, -1f, 0) *
                           Matrix4.CreateScale(scale) *
                           Matrix4.CreateTranslation(-1f + dx + (float)_pan.X, 1f - dy + (float)_pan.Y, 0);


            return Matrix4.CreateOrthographicOffCenter(0, ctrlW, ctrlH, 0, zIndexStart, zIndexEnd) * view;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _oglRenderer.Dispose();
            _textureManager.Dispose();

            View.Render -= OnRender;
            View.MouseWheel -= OnWheel;
            View.MouseDown -= OnMouseDown;
            View.MouseUp -= OnMouseUp;
            View.MouseMove -= OnMouseMove;
            View.MouseLeave -= OnMouseLeave;
            View.KeyDown -= OnKeyDown;

            Session.SelectedElementChanged -= Redraw;
            Session.SelectedLayerChanged -= Redraw;
            Session.SelectedLayerIndexChanged -= Redraw;

            _disposed = true;
            Console.WriteLine("[OGL] Renderer disposed");
        }

        #region Mouse Events

        private Point ConvertMouseToScene(Point mouse)
        {
            if (SceneBuilder.SceneBounds.Area < 1E-06) return Point.Empty; // No valid scene bounds, return empty point

            var bounds = SceneBuilder.SceneBounds;
            float cw = (float)View.ActualWidth, ch = (float)View.ActualHeight;
            float ew = (float)bounds.Width, eh = (float)bounds.Height;

            float scale = MathF.Min(cw / ew, ch / eh) * ZoomFactor;

            float panPxX = (float)_pan.X * cw / 2f;
            float panPxY = (float)_pan.Y * ch / 2f;

            float offsetX = (cw - ew * scale) / 2f + panPxX;
            float offsetY = (ch - eh * scale) / 2f + panPxY;

            float x = (float)((mouse.X - offsetX) / scale + bounds.X);
            float y = (float)((mouse.Y - offsetY) / scale + bounds.Y);
            return new Point(x, y);
        }

        private void OnWheel(object s, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ZoomIn();
            else ZoomOut();
        }

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
    }
}
