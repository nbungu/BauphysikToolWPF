using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting.Effects;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for LiveCharts: Used in xaml as "DataContext"
    [ObservableObject]
    public partial class LiveChartsViewModel
    {
        public ISeries[] TempCurve { get; set; }
        public RectangularSection[] Layers { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint TooltipTextPaint { get; set; }
        public SolidColorPaint TooltipBackgroundPaint { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; }
        public SolidColorPaint LegendBackgroundPaint { get; set; }

        public LiveChartsViewModel()
        {
            this.Layers = new RectangularSection[]
            {
                new RectangularSection
                {
                    Yi = 5, //TODO: Taupunkttemp
                    Yj = 5,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Red,
                        StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 2, 2 })
                    }
                },
                new RectangularSection
                {
                    Xi = 1,
                    Xj = 3,
                    Fill = new SolidColorPaint { Color = SKColors.Blue.WithAlpha(20) }
                }
            };
            this.TempCurve = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = new int[] { 4, 6, 5, 3, -3 },
                    Fill = null,
                    GeometrySize = 3, // size of the connecting dots                   
                    LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                    Stroke = new LinearGradientPaint(new[]{ new SKColor(45, 64, 89), new SKColor(255, 212, 96)}) { StrokeThickness = 3 },
                    GeometryStroke = new LinearGradientPaint(new[]{ new SKColor(45, 64, 89), new SKColor(255, 212, 96)}) { StrokeThickness = 3 }
                }
            };
            this.XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "X Axis",
                    //Labels = new string[] { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5" },
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
                }
            };
            this.YAxes = new Axis[]
{
                new Axis
                {
                    Name = "Y Axis",
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    }
                }
            };
            this.TooltipTextPaint = new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0),
                SKTypeface = SKTypeface.FromFamilyName("Courier New")
            };
            this.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(255, 255, 255));

            this.LegendBackgroundPaint = new SolidColorPaint(new SKColor(240, 240, 240));

            this.LegendTextPaint = new SolidColorPaint
            {
                Color = new SKColor(50, 50, 50),
                SKTypeface = SKTypeface.FromFamilyName("Courier New")
            };
        }
    }
}
