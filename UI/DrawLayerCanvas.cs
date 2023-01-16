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
                    Fill = Test3(),//new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode)),
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

        private DrawingBrush Test3()
        {


            DrawingBrush brush = new DrawingBrush();

            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();            

            PathGeometry pathGeometry = new PathGeometry();

            //Imaginary 60x60 Rectangle
            double arcRad = 10;
            double arcEndX_Left = arcRad;
            double arcEndX_Right = 60 - arcRad;

            double currentY_Left = 10;
            double currentY_Right = 0;

            //Imaginary Rectangle, coordinate origin is at top left corner
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(arcEndX_Left, 0); // Startpoint of the segment series
            pathFigure.IsClosed = false;
            pathFigure.IsFilled = false;


            for(int i = 0; i<4; i++)
            {
                currentY_Left += arcRad;
                ArcSegment leftArc1 = new ArcSegment();                
                leftArc1.Point = new Point(arcEndX_Left, currentY_Left); // Connects previous Point with this (End)point of the Segment
                leftArc1.Size = new Size(arcRad, arcRad);
                leftArc1.IsLargeArc = false;
                leftArc1.SweepDirection = SweepDirection.Counterclockwise;
                pathFigure.Segments.Add(leftArc1);

                currentY_Right += arcRad;
                LineSegment connectingLineLTR = new LineSegment() { Point = new Point(arcEndX_Right, currentY_Right) }; // Connects previous Point with this (End)point of the Segment
                pathFigure.Segments.Add(connectingLineLTR);

                currentY_Left += arcRad;
                ArcSegment rightArc1 = new ArcSegment();             
                rightArc1.Point = new Point(arcEndX_Right, currentY_Left); // Connects previous Point with this (End)point of the Segment
                rightArc1.Size = new Size(arcRad, arcRad);
                rightArc1.IsLargeArc = false;
                rightArc1.SweepDirection = SweepDirection.Clockwise;
                pathFigure.Segments.Add(rightArc1);

                currentY_Right += arcRad;
                LineSegment connectingLineRTL = new LineSegment() { Point = new Point(arcEndX_Left, currentY_Right) }; // Connects previous Point with this (End)point of the Segment
                pathFigure.Segments.Add(connectingLineRTL);
            }
            pathGeometry.Figures.Add(pathFigure);

            hatchContent.Children.Add(pathGeometry);
            hatchContent.Transform = new RotateTransform(45);

            // Use the hatch lines as the Drawing's content
            brush.Drawing = new GeometryDrawing(null, new Pen(Brushes.Black, 0.2), hatchContent);

            return brush;
        }
    }
}
