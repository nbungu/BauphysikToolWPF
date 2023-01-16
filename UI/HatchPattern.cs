using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI
{
    public static class HatchPattern
    {
        public static DrawingBrush GetHatchPattern(string category, double rectWidth, double rectHeight)
        {
            DrawingBrush brush = new DrawingBrush();
            switch (category)
            {
                case "Insulation":
                    brush = GetInsulationBrush(rectWidth, rectHeight);
                    break;
                default:
                    break;
            }
            return brush;
        }
        
        /*public static DrawingBrush GetPlasterBrush(double rectWidth, double rectHeight)
        {
            int filterDistortion = Convert.ToInt32(rectWidth); // avoid distortion when drawing the ellipse
            
            double area = rectWidth*rectHeight;

            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();
            

            int iMax = Convert.ToInt32(area/300); //increase the number of points for bigger areas; decrease for smaller ones
            for (int i=0; i<100; i++)
            {
                EllipseGeometry ellipse = new EllipseGeometry(new Point(new Random().Next(0, 200), new Random().Next(0, 200)), new Random().Next(2, 4), new Random().Next(2, 4), new RotateTransform(new Random().Next(0,45)));
                hatchContent.Children.Add(ellipse);
            }   
            hatchContent.Transform= new ScaleTransform(1.0, 0.5);

            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush() { Drawing = new GeometryDrawing(new SolidColorBrush(Colors.Gray), new Pen(Brushes.Black, 0.2), hatchContent) };

            return brush;
        }*/

        public static DrawingBrush GetInsulationBrush(double rectWidth, double rectHeight)
        {
            double w_h_ratio = rectWidth / rectHeight;

            //Imaginary 60x60 Rectangle
            double arcRad = 10;
            double arcEndX_Left = arcRad;
            double arcEndX_Right = 60 - arcRad;
            double currentY_Left = 0;

            //Imaginary Rectangle, coordinate origin is at top left corner
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(0, 0); // Startpoint of the segment series
            //pathFigure.IsFilled = true;

            int iMax = Convert.ToInt32(-20 * w_h_ratio + 20); //increase the number of loops for narrow rectangles; decrease for broader ones
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

            // Use the hatch lines as the Drawing's content
            DrawingBrush brush = new DrawingBrush() { Drawing = new GeometryDrawing(null, new Pen(Brushes.Black, 0.4), hatchContent) };

            return brush;
        }

        public static ImageBrush GetImageBrush(string source)
        {
            string uriBase = "../../../Resources/Icons/";
            string uri = uriBase + source;

            ImageBrush textureBrush = new ImageBrush(new BitmapImage(new Uri(uri, UriKind.Relative)));
            // Set the OpacityMask of the rectangle to the texture brush
            return textureBrush;
        }
    }
}
