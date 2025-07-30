using BT.Geometry;
using System.Windows.Media;
using BauphysikToolWPF.Services.UI.OpenGL;
using Brush = System.Windows.Media.Brush;

namespace BauphysikToolWPF.Models.UI
{
    public interface IDrawingGeometry
    {
        int InternalId { get; set; }
        // Rectangle on 2D Canvas. Drawing Origin (0,0) is top left corner.
        Rectangle Rectangle { get; set; } // in [px]
        Pen BorderPen { get; set; } // Pen for the rectangle border
        Brush BackgroundColor { get; set; }
        Brush TextureBrush { get; set; }
        double Opacity { get; set; }
        int ZIndex { get; set; }
        object Tag { get; set; }
        int? TextureId { get; set; } // Texture ID for OpenGL rendering, if applicable
        ShapeId ShapeId { get; set; } // Unique ID for this shape, used to identify it in the OpenGL scene
        int VertexStartIndex { get; set; }
        HatchFitMode HatchFitMode { get; set; } // How to fit the hatch pattern inside the rectangle
        IDrawingGeometry Convert();
    }
}
