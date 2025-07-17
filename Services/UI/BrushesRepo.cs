using BT.Geometry;
using System;
using System.Windows;
using System.Windows.Media;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using Point = System.Windows.Point;
using Vector = BT.Geometry.Vector;

namespace BauphysikToolWPF.Services.UI
{
    public static class BrushesRepo
    {
        private static double _currentLineThickness;
        //private static Rectangle _currentRectangle;
        private static bool _redraw = true;

        public static DrawingBrush? PlasterBrush { get; private set; }
        public static DrawingBrush? InsulationBrush { get; private set; }
        public static DrawingBrush? WoodBrush { get; private set; }
        public static DrawingBrush? MasonryBrush { get; private set; }
        public static DrawingBrush? ConcreteBrush { get; private set; }
        public static DrawingBrush? AirLayerBrush { get; private set; }
        public static DrawingBrush? SealantBrush { get; private set; }

        public static DrawingBrush GetHatchPattern(MaterialCategory category, double lineThickness, Rectangle rectangle)
        {
            if (rectangle.Width <= 0 || rectangle.Height <= 0) return new DrawingBrush();

            _redraw = Math.Abs(lineThickness - _currentLineThickness) > 1E-06;
            _currentLineThickness = lineThickness;
            
            // return corresp. class variable if already set, otherwise draw new Brush
            switch (category)
            {
                case MaterialCategory.Insulation:
                    return GetInsulationBrush(rectangle, lineThickness);
                case MaterialCategory.Concrete:
                    if (_redraw || ConcreteBrush is null) return GetConcreteBrush(lineThickness);
                    return ConcreteBrush;
                case MaterialCategory.Masonry:
                    if (_redraw || MasonryBrush is null) return GetMasonryBrush(lineThickness);
                    return MasonryBrush;
                case MaterialCategory.Plasters:
                    if (_redraw || PlasterBrush is null) return GetPlasterBrush(lineThickness);
                    return PlasterBrush;
                case MaterialCategory.Sealant:
                    if (_redraw || SealantBrush is null) return GetSealantBrush();
                    return SealantBrush;
                case MaterialCategory.Air:
                    if (_redraw || AirLayerBrush is null) return GetAirLayerBrush(lineThickness);
                    return AirLayerBrush;
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
            PlasterBrush = brush;
            return PlasterBrush;
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
            ConcreteBrush = brush;
            return ConcreteBrush;
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
            SealantBrush = brush;
            return SealantBrush;
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
            MasonryBrush = brush;
            return MasonryBrush;
        }

        //Brush for Wärmedämmung
        // Horizontally
        private static DrawingBrush GetInsulationBrush(Rectangle rectangle, double lineThickness)
        {
            bool isVertical = rectangle.Height > rectangle.Width;
            double widthToHeightRatio = isVertical ? (rectangle.Width / rectangle.Height) : (rectangle.Height / rectangle.Width);
            widthToHeightRatio = Math.Max(0.0005, Math.Abs(widthToHeightRatio));

            double radRatio = 0.167; // 1:6
            double arcRad = isVertical ? rectangle.Width * radRatio : rectangle.Height * radRatio;
            double arcEndXLeft = arcRad;
            double arcEndXRight = isVertical ? rectangle.Width - arcRad : rectangle.Height - arcRad;
            double currentYLeft = 0;

            //// Initial 60x60 Square with
            //double arcRad = 10;
            //double arcEndXLeft = arcRad;
            //double arcEndXRight = 60 - arcRad;
            //double currentYLeft = 0;

            //Imaginary Rectangle, coordinate origin is at top left corner
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(0, 0); // Startpoint of the segment series


            int iMax = Math.Max(6, Convert.ToInt32(-20 * Math.Log(2*widthToHeightRatio)));
            //int iMax = Math.Max(6, Convert.ToInt32(-40 * w_h_ratio*w_h_ratio + 40)); //increase the number of loops for narrow rectangles; decrease for broader ones
            for (int i = 0; i < iMax; i++)
            {
                currentYLeft += arcRad;
                ArcSegment startingArc = new ArcSegment() //First quarter circle 
                {
                    Point = new Point(arcEndXLeft, currentYLeft), // Connects previous Point with this (End)point of the Segment
                    Size = new Size(arcRad, arcRad),
                    SweepDirection = SweepDirection.Counterclockwise
                };
                pathFigure.Segments.Add(startingArc);

                LineSegment connectingLineLtr = new LineSegment()
                {
                    Point = new Point(arcEndXRight, currentYLeft - arcRad) // Connects previous Point with this (End)point of the Segment
                };
                pathFigure.Segments.Add(connectingLineLtr);

                currentYLeft += arcRad;
                ArcSegment rightArc1 = new ArcSegment()
                {
                    Point = new Point(arcEndXRight, currentYLeft), // Connects previous Point with this (End)point of the Segment
                    Size = new Size(arcRad, arcRad),
                    SweepDirection = SweepDirection.Clockwise,
                };
                pathFigure.Segments.Add(rightArc1);

                LineSegment connectingLineRtl = new LineSegment()
                {
                    Point = new Point(arcEndXLeft, currentYLeft - arcRad) // Connects previous Point with this (End)point of the Segment
                };
                pathFigure.Segments.Add(connectingLineRtl);

                ArcSegment endArc = new ArcSegment() //End quarter circle 
                {
                    Point = new Point(0, currentYLeft), // Connects previous Point with this (End)point of the Segment
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
            DrawingBrush brush = new DrawingBrush()
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.DimGray, lineThickness), hatchContent),
                TileMode = TileMode.None,
                Viewport = new Rect(0, 0, hatchContent.Bounds.Width, hatchContent.Bounds.Height),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, hatchContent.Bounds.Width, hatchContent.Bounds.Height),
                ViewboxUnits = BrushMappingMode.Absolute,
            };
            
