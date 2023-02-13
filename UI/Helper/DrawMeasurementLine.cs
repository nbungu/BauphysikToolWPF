using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI.Helper
{
    // !! Width of the parent container must explicitly set in XAML
    public class DrawMeasurementLine
    {
        // custom parameter constructor, with optional parameters "strokeWidth" and "showLabels"
        public DrawMeasurementLine(Grid container, List<Layer> layers, double strokeWidth = 1, bool showLabels = true)
        {
            container.Children.Clear();
            if (layers == null || layers.Count == 0)
                return;

            // check if container was already created in frontend
            container = container ?? throw new ArgumentNullException(nameof(container));

            // Horizontal base line
            Line baseLine = new Line() { X2 = container.Width, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(baseLine);

            // first vertical tick
            Line lineStart = new Line() { Y2 = 12, X1 = strokeWidth / 2, X2 = strokeWidth / 2, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(lineStart);

            double x = 0;
            double elementWidth = 0;
            foreach (Layer layer in layers)
            {
                elementWidth += layer.LayerThickness;
            }

            // Drawing from left to right: first layer in list is inside
            for (int i = 0; i < layers.Count; i++)
            {   
                double layerWidthScale = layers[i].LayerThickness / elementWidth; // from  0 ... 1
                double layerWidth = container.Width * layerWidthScale;
                x += layerWidth;

                if (i == layers.Count - 1)
                    x -= strokeWidth / 2; // add offset to avoid clipping

                // Add vertical tick
                Line line = new Line() { Y2 = 12, X1 = x, X2 = x, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
                container.Children.Add(line);

                if (showLabels == true)
                {
                    Label label = new Label() { Content = Math.Round(layers[i].LayerThickness, 1).ToString(), FontSize = 10, VerticalContentAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Center };
                    label.Measure(new Size(40, 40));

                    double labelLeftMargin = x - label.DesiredSize.Width / 2 - layerWidth / 2; // space right of the label
                    double labelRightMargin = container.Width - label.DesiredSize.Width - labelLeftMargin; // space left of the label
                    label.Margin = new Thickness(labelLeftMargin, 0, labelRightMargin, 0);
                    container.Children.Add(label);
                }
            }
        }
    }
}
