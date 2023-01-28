using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
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

            //TODO check if Width of the parent container was set in XAML-> container.Measure as catch?

            double fullWidth_px = container.Width;
            double right = fullWidth_px;

            // Horizontal base line
            Line baseLine = new Line() { X2 = right, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(baseLine);

            // first vertical tick
            Line lineStart = new Line() { Y2 = 12, X1 = right - strokeWidth / 2, X2 = right - strokeWidth / 2, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(lineStart);

            double elementWidth = 0;
            foreach (Layer layer in layers)
            {
                elementWidth += layer.LayerThickness;
            }

            double prevLayer = 0;
            // Drawing from right to left: first layer in list is inside (= right side)
            for (int i = 0; i < layers.Count; i++)
            {
                if (i == layers.Count)
                    break; // cancel last loop here: last line is drawn manually

                double layerWidthScale = layers[i].LayerThickness / elementWidth; // from  0 ... 1
                double layerWidth = fullWidth_px * layerWidthScale;
                right -= layerWidth;

                // vertical tick
                Line line = new Line() { Y2 = 12, X1 = right, X2 = right, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
                container.Children.Add(line);

                if (showLabels == false)
                    break; // cancel current loop here when no labels wanted

                // TODO: sauber machen
                Label label = new Label() { Content = Math.Round(layers[i].LayerThickness, 1).ToString(), FontSize = 10, VerticalContentAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Center };
                label.Measure(new Size(56, 40)); //https://stackoverflow.com/questions/2928498/label-size-is-always-nan
                double labelRightMargin = prevLayer + layerWidth / 2 - label.DesiredSize.Width / 2; //space right of the label
                double labelLeftMargin = fullWidth_px - label.DesiredSize.Width - labelRightMargin; // space left of the label
                label.Margin = new Thickness(labelLeftMargin, 0, labelRightMargin, 0);
                container.Children.Add(label);
                prevLayer += layerWidth;
            }

            // last vertical tick
            Line lineEnd = new Line() { Y2 = 12, X1 = right + strokeWidth / 2, X2 = right + strokeWidth / 2, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = strokeWidth, VerticalAlignment = VerticalAlignment.Center };
            container.Children.Add(lineEnd);
        }
    }
}
