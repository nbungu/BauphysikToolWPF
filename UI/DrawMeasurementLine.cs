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
    public class DrawMeasurementLine
    {
        public DrawMeasurementLine(Grid grid, List<Layer> layers = null) // optional parameter "layers"
        {
            // when "layers" param wasnt used (== null), use empty list
            layers ??= new List<Layer>(); 
            Draw(layers, grid, 1);
        }

        // Horizontal measurement Line - Grid must have same size a the measured object (e.g. a canvas)
        public void Draw(List<Layer> layers, Grid container, double strokeWidth)
        {
            container.Children.Clear();
            double fullWidth_px = container.Width;
            double right = fullWidth_px;

            // Horizontal base line
            Line baseLine = new Line() { X2 = right, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(baseLine);

            // first vertical tick
            Line lineStart = new Line() { Y2 = 12, X1 = right - strokeWidth / 2, X2 = right - strokeWidth / 2, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(lineStart);

            if(layers.Count != 0)
            {
                double elementWidth = 0;
                foreach (Layer layer in layers)
                {
                    elementWidth += layer.LayerThickness;
                }
                // Drawing from right to left
                for (int i = 0; i < layers.Count; i++)
                {
                    if (i == layers.Count)
                        break; // last line is drawn manually

                    double layerWidthScale = layers[i].LayerThickness / elementWidth; // from  0 ... 1
                    double layerWidth = fullWidth_px * layerWidthScale;
                    right -= layerWidth;

                    // vertical tick
                    Line line = new Line() { Y2 = 12, X1 = right, X2 = right, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
                    container.Children.Add(line);
                }
            }
            else
            {
                right = 0;
            }
            // last vertical tick
            Line lineEnd = new Line() { Y2 = 12, X1 = right+strokeWidth/2, X2 = right+strokeWidth/2, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(lineEnd);
        }
    }
}
