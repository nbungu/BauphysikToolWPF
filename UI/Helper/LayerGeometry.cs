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
        //public double LayerThickness { get; set; } // [cm]
        //public string LayerPosition { get; set; } = string.Empty;

        // Appearance of the Layer as a 2D Rectangle
        public Brush BackgroundColor { get; set; } = new SolidColorBrush();
        public Brush HatchPattern { get; set; } = new DrawingBrush();
        public Brush BorderStroke { get; set; } = new SolidColorBrush();
        public double BorderThickness { get; set; }
        public double Opacity { get; set; }

        protected LayerGeometry() { }

        protected LayerGeometry(Layer layer)
        {
            var initWidth = layer.LayerThickness; // cm
            var initHeight = 100;                       // cm TODO: set to canvas Height

            Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight);
            BackgroundColor = new SolidColorBrush(layer.Material.Color);
            HatchPattern = Helper.HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, initWidth, initHeight);
            BorderStroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            BorderThickness = layer.IsSelected ? 1 : 0.2;
            Opacity = layer.IsEffective ? 1 : 0.2;
        }

        protected void Initialize(Layer layer)
        {
            var initWidth = layer.LayerThickness; // cm
            var initHeight = 100;                       // cm TODO: set to canvas Height

            Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight);
            BackgroundColor = new SolidColorBrush(layer.Material.Color);
            HatchPattern = Helper.HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, initWidth, initHeight);
            BorderStroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            BorderThickness = layer.IsSelected ? 1 : 0.2;
            Opacity = layer.IsEffective ? 1 : 0.2;
        }
    }
}
