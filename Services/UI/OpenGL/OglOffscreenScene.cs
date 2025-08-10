using BauphysikToolWPF.Models.Domain;
using BT.Geometry;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Provides functionality to capture a rendered OpenGL scene as a BitmapSource
    /// without relying on a visible GLWpfControl.
    /// This uses the same projection logic as OglController.BuildProjection()
    /// so the result matches what would be drawn in the UI.
    /// </summary>
    public static class OglOffscreenScene
    {
        /// <summary>
        /// Renders the given element's scene offscreen and returns it as a BitmapSource.
        /// Matches WPF GLWpfControl visuals by using identical projection math.
        /// </summary>
        /// <param name="element">The element to render (must be valid for scene building).</param>
        /// <param name="width">Target image width in pixels.</param>
        /// <param name="height">Target image height in pixels.</param>
        /// <param name="zoom">Optional zoom factor to apply when rendering.</param>
        /// <param name="dpi">Output DPI (defaults to 96).</param>
        /// <returns>A frozen BitmapSource containing the rendered scene.</returns>
        public static BitmapSource CaptureElementImage(
            Element element,
            int width,
            int height,
            double zoom = 1.0,
            int dpi = 96)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Create an invisible OpenGL context using OpenTK's GameWindow
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(width, height),
                //Size = new Vector2i(width, height),
                StartVisible = false,
                StartFocused = false,
                APIVersion = new Version(4, 3), // Match GLWpfControl
                Profile = ContextProfile.Core
            };

            // GameWindow from OpenTK is used purely to get a valid GL context — no window is shown.
            using var game = new GameWindow(GameWindowSettings.Default, nativeSettings);
            game.MakeCurrent();

            // Renderer setup
            var textureManager = new TextureManager();
            var renderer = new OglRenderer(textureManager);
            renderer.Initialize();

            // Build scene using same logic as OglController.Redraw()
            var sceneBuilder = new ElementSceneBuilder(element, DrawingType.CrossSection);
            //{
            //    TextureManager = textureManager,
            //    ZoomFactor = (float)zoom
            //};
            sceneBuilder.TextureManager = textureManager;
            sceneBuilder.ZoomFactor = (float)zoom;


            sceneBuilder.RectVertices.Clear();
            sceneBuilder.LineVertices.Clear();
            sceneBuilder.RectBatches.Clear();
            sceneBuilder.LineBatches.Clear();
            sceneBuilder.SceneShapes.Clear();
            sceneBuilder.BuildScene();
            sceneBuilder.SceneShapes.Sort((a, b) => b.ZIndex.CompareTo(a.ZIndex));

            // Use identical projection math from OglController.BuildProjection()
            var projection = BuildProjection(
                new Size(width, height),
                sceneBuilder.SceneBounds,
                zoom,
                Vector.Empty // No panning in offscreen mode
            );

            // Capture to BitmapSource
            var bmp = renderer.CaptureOffscreenToBitmap(
                sceneBuilder.RectVertices.ToArray(),
                sceneBuilder.LineVertices.ToArray(),
                sceneBuilder.RectBatches,
                sceneBuilder.LineBatches,
                projection,
                width,
                height,
                dpi
            );

            return bmp;
        }

        /// <summary>
        /// Replicates OglController.BuildProjection() exactly, so output matches WPF rendering.
        /// </summary>
        private static Matrix4 BuildProjection(
            Size control,
            Rectangle content,
            double zoomFactor,
            Vector pan)
        {
            var rectF = content.ToRectangleF();

            float ctrlW = (float)control.Width, ctrlH = (float)control.Height;
            float contW = rectF.Width, contH = rectF.Height;

            float scaleX = ctrlW / contW;
            float scaleY = ctrlH / contH;
            float scale = MathF.Min(scaleX, scaleY) * (float)zoomFactor;

            float baseDivisorWidth = ctrlW / 2f;
            float baseDivisorHeight = ctrlH / 2f;

            // Initial offsets: top-left corner of the content rectangle
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
                           Matrix4.CreateTranslation(
                               -1f + dx + (float)pan.X,
                               1f - dy + (float)pan.Y,
                               0
                           );

            return Matrix4.CreateOrthographicOffCenter(0, ctrlW, ctrlH, 0, zIndexStart, zIndexEnd) * view;
        }
    }
}
