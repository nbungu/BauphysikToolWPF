﻿using System;
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
    public enum Axis
    {
        X,
        Y,
        Z
    }
    public static class MeasurementChain
    {
        #region Public Methods

        public static DrawingGeometry GetMeasurementChain(IEnumerable<Layer> layers, Axis intervalDirection = Axis.Z)
        {
            var intervals = GetGeometryIntervals(layers.Select(l => l.Convert()), intervalDirection);

            return GetMeasurementDrawing(intervals, intervalDirection);
        }

        public static DrawingGeometry GetMeasurementChain(IEnumerable<IDrawingGeometry> geometries, Axis intervalDirection = Axis.Z)
        {
            var intervals = GetGeometryIntervals(geometries, intervalDirection);

            return GetMeasurementDrawing(intervals, intervalDirection);
        }

        public static DrawingGeometry GetMeasurementChain(double[] intervals, Axis intervalDirection = Axis.Z)
        {
            return GetMeasurementDrawing(intervals, intervalDirection);
        }

        #endregion

        #region private Methods
        
        // Top Left is Origin (0,0)
        private static DrawingGeometry GetMeasurementDrawing(double[] intervals, Axis intervalDirection)
        {
            DrawingGeometry geometry = new DrawingGeometry();
            intervals = FixIntervals(intervals);
            if (intervals.Length == 0) return geometry;

            // Create a GeometryGroup to contain the lines
            var hatchContent = new GeometryGroup();

            LineGeometry baseline;
            if (intervalDirection == Axis.Z)
            {
                baseline = new LineGeometry() { StartPoint = new Point(0, intervals.FirstOrDefault(0.0)), EndPoint = new Point(0, intervals.Last()) };
            }
            else
            {
                baseline = new LineGeometry() { StartPoint = new Point(intervals.FirstOrDefault(0.0), 0), EndPoint = new Point(intervals.Last(), 0) };
            }
            hatchContent.Children.Add(baseline);

            var selectedInterval = intervals[0];
            for (int i = 0; i < intervals.Length; i++)
            {
                if (i > 0)
                {
                    var intervalWidthInCm = Math.Round(Math.Abs(intervals[i] - selectedInterval) / CanvasDrawingService.SizeOf1Cm, 2);
                    if (intervalWidthInCm > 0)
                    {
                        var label = new FormattedText(intervalWidthInCm.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 14, Brushes.DimGray, 1.0);

                        // Create the text drawing
                        var labelOrigin = (selectedInterval + intervals[i]) / 2;
                        System.Windows.Media.Geometry textGeometry;
                        if (intervalDirection == Axis.Z)
                        {
                            textGeometry = label.BuildGeometry(new Point(8, labelOrigin - label.Height / 2));
                        }
                        else
                        {
                            textGeometry = label.BuildGeometry(new Point(labelOrigin - label.Width / 2, 8));
                        }
                        hatchContent.Children.Add(textGeometry);
                    }
                }

                // Create Interval Tick Markers
                if (intervalDirection == Axis.Z)
                {
                    var lineTickHorizontal = new LineGeometry() { StartPoint = new Point(-16, intervals[i]), EndPoint = new Point(6, intervals[i]) };
                    var lineTick45 = new LineGeometry() { StartPoint = new Point(4, intervals[i] - 4), EndPoint = new Point(-4, intervals[i] + 4) };
                    hatchContent.Children.Add(lineTickHorizontal);
                    hatchContent.Children.Add(lineTick45);
                }
                else
                {
                    var lineTickVertical = new LineGeometry() { StartPoint = new Point(intervals[i], -16), EndPoint = new Point(intervals[i], 6) };
                    var lineTick45 = new LineGeometry() { StartPoint = new Point(intervals[i] - 4, 4), EndPoint = new Point(intervals[i] + 4, -4) };
                    hatchContent.Children.Add(lineTickVertical);
                    hatchContent.Children.Add(lineTick45);
                }
                // update last used interval
                selectedInterval = intervals[i];
            }

            // Adjust Rectangle
            geometry.Rectangle = new Rectangle(hatchContent.Bounds.Left, hatchContent.Bounds.Top, hatchContent.Bounds.Width, hatchContent.Bounds.Height);

            // Use the lines as the Drawing's content
            var brush = new DrawingBrush
            {
                Drawing = new GeometryDrawing(Brushes.DimGray, new Pen(Brushes.DimGray, 0.6), hatchContent),
            };
            geometry.DrawingBrush = brush;

            return geometry;
        }

        private static double[] GetGeometryIntervals(IEnumerable<IDrawingGeometry> drawingGeometries, Axis direction = Axis.Z)
        {
            double[] intervals = Array.Empty<double>();
            if (direction == Axis.X)
            {
                intervals = drawingGeometries.SelectMany(e => new[] { e.Rectangle.Left, e.Rectangle.Right }).ToArray();
            }
            else if (direction == Axis.Z)
            {
                intervals = drawingGeometries.SelectMany(e => new[] { e.Rectangle.Top, e.Rectangle.Bottom }).ToArray();
            }

            return FixIntervals(intervals);
        }

        private static double[] FixIntervals(double[] intervals)
        {
            if (intervals is null || intervals.Length <= 1) return Array.Empty<double>();

            // Sort Ascending and Distinct
            return intervals.OrderBy(x => x).Distinct().ToArray();
        }

        #endregion
    }
}
