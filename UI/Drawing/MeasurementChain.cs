using BauphysikToolWPF.Models;
using Geometry;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace BauphysikToolWPF.UI.Drawing
{
    public static class MeasurementChain
    {
        #region Public Methods

        public static DrawingGeometry GetLayerMeasurement(IEnumerable<Layer> layers, Axis intervalDirection = Axis.Z)
        {
            double[] intervals = layers.Select(l => l.Rectangle.Bottom).ToArray();
            string[] labels = layers.Select(l => l.Thickness.ToString(CultureInfo.CurrentCulture)).ToArray();

            if (intervalDirection == Axis.Z) return GetMeasurementDrawingVertical(intervals, labels);
            return GetMeasurementDrawingHorizontal(intervals, labels);
        }

        //public static DrawingGeometry GetLayerMeasurement(IEnumerable<IDrawingGeometry> geometries, Axis intervalDirection = Axis.Z)
        //{
        //    double[] intervals = geometries.Select(l => l.Rectangle.Bottom).ToArray();

        //    if (intervalDirection == Axis.Z) return GetMeasurementDrawingVertical(intervals, labels);
        //    return GetMeasurementDrawingHorizontal(intervals, labels);
        //}

        public static DrawingGeometry GetMeasurement(double[] intervals, string[] labels, Axis intervalDirection = Axis.Z)
        {
            if (intervalDirection == Axis.Z) return GetMeasurementDrawingVertical(intervals, labels);
            return GetMeasurementDrawingHorizontal(intervals, labels);
        }
        
        #endregion

        #region private Methods

        // Vertically!
        // Top Left is Origin (0,0)
        private static DrawingGeometry GetMeasurementDrawingVertical(double[] intervals, string[] labels)
        {
            DrawingGeometry geometry = new DrawingGeometry();
            intervals = FixIntervals(intervals);
            
            // Create a GeometryGroup to contain the lines
            var hatchContent = new GeometryGroup();

            var baseline = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(0, intervals.Last()) };
            hatchContent.Children.Add(baseline);

            // Create Starting Tick Marker at 0
            var selectedInterval = 0.0;
            var firstLineTickHorizontal = new LineGeometry() { StartPoint = new Point(-12, selectedInterval), EndPoint = new Point(6, selectedInterval) };
            var firstLineTick45 = new LineGeometry() { StartPoint = new Point(4, selectedInterval - 4), EndPoint = new Point(-4, selectedInterval + 4) };
            hatchContent.Children.Add(firstLineTickHorizontal);
            hatchContent.Children.Add(firstLineTick45);
            
            for (int i = 0; i < intervals.Length; i++)
            {
                // Create the text drawing
                var labelOrigin = (selectedInterval + intervals[i]) / 2;
                var formattedText = new FormattedText(labels[i], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.DimGray, 1.0);
                var textGeometry = formattedText.BuildGeometry(new Point(8, labelOrigin - formattedText.Height / 2));
                hatchContent.Children.Add(textGeometry);

                // Create Interval Tick Markers
                var lineTickHorizontal = new LineGeometry() { StartPoint = new Point(-12, intervals[i]), EndPoint = new Point(6, intervals[i]) };
                var lineTick45 = new LineGeometry() { StartPoint = new Point(4, intervals[i] - 4), EndPoint = new Point(-4, intervals[i] + 4) };
                hatchContent.Children.Add(lineTickHorizontal);
                hatchContent.Children.Add(lineTick45);
                // update last used interval
                selectedInterval = intervals[i];
            }

            //Adjust Rectangle
            geometry.Rectangle = new Rectangle(hatchContent.Bounds.Left, hatchContent.Bounds.Top, hatchContent.Bounds.Width, hatchContent.Bounds.Height).MoveTo(new Geometry.Point(0,-4));

            // Use the lines as the Drawing's content
            var brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(Brushes.DimGray, new Pen(Brushes.DimGray, 0.6), hatchContent),
            };
            geometry.DrawingBrush = brush;

            return geometry;
        }
        // Horizontally!
        // Top Left is Origin (0,0)
        private static DrawingGeometry GetMeasurementDrawingHorizontal(double[] intervals, string[] labels)
        {
            DrawingGeometry geometry = new DrawingGeometry();
            intervals = FixIntervals(intervals);

            // Create a GeometryGroup to contain the lines
            var hatchContent = new GeometryGroup();

            var baseline = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(intervals.Last(), 0) };
            hatchContent.Children.Add(baseline);

            // Create Starting Tick Marker at 0
            var selectedInterval = 0.0;
            var firstLineTickVertical = new LineGeometry() { StartPoint = new Point(selectedInterval, -12), EndPoint = new Point(selectedInterval, 6) };
            var firstLineTick45 = new LineGeometry() { StartPoint = new Point(selectedInterval - 4, 4), EndPoint = new Point(selectedInterval + 4, -4) };
            hatchContent.Children.Add(firstLineTickVertical);
            hatchContent.Children.Add(firstLineTick45);

            for (int i = 0; i < intervals.Length; i++)
            {
                // Create the text drawing
                var labelOrigin = (selectedInterval + intervals[i]) / 2;
                var formattedText = new FormattedText(labels[i], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.DimGray, 1.0);
                var textGeometry = formattedText.BuildGeometry(new Point(labelOrigin - formattedText.Height / 2, 8));
                hatchContent.Children.Add(textGeometry);

                // Create Interval Tick Markers
                var lineTickVertical = new LineGeometry() { StartPoint = new Point(intervals[i], -12), EndPoint = new Point(intervals[i], 6) };
                var lineTick45 = new LineGeometry() { StartPoint = new Point(intervals[i] - 4, 4), EndPoint = new Point(intervals[i] + 4, -4) };
                hatchContent.Children.Add(lineTickVertical);
                hatchContent.Children.Add(lineTick45);
                // update last used interval
                selectedInterval = intervals[i];
            }

            //Adjust Rectangle
            geometry.Rectangle = new Rectangle(hatchContent.Bounds.Left, hatchContent.Bounds.Top, hatchContent.Bounds.Width, hatchContent.Bounds.Height).MoveTo(new Geometry.Point(-4, 0));

            // Use the lines as the Drawing's content
            var brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(Brushes.DimGray, new Pen(Brushes.DimGray, 0.6), hatchContent),
            };
            geometry.DrawingBrush = brush;

            return geometry;
        }
        private static double[] FixIntervals(double[] intervals)
        {
            // Check if the first entry is zero
            if (intervals.Length > 0 && intervals[0] == 0)
            {
                // Use LINQ to skip the first element and create a new array
                return intervals.Skip(1).Distinct().ToArray();
            }
            // If the first entry is not zero, return the original array
            return intervals.Distinct().ToArray();
        }

        #endregion
    }
}
