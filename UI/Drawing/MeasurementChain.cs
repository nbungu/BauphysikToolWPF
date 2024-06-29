using BauphysikToolWPF.Models;
using Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace BauphysikToolWPF.UI.Drawing
{
    public class MeasurementChain : IDrawingGeometry
    {
        // All in [px]
        // Origin is TopLeft (0,0)
        private double[] _intervals = Array.Empty<double>();
        private double[] _labels = Array.Empty<double>();
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        public Brush RectangleBorderColor { get; set; } = Brushes.Transparent;
        public double RectangleBorderThickness { get; set; } = 0;
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        public double Opacity { get; set; } = 1;
        public int ZIndex { get; set; }
        public object Tag { get; set; }

        public MeasurementChain() {}

        public MeasurementChain(List<Layer> layers)
        {
            if (layers.Count == 0) return;
            _intervals = layers.Select(l => l.Rectangle.Bottom).Distinct().ToArray();
            _labels = layers.Select(l => l.Thickness).ToArray();
            DrawingBrush = GetMeasurementDrawing();
        }

        public MeasurementChain(double[] intervals, double[] labels)
        {
            if (intervals.Length < 1) return;
            _intervals = intervals;
            _labels = labels;
            DrawingBrush = GetMeasurementDrawing();
        }

        // Vertically!
        // Top Left is Origin (0,0)
        private DrawingBrush GetMeasurementDrawing()
        {
            // Create a GeometryGroup to contain the lines
            var hatchContent = new GeometryGroup();

            var baseline = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(0, _intervals.Last()) };
            hatchContent.Children.Add(baseline);

            // Create Starting Tick Marker at 0
            var selectedInterval = 0.0;
            var firstLineTickHorizontal = new LineGeometry() { StartPoint = new Point(-12, selectedInterval), EndPoint = new Point(6, selectedInterval) };
            var firstLineTick45 = new LineGeometry() { StartPoint = new Point(4, selectedInterval - 4), EndPoint = new Point(-4, selectedInterval + 4) };
            hatchContent.Children.Add(firstLineTickHorizontal);
            hatchContent.Children.Add(firstLineTick45);
            
            for (int i = 0; i < _intervals.Length; i++)
            {
                // Create the text drawing
                var labelOrigin = (selectedInterval + _intervals[i]) / 2;
                var formattedText = new FormattedText(_labels[i].ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.DimGray, 1.0);
                var textGeometry = formattedText.BuildGeometry(new Point(8, labelOrigin - formattedText.Height / 2));
                hatchContent.Children.Add(textGeometry);

                // Create Interval Tick Markers
                var lineTickHorizontal = new LineGeometry() { StartPoint = new Point(-12, _intervals[i]), EndPoint = new Point(6, _intervals[i]) };
                var lineTick45 = new LineGeometry() { StartPoint = new Point(4, _intervals[i] - 4), EndPoint = new Point(-4, _intervals[i] + 4) };
                hatchContent.Children.Add(lineTickHorizontal);
                hatchContent.Children.Add(lineTick45);
                // update last used interval
                selectedInterval = _intervals[i];
            }

            //Adjust Rectangle
            Rectangle = new Rectangle(hatchContent.Bounds.Left, hatchContent.Bounds.Top, hatchContent.Bounds.Width, hatchContent.Bounds.Height).MoveTo(new Geometry.Point(0,-4));

            // Use the lines as the Drawing's content
            var brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(Brushes.DimGray, new Pen(Brushes.DimGray, 0.6), hatchContent),
            };
            return brush;
        }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }
        public void UpdateGeometry()
        {
            throw new NotImplementedException();
        }

        // For use as single collection Type in XAML Items Source of Canvas
        public List<MeasurementChain> ToList()
        {
            return new List<MeasurementChain>() { this };
        }
    }
}
