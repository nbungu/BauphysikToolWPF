using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI
{
    public class DrawLayerCanvas
    {
        private List<Layer> layers;
        public List<Layer> Layers //for Validation
        {
            get { return layers; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("null layer list specified");
                layers = value;
            }
        }
        public Canvas Canvas { get; set; }

        public DrawLayerCanvas(List<Layer> layers, Canvas canvas)
        {
            this.Layers = layers;
            this.Canvas = canvas;
            DrawRectanglesFromLayers();
        }

        public void DrawRectanglesFromLayers()
        {
            Canvas.Children.Clear();
            
            if (Layers == null || Layers.Count == 0)
                return;

            double canvasHeight = Canvas.Height;
            double canvasWidth = Canvas.Width;
            double bottom = 0;
            double right = canvasWidth;
            double fullWidth = 0;

            //TODO refactor: variablen sollen nicht bei jedem foreach neu initialisiert und zugeweisen werden müssen

            //Get width of all layers combined to get fullWidth
            foreach (Layer layer in Layers)
            {
                fullWidth += layer.LayerThickness;
            }
            foreach (Layer layer in Layers)
            {
                double layerWidthScale = layer.LayerThickness / fullWidth; // from  0 ... 1
                double layerWidth = canvasWidth * layerWidthScale;
                double left = right - layerWidth; // start drawing from right canvas side (beginning with INSIDE Layer, which is first list element) -> We want Inside layer position on right/inner side. 

                // Set properties of the layer rectangle
                Rectangle rect = new Rectangle()
                {
                    Width = layerWidth,
                    Height = canvasHeight,
                    Stroke = layer.IsSelected ? Brushes.Blue : Brushes.Black,
                    StrokeThickness = layer.IsSelected ? 1 : 0.4,
                    Fill = Test3()//new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode)),
                   // OpacityMask = GetTextureFromImage()
                };
                Label label = new Label()
                {
                    Content = layer.LayerPosition,
                    FontSize = 14
                };

                // Draw layer rectangle
                Canvas.Children.Add(rect);
                Canvas.SetTop(rect, bottom);
                Canvas.SetLeft(rect, left);

                Canvas.Children.Add(label);

                Canvas.SetTop(label, bottom);
                Canvas.SetLeft(label, left);

                right -= layerWidth; // Add new layer at left edge of previous layer
            }
        }
        private ImageBrush GetTextureFromImage()
        {
            ImageBrush textureBrush = new ImageBrush(new BitmapImage(new Uri("../../../Resources/Icons/CL_001x80_climate_menu.png", UriKind.Relative)));

            // Set the OpacityMask of the rectangle to the texture brush
            return textureBrush;

        }

        /*private DrawingBrush Test()
        {
            DrawingBrush insulationBrush = new DrawingBrush();
            DrawingGroup drawingGroup = new DrawingGroup();

            // Create a Pen with a dashed line style and set its thickness and color
            Pen insulationPen = new Pen(Brushes.Gray, 1);
            insulationPen.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);

            // Create the geometry for the hatch pattern
            GeometryGroup insulationGeometry = new GeometryGroup();

            // Create a series of lines with 45 degree angles
            for (int i = 0; i < 10; i++)
            {
                LineGeometry line1 = new LineGeometry(new Point(i * 10, 0), new Point(i * 10 + 10, 10));
                LineGeometry line2 = new LineGeometry(new Point(i * 10, 10), new Point(i * 10 + 10, 0));
                insulationGeometry.Children.Add(line1);
                insulationGeometry.Children.Add(line2);
            }

            // Create a series of arcs
            for (int i = 0; i < 10; i++)
            {
                ArcSegment arc = new ArcSegment(, 0, false, SweepDirection.Clockwise, true);
                PathFigure figure = new PathFigure();
                figure.StartPoint = new Point(i * 10, 0);
                figure.Segments.Add(arc);
                PathGeometry path = new PathGeometry();
                path.Figures.Add(figure);
                insulationGeometry.Children.Add(path);
            }

            // Create a GeometryDrawing with the created GeometryGroup
            GeometryDrawing insulationDrawing = new GeometryDrawing(null, insulationPen, insulationGeometry);
            drawingGroup.Children.Add(insulationDrawing);


            // Set the Drawing property of the DrawingBrush to the DrawingGroup
            insulationBrush.Drawing = drawingGroup;

            // Set the Viewport property of the DrawingBrush to define the area that the brush will cover
            insulationBrush.Viewport = new Rect(0, 0, 0.5, 0.5);

            // Set the TileMode property of the DrawingBrush to define how the brush will be tiled across the area it covers
            insulationBrush.TileMode = TileMode.Tile;

            return insulationBrush;

        }*/

        private DrawingBrush Test3()
        {
            DrawingBrush brush = new DrawingBrush();

            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();

            PathGeometry pathGeometry = new PathGeometry();

            //Imaginary Rectangle, coordinate origin is at top left corner
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(10, 0); // Startpoint of the segment series
            pathFigure.IsClosed = false;
            pathFigure.IsFilled = false;

            ArcSegment arc = new ArcSegment();
            arc.Point = new Point(10,20); // Connects previous Point with this (End)point of the Segment
            arc.Size = new Size(10, 10);
            arc.IsLargeArc = false;
            arc.SweepDirection = SweepDirection.Counterclockwise;
            pathFigure.Segments.Add(arc);

            LineSegment line = new LineSegment() { Point = new Point(30, 11) }; // Connects previous Point with this (End)point of the Segment
            pathFigure.Segments.Add(line);

            ArcSegment arc2 = new ArcSegment();
            arc2.Point = new Point(30, 31); // Connects previous Point with this (End)point of the Segment
            arc2.Size = new Size(10, 10);
            arc2.IsLargeArc = false;
            arc2.SweepDirection = SweepDirection.Clockwise;
            pathFigure.Segments.Add(arc2);

            LineSegment line2 = new LineSegment() { Point = new Point(10, 21) }; // Connects previous Point with this (End)point of the Segment
            pathFigure.Segments.Add(line2);

            pathGeometry.Figures.Add(pathFigure);

            hatchContent.Children.Add(pathGeometry);

           // This will create a half - circle with the center at(50, 10), a radius of 20, and it starts from the point(50,10) and ends at(70, 10)



            // add the connecting lines
           // hatchContent.Children.Add(new LineGeometry(new System.Windows.Point(0, 0), new System.Windows.Point(10, 10)));
            //hatchContent.Children.Add(new LineGeometry(new System.Windows.Point(0, 10), new System.Windows.Point(10, 0)));
            // Use the hatch lines as the Drawing's content
            brush.Drawing = new GeometryDrawing(null, new Pen(Brushes.Black, 0.2), hatchContent);

            return brush;

            // Create a new Bitmap object
            /*Bitmap bmp = new Bitmap(100, 100);

            // Create a Graphics object from the Bitmap
            Graphics g = Graphics.FromImage(bmp);

            // Create a new Pen object with the desired color and width
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.Black, 2);

            // Draw half circles at the top and bottom of the rectangle
            g.DrawArc(pen, rect.Left, rect.Top, rect.Width, rect.Height / 2, 0, 180);
            g.DrawArc(pen, rect.Left, rect.Bottom - rect.Height / 2, rect.Width, rect.Height / 2, 0, 180);

            // Draw lines between the half circles to create the hatching pattern
            g.DrawLine(pen, rect.Left, rect.Top + rect.Height / 4, rect.Right, rect.Top + rect.Height / 4);
            g.DrawLine(pen, rect.Left, rect.Bottom - rect.Height / 4, rect.Right, rect.Bottom - rect.Height / 4);

            // Draw the rectangle
            g.DrawRectangle(new Pen(Brushes.Black, 1), rect);

            /*
                     // Create a new Pen object with the desired color and width
        Pen pen = new Pen(Brushes.Black, 2);
        pen.DashStyle = DashStyle.Custom;
        pen.DashPattern = new float[] { 4, 4 };

        // Draw half circles at the top and bottom of the rectangle
        g.DrawArc(pen, 0, 0, 100, 50, 0, 180);
        g.DrawArc(pen, 0, 50, 100, 50, 0, 180);

        // Draw lines between the half circles to create the hatching pattern
        g.DrawLine(pen, 0, 25, 100, 25);
        g.DrawLine(pen, 0, 75, 100, 75);
             */
        }
    }
}
