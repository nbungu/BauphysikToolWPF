using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace BauphysikToolWPF.UI.Helper
{
    public class MeasurementChain : DrawingGeometry
    {
        // All in [px]
        // Origin is TopLeft (0,0)
        
        private readonly double[] _intervals = Array.Empty<double>();

        public MeasurementChain() {}

        public MeasurementChain(List<Layer> layers)
        {
            if (layers.Count == 0) return;
            _intervals = GetIntervals(layers);
            DrawingBrush = GetMeasurementDrawing();
        }

        public MeasurementChain(double[] intervals)
        {
            if (intervals.Length <= 1) return;
            _intervals = intervals;
            DrawingBrush = GetMeasurementDrawing();
        }

        private double[] GetIntervals(List<Layer> layers)
        {
            var startInterval = layers[0].Rectangle.Left;
            var intervals = layers.Select(l => l.Rectangle.Right).ToList();
            intervals.Insert(0, startInterval);
            return intervals.Distinct().ToArray();
        }

        private DrawingBrush GetMeasurementDrawing()
        {
            // Create a GeometryGroup to contain the lines
            var hatchContent = new GeometryGroup();

            var baseline = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(_intervals.Last(), 0) };
            hatchContent.Children.Add(baseline);

            for (int i = 0; i < _intervals.Length; i++)
            {
                var lineTickVertical = new LineGeometry() { StartPoint = new Point(_intervals[i], -4), EndPoint = new Point(_intervals[i], 4) };
                var lineTick45 = new LineGeometry() { StartPoint = new Point(_intervals[i] - 4, 4), EndPoint = new Point(_intervals[i] + 4, -4) };
                hatchContent.Children.Add(lineTickVertical);
                hatchContent.Children.Add(lineTick45);
            }

            //Adjust Rectangle
            Rectangle = new Rectangle(hatchContent.Bounds.Left, hatchContent.Bounds.Top, hatchContent.Bounds.Width, hatchContent.Bounds.Height).MoveTo(new Geometry.Point(-4,0));

            // Use the lines as the Drawing's content
            var brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(Brushes.DimGray, 0.8), hatchContent),
            };
            return brush;
        }
    }
}
