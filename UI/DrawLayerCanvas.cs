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
using System.Reflection.Emit;

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
                    StrokeThickness = layer.IsSelected ? 1.4 : 0.4,
                    Fill = (layer.Material.Category == "Insulation") ? GetInsulationHatchPattern(layerWidth, canvasHeight) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode)),
                    //OpacityMask = GetTextureFromImage()
                };
                System.Windows.Controls.Label label = new System.Windows.Controls.Label()
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

        private DrawingBrush GetInsulationHatchPattern(double rectWidth, double rectHeigt)
        {
            double w_h_ratio = rectWidth / rectHeigt;

            DrawingBrush brush = new DrawingBrush();
            // Create a GeometryGroup to contain the hatch lines
            GeometryGroup hatchContent = new GeometryGroup();          

            PathGeometry pathGeometry = new PathGeometry();

            //Imaginary 60x60 Rectangle
            double arcRad = 10;
            double arcEndX_Left = arcRad;
            double arcEndX_Right = 60 - arcRad;

            double currentY_Left = 0;

            //Imaginary Rectangle, coordinate origin is at top left corner
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(0, 0); // Startpoint of the segment series
            pathFigure.IsClosed = false;
            pathFigure.IsFilled = false;

            int iMax = Convert.ToInt32(-20 * w_h_ratio + 20); //increase the number of loops for narrow rectangles; decrease for broader ones
            for (int i = 0; i<iMax; i++)
            {
                currentY_Left += arcRad;
                //First quarter circle 
                ArcSegment startingArc = new ArcSegment();
                startingArc.Point = new Point(arcEndX_Left, currentY_Left); // Connects previous Point with this (End)point of the Segment
                startingArc.Size = new Size(arcRad, arcRad);
                startingArc.IsLargeArc = false;
                startingArc.SweepDirection = SweepDirection.Counterclockwise;
                pathFigure.Segments.Add(startingArc);

                LineSegment connectingLineLTR = new LineSegment() { Point = new Point(arcEndX_Right, currentY_Left-arcRad) }; // Connects previous Point with this (End)point of the Segment
                pathFigure.Segments.Add(connectingLineLTR);

                currentY_Left += arcRad;
                ArcSegment rightArc1 = new ArcSegment();             
                rightArc1.Point = new Point(arcEndX_Right, currentY_Left); // Connects previous Point with this (End)point of the Segment
                rightArc1.Size = new Size(arcRad, arcRad);
                rightArc1.IsLargeArc = false;
                rightArc1.SweepDirection = SweepDirection.Clockwise;
                pathFigure.Segments.Add(rightArc1);

                LineSegment connectingLineRTL = new LineSegment() { Point = new Point(arcEndX_Left, currentY_Left - arcRad) }; // Connects previous Point with this (End)point of the Segment
                pathFigure.Segments.Add(connectingLineRTL);

                //End quarter circle 
                ArcSegment endArc = new ArcSegment();
                endArc.Point = new Point(0, currentY_Left); // Connects previous Point with this (End)point of the Segment
                endArc.Size = new Size(arcRad, arcRad);
                endArc.IsLargeArc = false;
                endArc.SweepDirection = SweepDirection.Counterclockwise;
                pathFigure.Segments.Add(endArc);
            }
            pathGeometry.Figures.Add(pathFigure);
            hatchContent.Children.Add(pathGeometry);

            // Use the hatch lines as the Drawing's content
            brush.Drawing = new GeometryDrawing(new SolidColorBrush(Colors.Red), new Pen(Brushes.Black, 0.2), hatchContent);
            
            return brush;
        }
    }
}
