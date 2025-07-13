using BT.Geometry;
using System.Windows.Media;
using BauphysikToolWPF.Services.UI.OpenGL;
using Brush = System.Windows.Media.Brush;
using static BauphysikToolWPF.Services.UI.OpenGL.ElementScene;

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
        int? TextureId { get; set; } // Texture ID for OpenGL rendering, if applicable
        ShapeId ShapeId { get; set; } // Unique ID for this shape, used to identify it in the OpenGL scene
        HatchFitMode HatchFitMode { get; set; } // How to fit the hatch pattern inside the rectangle
        IDrawingGeometry Convert();
    }
}
