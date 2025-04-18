using System.Collections.Generic;
using System.Windows.Media;
using BT.Geometry;

namespace BauphysikToolWPF.Models.UI
{
    public class DrawingGeometry : IDrawingGeometry
    {
        public int InternalId { get; set; } = -1;
        // Rectangle on 2D Canvas. Drawing Origin (0,0) is top left corner.
        public Rectangle Rectangle { get; set; } = Rectangle.Empty; // in [px]

        // Appearance of the Layer as a 2D Rectangle
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        public Brush RectangleBorderColor { get; set; } = Brushes.Transparent;
        public double RectangleBorderThickness { get; set; }
        public DoubleCollection RectangleStrokeDashArray { get; set; } = new DoubleCollection();
        public double Opacity { get; set; } = 1.0;
        public int ZIndex { get; set; }
        public object Tag { get; set; } = new object();
        public bool IsValid => Rectangle != Rectangle.Empty;

        public DrawingGeometry(IDrawingGeometry drawingGeometry)
        {
            InternalId = drawingGeometry.InternalId;
            Rectangle = drawingGeometry.Rectangle;
            BackgroundColor = drawingGeometry.BackgroundColor;
            DrawingBrush = drawingGeometry.DrawingBrush;
            RectangleBorderColor = drawingGeometry.RectangleBorderColor;
            RectangleBorderThickness = drawingGeometry.RectangleBorderThickness;
            RectangleStrokeDashArray = drawingGeometry.RectangleStrokeDashArray;
            Opacity = drawingGeometry.Opacity;
            ZIndex = drawingGeometry.ZIndex;
            Tag = drawingGeometry.Tag;
        }

        public DrawingGeometry() { }

        public DrawingGeometry Copy()
        {
            return new DrawingGeometry(this);
        }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        public void UpdateGeometry()
        {
            throw new System.NotImplementedException();
        }

        // For use as single collection Type in XAML Items Source of Canvas
        public List<DrawingGeometry> ToList()
        {
            return new List<DrawingGeometry>() { this };
        }
    }
}
