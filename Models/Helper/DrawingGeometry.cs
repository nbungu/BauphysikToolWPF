using System.Windows.Media;
using BauphysikToolWPF.UI.Helper;
using Geometry;

namespace BauphysikToolWPF.Models.Helper
{
    /// <summary>
    /// Presentation logic of a drawable element in the XAML Canvas
    /// </summary>
    public class DrawingGeometry // RF to DrawingGeometry
    {
        // Rectangle on 2D Canvas. Drawing Origin (0,0) is top left corner.
        public Rectangle Rectangle { get; set; } = Rectangle.Empty; // in [px]

        // Appearance of the Layer as a 2D Rectangle
        public Brush BackgroundColor { get; set; } = new SolidColorBrush();
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 1.0;
        public double Opacity { get; set; } = 1.0;

        public DrawingGeometry() { }

        public void UpdateGeometry(Layer layer)
        {
            var initWidth = layer.LayerThickness; // cm
            var initHeight = 100;                       // cm

            Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight);
            BackgroundColor = new SolidColorBrush(layer.Material.Color);
            DrawingBrush = HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, initWidth, initHeight);
            StrokeColor = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            StrokeThickness = layer.IsSelected ? 1 : 0.2;
            Opacity = layer.IsEffective ? 1 : 0.2;
        }
    }
}