            InsulationBrush = brush;
            return InsulationBrush;
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
            AirLayerBrush = brush;
            return AirLayerBrush;
        }

        public static Pen GetDottedPen(Color color, double thickness)
        {
            return new Pen(new SolidColorBrush(color), thickness)
            {
                DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
            };
        }
        public static Pen GetDottedPen(Brush color, double thickness)
        {
            return new Pen(color, thickness)
            {
                DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
            };
        }
        public static Pen GetDashedPen(Brush color, double thickness)
        {
            return new Pen(color, thickness)
            {
                DashStyle = new DashStyle(new double[] { 12, 12 }, 0)
            };
        }

        public static DrawingBrush GetCircleWithNumberBrush(string text, Brush backgroundColor, Brush borderBrush, double borderThickness, Brush textColor, Vector textOffset = new Vector())
        {
            double radius = 10; // Kleinerer Kreis mit 20px Durchmesser

            // Kreis-Geometrie
            EllipseGeometry circleGeometry = new EllipseGeometry(new Point(radius, radius), radius, radius);

            // Text-Formatierung
            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                14, // Schriftgröße
                textColor,
                1.0 // PixelsPerDip
            )
            {
                TextAlignment = TextAlignment.Center
            };

            // Zeichnung erstellen
            DrawingGroup drawingGroup = new DrawingGroup();

            // Kreis-Hintergrund
            drawingGroup.Children.Add(new GeometryDrawing(backgroundColor, new Pen(borderBrush, borderThickness), circleGeometry));

            // Text als gezeichnetes Bild einfügen (kein Geometry)
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext ctx = visual.RenderOpen())
            {
                ctx.DrawText(formattedText, new Point(radius - formattedText.Width / 2 + textOffset.X, radius - formattedText.Height / 2 + textOffset.Y));
            }
            drawingGroup.Children.Add(visual.Drawing);

            return new DrawingBrush
            {
                Drawing = drawingGroup,
                TileMode = TileMode.None,
                Viewport = new Rect(0, 0, radius * 2, radius * 2),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }
    }
}
