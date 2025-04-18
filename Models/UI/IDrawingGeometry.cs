using System.Windows.Media;
using BT.Geometry;
using Brush = System.Windows.Media.Brush;

namespace BauphysikToolWPF.Models.UI
{
    public interface IDrawingGeometry
    {
        int InternalId { get; set; }
        // Rectangle on 2D Canvas. Drawing Origin (0,0) is top left corner.
        Rectangle Rectangle { get; set; } // in [px]
        Brush RectangleBorderColor { get; set; }
        double RectangleBorderThickness { get; set; }
        DoubleCollection RectangleStrokeDashArray { get; set; }
        Brush BackgroundColor { get; set; }
        Brush DrawingBrush { get; set; }
        double Opacity { get; set; }
        int ZIndex { get; set; }
        object Tag { get; set; }
        IDrawingGeometry Convert();
    }
}
