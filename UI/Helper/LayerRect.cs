using BauphysikToolWPF.SQLiteRepo;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.Helper
{
    public class LayerRect
    {
        // Rectangle x,y Coordinates on 2D Canvas. Drawing Origin (0,0) is top left corner.
        public double Top { get; set; }
        public double Left { get; set; }

        // Dimensions of the Layer as a 2D Rectangle
        public double RectWidth { get; set; } // [px]
        public double RectWidth_cm { get; set; } // [cm]
        public double RectHeight { get; set; } // [px]
        public string RectPosition { get; set; } = "";

        // Appearance of the Layer as a 2D Rectangle
        public Brush RectFill { get; set; } = new SolidColorBrush();
        public Brush RectHatchPattern { get; set; } = new SolidColorBrush();
        public Brush RectStroke { get; set; } = new SolidColorBrush();
        public double RectStrokeThickness { get; set; }
        public double RectOpacity { get; set; }

        public LayerRect(double elementWidth, double canvasWidth, double canvasHeight, Layer layer, LayerRect? previousLayerRect)
        {
            if (previousLayerRect != null) Left = previousLayerRect.Left + previousLayerRect.RectWidth;

            double scalingFactor = layer.LayerThickness / elementWidth; // from  0 ... 1
            double rectangleWidth = canvasWidth * scalingFactor;

            RectWidth = rectangleWidth;
            RectWidth_cm = layer.LayerThickness;
            RectHeight = canvasHeight;
            RectPosition = layer.LayerPosition.ToString();
            RectFill = new SolidColorBrush(layer.Material.Color);
            RectHatchPattern = HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, rectangleWidth, canvasHeight);
            RectStroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            RectStrokeThickness = layer.IsSelected ? 1 : 0.2;
            RectOpacity = layer.IsEffective ? 1 : 0.2;
        }

        // TODO add ToString()
    }
}
