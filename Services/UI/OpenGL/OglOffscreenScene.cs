using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BT.Geometry;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
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
        /// <param name="scene">The scene to render.</param>
        /// <param name="width">Target image width in pixels.</param>
        /// <param name="height">Target image height in pixels.</param>
        /// <param name="zoom">Optional zoom factor to apply when rendering.</param>
        /// <param name="dpi">Output DPI (defaults to 96).</param>
        /// <returns>A frozen BitmapSource containing the rendered scene.</returns>
        public static BitmapSource GetOffscreenSceneImage(
            IOglSceneBuilder scene,
            int width,
            int height,
            double zoom = 1.0,
            int dpi = 96)
        {
            if (scene == null) throw new ArgumentNullException(nameof(scene));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException("Width and height must be positive integers.");

            // Create an invisible OpenGL context using OpenTK's GameWindow
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(width, height),
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
            var sceneBuilder = scene;
            sceneBuilder.TextureManager = textureManager;
            sceneBuilder.ZoomFactor = (float)zoom;
            sceneBuilder.RectVertices.Clear();
            sceneBuilder.LineVertices.Clear();
            sceneBuilder.RectBatches.Clear();
            sceneBuilder.LineBatches.Clear();
            sceneBuilder.SceneShapes.Clear();
            sceneBuilder.BuildScene();

            // Use identical projection math from OglController.BuildProjection()
            var projection = OglController.BuildProjection(
                new Size(width, height),
                sceneBuilder.SceneBounds,
                (float)zoom,
                Vector.Empty // No panning in offscreen mode
            );

            // Capture to BitmapSource
            var bmp = renderer.CaptureRendering(
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
        /// Renders elements via offscreen capturing and assigns the resulting images to the elements.
        /// </summary>
        public static void GenerateImagesOfElements(IEnumerable<Element> elements, RenderTarget target, bool withDecorations)
        {
            var elementScene = new ElementSceneBuilder();
            elementScene.ShowSceneDecoration = withDecorations;
            elementScene.CrossSectionBuilder.DrawingType = DrawingType.CrossSection;
            elementScene.Dpi = target == RenderTarget.Screen ? 96 : 240; // TODO: as constants

            foreach (var element in elements)
            {
                if (element.Layers.Count == 0)
                {
                    element.Image = Array.Empty<byte>();
                    continue;
                }

                // Ensure Layer numbering is correct
                if (withDecorations) element.AssignInternalIdsToLayers();

                elementScene.CrossSectionBuilder.Element = element;
                var bmp = GetOffscreenSceneImage(
                    elementScene,
                    (int)elementScene.CrossSectionBuilder.CanvasSize.Width * elementScene.DpiScaleFactor, // Width
                    (int)elementScene.CrossSectionBuilder.CanvasSize.Height * elementScene.DpiScaleFactor, // Height
                    zoom: 1.0, // Zoom factor
                    dpi: elementScene.Dpi // DPI
                );
                element.Image = bmp.ToByteArray();
            }
        }
    }

    public enum RenderTarget
    {
        Screen,
        Document
    }
}
