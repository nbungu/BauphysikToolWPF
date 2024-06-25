﻿using System.Windows.Media;
using Geometry;

namespace BauphysikToolWPF.UI.Drawing
{
    public class DrawingGeometry : IDrawingGeometry
    {
        // Rectangle on 2D Canvas. Drawing Origin (0,0) is top left corner.
        public Rectangle Rectangle { get; set; } = Rectangle.Empty; // in [px]

        // Appearance of the Layer as a 2D Rectangle
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        public double RectangleBorderThickness { get; set; } = 1.0;
        public double Opacity { get; set; } = 1.0;
        public int ZIndex { get; set; } = 0;
        public object Tag { get; set; }

        public DrawingGeometry(IDrawingGeometry drawingGeometry)
        {
            if (drawingGeometry is null) return;
            Rectangle = drawingGeometry.Rectangle;
            BackgroundColor = drawingGeometry.BackgroundColor;
            DrawingBrush = drawingGeometry.DrawingBrush;
            RectangleBorderColor = drawingGeometry.RectangleBorderColor;
            RectangleBorderThickness = drawingGeometry.RectangleBorderThickness;
            Opacity = drawingGeometry.Opacity;
            ZIndex = drawingGeometry.ZIndex;
            Tag = drawingGeometry.Tag;
        }

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
    }
}
