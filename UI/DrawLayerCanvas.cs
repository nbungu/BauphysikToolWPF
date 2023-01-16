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
using SkiaSharp;
using Xceed.Wpf.Toolkit.Converters;

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
        public Canvas Canvas { get; set; } // holds the layer rectangles

        public Grid Grid { get; set; } // holds the measurement lines

        public DrawLayerCanvas(List<Layer> layers, Canvas canvas, Grid grid)
        {
            this.Layers = layers;
            this.Canvas = canvas;
            this.Grid = grid;
            DrawRectanglesFromLayers();
        }
       
        public void DrawRectanglesFromLayers()
        {
            Canvas.Children.Clear();
            Grid.Children.Clear();
            
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

                Line line = new Line()
                {
                    Y1 = 0,
                    Y2 = 8,
                    X1 = right,
                    X2 = right,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    //RenderTransform = new RotateTransform(45),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.Children.Add(line);

                Rectangle baseRect = new Rectangle()
                {
                    Width = layerWidth,
                    Height = canvasHeight,
                    Stroke = layer.IsSelected ? Brushes.Blue : Brushes.Black,
                    StrokeThickness = layer.IsSelected ? 2 : 0.2,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode)),
                };

                // Draw layer rectangle
                Canvas.Children.Add(baseRect);
                Canvas.SetTop(baseRect, bottom);
                Canvas.SetLeft(baseRect, left);
                
                Rectangle hatchPatternRect = new Rectangle()
                {
                    Width = layerWidth - 1,
                    Height = canvasHeight,
                    Fill = HatchPattern.GetHatchPattern(layer.Material.Category, layerWidth, canvasHeight),
                    Opacity = 0.7
                };
                // Draw hatch pattern rectangle
                Canvas.Children.Add(hatchPatternRect);
                Canvas.SetTop(hatchPatternRect, bottom);
                Canvas.SetLeft(hatchPatternRect, left+0.5);

                // Add Label with layer position
                System.Windows.Controls.Label label = new System.Windows.Controls.Label()
                {
                    Content = layer.LayerPosition,
                    FontSize = 14
                };
                Canvas.Children.Add(label);
                Canvas.SetTop(label, bottom);
                Canvas.SetLeft(label, left);

                right -= layerWidth; // Add new layer at left edge of previous layer
            }
        }
    }
}
