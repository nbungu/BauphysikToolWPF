using System.Collections.Generic;
using BauphysikToolWPF.Models;
using Geometry;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.Helper
{
    /// <summary>
    /// Presentation logic of a Layer model
    /// </summary>
    public class LayerGeometry
    {
        // Rectangle on 2D Canvas. Drawing Origin (0,0) is top left corner.
        public Rectangle Rectangle { get; set; } = Rectangle.Empty; // in [px]
        public double LayerThickness { get; set; } // [cm]
        public string LayerPosition { get; set; } = string.Empty;

        // Appearance of the Layer as a 2D Rectangle
        public Brush BackgroundColor { get; set; } = new SolidColorBrush();
        public Brush HatchPattern { get; set; } = new DrawingBrush();
        public Brush BorderStroke { get; set; } = new SolidColorBrush();
        public double BorderThickness { get; set; }
        public double Opacity { get; set; }

        public LayerGeometry() { }

        public LayerGeometry(Layer layer)
        {
            var initWidth = layer.LayerThickness; // cm
            var initHeight = 100;                       // cm TODO: set to canvas Height

            Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight);
            LayerThickness = initWidth;
            LayerPosition = layer.LayerPosition.ToString();
            BackgroundColor = new SolidColorBrush(layer.Material.Color);
            HatchPattern = Helper.HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, 0, 0);
            BorderStroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            BorderThickness = layer.IsSelected ? 1 : 0.2;
            Opacity = layer.IsEffective ? 1 : 0.2;
        }

        /*public LayerGeometry(LayerSubConstruction subConstruction)
        {
            var initWidth = subConstruction.Width;   // cm
            var initHeight = subConstruction.Height; // cm 

            Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight);
            LayerThickness = initWidth;
            LayerPosition = layer.LayerPosition.ToString();
            BackgroundColor = new SolidColorBrush(layer.Material.Color);
            HatchPattern = Helper.HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, 0, 0);
            BorderStroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            BorderThickness = layer.IsSelected ? 1 : 0.2;
            Opacity = layer.IsEffective ? 1 : 0.2;

        }*/
    }

    public static class ElementDrawer
    {
        /*public LayerGeometry(double elementWidth, double canvasWidth, double canvasHeight, Layer layer, LayerGeometry? previousLayerRect)
        {
            if (previousLayerRect != null) Left = previousLayerRect.Left + previousLayerRect.RectWidth;

            double scalingFactor = layer.LayerThickness / elementWidth; // from  0 ... 1
            double rectangleWidth = canvasWidth * scalingFactor;

            RectWidth = rectangleWidth;
            LayerThickness = layer.LayerThickness;
            RectHeight = canvasHeight;
            LayerPosition = layer.LayerPosition.ToString();
            BackgroundColor = new SolidColorBrush(layer.Material.Color);
            HatchPattern = Helper.HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, rectangleWidth, canvasHeight);
            BorderStroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            BorderThickness = layer.IsSelected ? 1 : 0.2;
            Opacity = layer.IsEffective ? 1 : 0.2;
        }*/

        // stackedLayers.Add(new LayerGeometry(ElementWidth, 320, 400, layer, rectangles.LastOrDefault()));

        public static List<LayerGeometry> StackLayers(List<LayerGeometry> geometries)
        {
            if (geometries.Count == 0 || geometries is null) return new List<LayerGeometry>();

            Point ptStart = new Point(0, 0);
            foreach (var geometry in geometries)
            {
                geometry.Rectangle.MoveTo(ptStart);
                ptStart = geometry.Rectangle.TopRight;
            }
            return geometries;
        }

        /// <summary>
        /// In Px
        /// </summary>
        /// <param name="geometries"></param>
        /// <param name="canvasWidth"></param>
        /// <param name="canvasHeight"></param>
        /// <returns></returns>
        public static List<LayerGeometry> ScaleToFitCanvas(List<LayerGeometry> geometries, double canvasWidth = 320, double canvasHeight = 400)
        {
            if (geometries.Count == 0 || geometries is null) return new List<LayerGeometry>();

            foreach (var geometry in geometries)
            {
                geometry.Rectangle.MoveTo(ptStart);
                ptStart = geometry.Rectangle.TopRight;
            }
            return geometries;
        }

        // TODO: Scale to fit canvas
    }

}
