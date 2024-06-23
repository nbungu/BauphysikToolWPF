using BauphysikToolWPF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BauphysikToolWPF.Models.Helper;
using Geometry;
using Point = System.Windows.Point;

namespace BauphysikToolWPF.UI.Helper
{
    public class MeasurementChain : DrawingGeometry
    {
        private readonly double[] _intervals;

        public MeasurementChain(List<Layer> layers)
        {
            var startInterval = layers[0].Rectangle.Left;
            var intervals = layers.Select(l => l.Rectangle.Right).ToList();
            intervals.Insert(0, startInterval);

            //TODO: sort ascending
            _intervals = intervals.Distinct().ToArray();
            DrawingBrush = GetMeasurementDrawing();
        }

        public MeasurementChain(double[] intervals)
        {
            //TODO: sort ascending
            _intervals = intervals.Distinct().ToArray();
            DrawingBrush = GetMeasurementDrawing();
        }

        private DrawingBrush GetMeasurementDrawing()
        {
            // Create a GeometryGroup to contain the lines
            var hatchContent = new GeometryGroup();

            var baseline = new LineGeometry() { StartPoint = new Point(0, 0), EndPoint = new Point(_intervals.Last(), 0) };
            hatchContent.Children.Add(baseline);

            for (int i = 0; i < _intervals.Length; i++)
            {
                var lineTick = new LineGeometry() { StartPoint = new Point(_intervals[i], 0), EndPoint = new Point(_intervals[i], 16) };
                hatchContent.Children.Add(lineTick);
            }

            //Adjust Rectangle
            Rectangle = new Rectangle(hatchContent.Bounds.Top, hatchContent.Bounds.Left, hatchContent.Bounds.Width, hatchContent.Bounds.Height);

            // Use the lines as the Drawing's content
            var brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(new SolidColorBrush(), new Pen(StrokeColor, StrokeThickness), hatchContent),
            };
            return brush;
        }
    }
}
