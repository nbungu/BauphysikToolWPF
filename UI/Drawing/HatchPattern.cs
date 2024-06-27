using System;
using System.Windows;
using System.Windows.Media;
using BauphysikToolWPF.Models;
using SkiaSharp;

namespace BauphysikToolWPF.UI.Drawing
{
    public static class HatchPattern
    {
        //public static DrawingBrush? PlasterBrush { get; private set; }
        //public static DrawingBrush? InsulationBrush { get; private set; }
        //public static DrawingBrush? WoodBrush { get; private set; }
        //public static DrawingBrush? MasonryBrush { get; private set; }
        //public static DrawingBrush? ConcreteBrush { get; private set; }
        //public static DrawingBrush? AirLayerBrush { get; private set; }
        //public static DrawingBrush? SealantBrush { get; private set; }

        public static DrawingBrush GetHatchPattern(MaterialCategory category, double lineThickness, double rectWidth, double rectHeight)
        {
            if (rectWidth <= 0 || rectHeight <= 0) return new DrawingBrush();
            
            // return corresp. class variable if already set, otherwise draw new Brush
            switch (category)
            {
                case MaterialCategory.Insulation:
                    return GetInsulationBrush(rectWidth, rectHeight, lineThickness); // Draw new every time! Updates on every layer change, not only once
                case MaterialCategory.Concrete:
                    return GetConcreteBrush(lineThickness);
                //case MaterialCategory.Wood:
                //    return WoodBrush ??= GetWoodBrush(lineThickness);
                case MaterialCategory.Masonry:
                    return GetMasonryBrush(lineThickness);
                case MaterialCategory.Plasters:
                    return GetPlasterBrush(lineThickness);
                case MaterialCategory.Sealant:
                    return GetSealantBrush();
                case MaterialCategory.Air:
                    return GetAirLayerBrush(lineThickness);
                default:
                    return new DrawingBrush();
            }
        }

