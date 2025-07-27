using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenTK.Windowing.Common;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
using BauphysikToolWPF.Models.UI;
using System.Collections.Generic;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Central controller for the OpenGL scene: manages view state, connects OpenTK to WPF control,
    /// handles mouse interactions (zoom, pan, hover, click), and delegates rendering to submodules.
    /// </summary>
    public class ElementSceneController : IDisposable
    {
        public readonly ElementRenderer Renderer;
        public readonly TextureManager TextureManager;
        public readonly SceneBuilder SceneBuilder;
        public readonly CrossSectionBuilder CrossSectionBuilder;

        private GLWpfControl _view;
        private bool _disposed;

        private float _zoom = 1f;
        private Vector2 _pan = Vector2.Zero;
        private Point _lastMousePos;
        private bool _dragging;

        public event Action<ShapeId>? ShapeHovered;
        public event Action<ShapeId>? ShapeClicked;

        public ElementSceneController()
        {
            Renderer = new ElementRenderer(this);
            TextureManager = new TextureManager(this);
            SceneBuilder = new SceneBuilder(this);

            CrossSectionBuilder = new CrossSectionBuilder();
        }


        public void ConnectToView(GLWpfControl view, GLWpfControlSettings? settings = null)
        {
            _view = view;
            settings ??= new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3,
                Profile = ContextProfile.Core,
                ContextFlags = ContextFlags.Default,
                RenderContinuously = false,
                //Samples = 8,
                //TransparentBackground = true,
            };

            view.Render += OnRender;
            view.MouseWheel += OnWheel;
            view.MouseDown += OnMouseDown;
            view.MouseUp += OnMouseUp;
            view.MouseMove += OnMouseMove;
            view.MouseLeave += OnMouseLeave;
            view.MouseRightButtonUp += (_, __) => ResetView();

            view.Start(settings);

            Renderer.Initialize();
            
            Session.SelectedElementChanged += Rebuild;
            Session.SelectedLayerChanged += Rebuild;
        }

        public void UseElement(Element element)
        {
            CrossSectionBuilder.Element = element;
            Rebuild();
        }

        public void ZoomIn() => SetZoom(_zoom + 0.1f);
        public void ZoomOut() => SetZoom(_zoom - 0.1f);
        public void ResetView()
        {
            _zoom = 1f;
            _pan = Vector2.Zero;
            Invalidate();
        }
        public void SetZoom(double zoom)
        {
            _zoom = (float)Math.Clamp(zoom, 0.5, 5.0);
            Invalidate();
        }

        public void Rebuild()
        {
            TextureManager.Dispose();
            CrossSectionBuilder.RebuildCrossSection();
            SceneBuilder.BuildFrom(CrossSectionBuilder.DrawingGeometries);
            Invalidate();
        }

        private void Invalidate() => _view.InvalidateVisual();

        private void OnRender(TimeSpan _)
        {
            var size = new System.Drawing.Size((int)_view.ActualWidth, (int)_view.ActualHeight);
            var bounds = SceneBuilder.GetSceneBoundaries();
            var proj = BuildProjection(size, bounds);

            Renderer.Render(
                SceneBuilder.RectVertices.ToArray(),
                SceneBuilder.LineVertices.ToArray(),
                SceneBuilder.RectBatches,
                SceneBuilder.LineBatches,
                proj);
        }

        private Matrix4 BuildProjection(System.Drawing.Size control, BT.Geometry.Rectangle content)
        {
            var rectF = content.ToRectangleF();
            
            float ctrlW = control.Width, ctrlH = control.Height;
            float contW = rectF.Width, contH = rectF.Height;

            float scaleX = ctrlW / contW, scaleY = ctrlH / contH;
            float scale = MathF.Min(scaleX, scaleY) * _zoom;

            // Inital offsets set to the top-left corner of the content rectangle
            float dx = -rectF.X / (ctrlW / 2f), dy = -rectF.Y / (ctrlH / 2f);
            if (contW * scale < ctrlW) dx = (ctrlW - contW * scale) / 2f / (ctrlW / 2f);
            if (contH * scale < ctrlH) dy = (ctrlH - contH * scale) / 2f / (ctrlH / 2f);

            var zIndexStart = -100f;
            var zIndexEnd = 100f;

            Matrix4 view = Matrix4.CreateTranslation(1f, -1f, 0) *
                           Matrix4.CreateScale(scale) *
                           Matrix4.CreateTranslation(-1f + dx + _pan.X, 1f - dy + _pan.Y, 0);

            return Matrix4.CreateOrthographicOffCenter(0, ctrlW, ctrlH, 0, zIndexStart, zIndexEnd) * view;
        }

        private BT.Geometry.Point ConvertMouseToScene(Point mouse)
        {
            var bounds = SceneBuilder.GetSceneBoundaries();
            float cw = (float)_view.ActualWidth, ch = (float)_view.ActualHeight;
            float ew = (float)bounds.Width, eh = (float)bounds.Height;

            float scale = MathF.Min(cw / ew, ch / eh) * _zoom;

            float panPxX = _pan.X * cw / 2f;
            float panPxY = _pan.Y * ch / 2f;

            float offsetX = (cw - ew * scale) / 2f + panPxX;
            float offsetY = (ch - eh * scale) / 2f + panPxY;

            float x = (float)((mouse.X - offsetX) / scale + bounds.X);
            float y = (float)((mouse.Y - offsetY) / scale + bounds.Y);
            return new BT.Geometry.Point(x, y);
        }

        private void OnWheel(object s, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ZoomIn();
            else ZoomOut();
        }

        private void OnMouseDown(object s, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _dragging = true;
                _lastMousePos = e.GetPosition(_view);
                _view.CaptureMouse();
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                var pt = ConvertMouseToScene(e.GetPosition(_view));
                foreach (var geom in CrossSectionBuilder.DrawingGeometries)
                {
                    if (geom.Rectangle.Contains(pt))
                    {
                        ShapeClicked?.Invoke(geom.ShapeId);
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
                _view.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }

        private void OnMouseMove(object s, MouseEventArgs e)
        {
            if (_dragging)
            {
                var cur = e.GetPosition(_view);
                Vector delta = cur - _lastMousePos;
                _lastMousePos = cur;

                float dx = (float)(2.0 * delta.X / _view.ActualWidth);
                float dy = (float)(-2.0 * delta.Y / _view.ActualHeight);

                _pan += new Vector2(dx, dy);
                Invalidate();
            }
            else
            {
                var pt = ConvertMouseToScene(e.GetPosition(_view));
                foreach (var shape in CrossSectionBuilder.DrawingGeometries)
                {
                    if (shape.Rectangle.Contains(pt))
                    {
                        ShapeHovered?.Invoke(shape.ShapeId);
                        ToolTipService.SetToolTip(_view, shape.ShapeId.ToString());
                        Mouse.OverrideCursor = Cursors.Hand;
                        return;
                    }
                }
                ToolTipService.SetToolTip(_view, null);
                Mouse.OverrideCursor = null;
            }
        }

        private void OnMouseLeave(object s, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
            ToolTipService.SetToolTip(_view, null);
        }

        public void Dispose()
        {
            if (_disposed) return;

            Renderer.Dispose();
            TextureManager.Dispose();

            _view.Render -= OnRender;
            _view.MouseWheel -= OnWheel;
            _view.MouseDown -= OnMouseDown;
            _view.MouseUp -= OnMouseUp;
            _view.MouseMove -= OnMouseMove;
            _view.MouseLeave -= OnMouseLeave;

            Session.SelectedElementChanged -= Rebuild;
            Session.SelectedLayerChanged -= Rebuild;

            _disposed = true;
        }
    }
}