        // Brush for Mörtel/Putz
        private static DrawingBrush GetPlasterBrush(double lineThickness)
        {
            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();

            //only small ones
            for (int i = 0; i < 40; i++)
            {
                EllipseGeometry ellipse = new EllipseGeometry(new Point(new Random().Next(1, 127), new Random().Next(3, 125)), 1, 1);
                hatchContent.Children.Add(ellipse);
            }
            //only medium ones
            for (int i = 0; i < 15; i++)
            {
                EllipseGeometry ellipse = new EllipseGeometry(new Point(new Random().Next(4, 124), new Random().Next(4, 124)), new Random().Next(2, 3), new Random().Next(2, 3), new RotateTransform(new Random().Next(0, 45)));
                hatchContent.Children.Add(ellipse);
            }
            //only big ones
            for (int i = 0; i < 10; i++)
            {
                EllipseGeometry ellipse = new EllipseGeometry(new Point(new Random().Next(8, 120), new Random().Next(8, 120)), new Random().Next(3, 4), new Random().Next(3, 4), new RotateTransform(new Random().Next(0, 30)));
                hatchContent.Children.Add(ellipse);
            }
            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush()
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.Black, lineThickness), hatchContent),
                TileMode = TileMode.FlipXY,
                Viewport = new Rect(0, 0, 128, 128),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 128, 128),
                ViewboxUnits = BrushMappingMode.Absolute,
                Transform = new RotateTransform(15)

            };
            return brush;
        }

        // Brush for Beton (bewehrt)
        private static DrawingBrush GetConcreteBrush(double lineThickness)
        {
            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();

            LineGeometry line = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(32, 32) };
            hatchContent.Children.Add(line);
            LineGeometry line2 = new LineGeometry() { StartPoint = new Point(16, 0), EndPoint = new Point(32, 16) };
            hatchContent.Children.Add(line2);

            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.Black, lineThickness), hatchContent),
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 32, 32),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 32, 32),
                ViewboxUnits = BrushMappingMode.Absolute
            };
            return brush;
        }

        // Brush for Dichtstoffe
        private static DrawingBrush GetSealantBrush(bool drawHorizontally = true)
        {
            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();

            // Create Rectangle in a 16x40 Tile (0.5* total height of the brush (16x80))
            // Base BG-Color is black. Top Half of brush will be this Rectangle colored in white
            if (drawHorizontally) hatchContent.Children.Add(new RectangleGeometry() { Rect = new Rect(0, 0, 40, 16) });
            else hatchContent.Children.Add(new RectangleGeometry() { Rect = new Rect(0, 0, 16, 40) });

            //RectangleGeometry rect = new RectangleGeometry() { Rect = new Rect(0, 0, 16, 40) };
            //hatchContent.Children.Add(rect);

            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush()
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(Colors.White), new Pen(), hatchContent),
                TileMode = TileMode.Tile,
                Viewport = drawHorizontally ? new Rect(0, 0, 80, 16) : new Rect(0, 0, 16, 80),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = drawHorizontally ? new Rect(0, 0, 80, 16) : new Rect(0, 0, 16, 80),
                ViewboxUnits = BrushMappingMode.Absolute
            };
            return brush;
        }

        //Brush for Mauerwerk
        private static DrawingBrush GetMasonryBrush(double lineThickness)
        {
            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();

            LineGeometry line = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(32, 32) };
            hatchContent.Children.Add(line);

            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush()
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.Black, lineThickness), hatchContent),
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 32, 32),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 32, 32),
                ViewboxUnits = BrushMappingMode.Absolute
            };
            return brush;
        }

        //Brush for Wärmedämmung
        // Horizontally
        private static DrawingBrush GetInsulationBrush(double rectWidth, double rectHeight, double lineThickness)
        {
            bool isVertical = rectHeight > rectWidth;
            double w_h_ratio = isVertical ? (rectWidth / rectHeight) : (rectHeight / rectWidth);

            //Imaginary 60x60 Rectangle
            double arcRad = 10;
            double arcEndX_Left = arcRad;
            double arcEndX_Right = 60 - arcRad;
            double currentY_Left = 0;

            //Imaginary Rectangle, coordinate origin is at top left corner
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(0, 0); // Startpoint of the segment series

            int iMax = Math.Max(4, Convert.ToInt32(-20 * w_h_ratio + 25)); //increase the number of loops for narrow rectangles; decrease for broader ones
            for (int i = 0; i < iMax; i++)
            {
                currentY_Left += arcRad;
                ArcSegment startingArc = new ArcSegment() //First quarter circle 
                {
                    Point = new Point(arcEndX_Left, currentY_Left), // Connects previous Point with this (End)point of the Segment
                    Size = new Size(arcRad, arcRad),
                    SweepDirection = SweepDirection.Counterclockwise
                };
                pathFigure.Segments.Add(startingArc);

                LineSegment connectingLineLTR = new LineSegment()
                {
                    Point = new Point(arcEndX_Right, currentY_Left - arcRad) // Connects previous Point with this (End)point of the Segment
                };
                pathFigure.Segments.Add(connectingLineLTR);

                currentY_Left += arcRad;
                ArcSegment rightArc1 = new ArcSegment()
                {
                    Point = new Point(arcEndX_Right, currentY_Left), // Connects previous Point with this (End)point of the Segment
                    Size = new Size(arcRad, arcRad),
                    SweepDirection = SweepDirection.Clockwise,
                };
                pathFigure.Segments.Add(rightArc1);

                LineSegment connectingLineRTL = new LineSegment()
                {
                    Point = new Point(arcEndX_Left, currentY_Left - arcRad) // Connects previous Point with this (End)point of the Segment
                };
                pathFigure.Segments.Add(connectingLineRTL);

                ArcSegment endArc = new ArcSegment() //End quarter circle 
                {
                    Point = new Point(0, currentY_Left), // Connects previous Point with this (End)point of the Segment
                    Size = new Size(arcRad, arcRad),
                    SweepDirection = SweepDirection.Counterclockwise,
                };
                pathFigure.Segments.Add(endArc);
            }
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();
            hatchContent.Children.Add(pathGeometry);

            if (!isVertical)
            {
                var grp = new TransformGroup();
                grp.Children.Add(new RotateTransform(90));
                grp.Children.Add(new TranslateTransform(hatchContent.Bounds.Height, 0));
                hatchContent.Transform = grp;
            }

            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush() { Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.DimGray, lineThickness), hatchContent) };

            return brush;
        }

        // Brush for Luftschicht
        private static DrawingBrush GetAirLayerBrush(double lineThickness)
        {
            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();

            // Create 3 vertically stacked circles in a 16x40 Tile
            int y = 20;
            for (int i = 0; i < 3; i++)
            {
                EllipseGeometry ellipse = new EllipseGeometry(new Point(8, y), 3, 3);
                hatchContent.Children.Add(ellipse);
                y += 20;
            }
            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush()
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.Black, lineThickness), hatchContent),
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 16, 80),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 16, 80),
                ViewboxUnits = BrushMappingMode.Absolute,

            };
            return brush;
        }
    }
}
