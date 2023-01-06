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
using LiveChartsCore.Defaults;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.EnvironmentData;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Reflection.Emit;
using System.Windows.Shapes;
using System.Windows.Ink;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO2_Temperature.cs: Used in xaml as "DataContext"
    [ObservableObject]
    public partial class LiveChartsViewModel
    {
        public StationaryTempCurve TempCurveCalc { get; set; }
        public ISeries[] TempCurve { get; set; }
        public RectangularSection[] LayerSections { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint TooltipTextPaint { get; set; }
        public SolidColorPaint TooltipBackgroundPaint { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; }
        public SolidColorPaint LegendBackgroundPaint { get; set; }

        public LiveChartsViewModel() // Called by 'InitializeComponent()' from FO2_Calculate.cs due to Class-Binding in xaml via DataContext
        {
            this.TempCurveCalc = new StationaryTempCurve();
            this.LayerSections = DrawLayerSections();
            this.TempCurve = DrawTempCurvePoints();
            this.XAxes = DrawXAxes();
            this.YAxes = DrawYAxes();
            this.TooltipTextPaint = new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0),
                SKTypeface = SKTypeface.FromFamilyName("Arial"),
            };
            this.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(255, 255, 255));

            /*this.LegendBackgroundPaint = new SolidColorPaint(new SKColor(240, 240, 240));

            this.LegendTextPaint = new SolidColorPaint
            {
                Color = new SKColor(50, 50, 50),
                SKTypeface = SKTypeface.FromFamilyName("Arial")
            };*/
        }
        private RectangularSection[] DrawLayerSections()
        {
            RectangularSection[] rects = new RectangularSection[TempCurveCalc.Layers.Count];

            double fullWidth = TempCurveCalc.TotalElementWidth;
            double right = fullWidth;

            foreach (Layer layer in TempCurveCalc.Layers)
            {
                int position = layer.LayerPosition - 1; // change to 0 based index
                double layerWidth = layer.LayerThickness;
                double left = right - layerWidth; // start drawing from right side (beginning with INSIDE Layer, which is first list element)

                // Set properties of the layer rectangle at the desired position
                rects[position] = new RectangularSection
                {
                    Xi = left,
                    Xj = right,
                    Fill = new SolidColorPaint(SKColor.Parse(layer.Material.ColorCode)),
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Black,
                        StrokeThickness = 1,
                        //PathEffect = new DashEffect(new float[] { 6, 6 })
                    },
                    ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
                };
                right -= layerWidth; // Add new layer at left edge of previous layer
            }

            //TODO: is hardcoded
            /*fRsi frsi = new fRsi(TempCurveCalc.LayerTemps.First().Value, Temperatures.selectedTi.First().Value, Temperatures.selectedTe.First().Value);
            rects[5] = new RectangularSection
            {
                Yi = fRsi.TsiMin,
                Yj = fRsi.TsiMin,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.Red,
                    StrokeThickness = 1,
                    PathEffect = new DashEffect(new float[] { 2, 2 })
                }
            };*/

            return rects;
        }
        private ISeries[] DrawTempCurvePoints()
        {
            ISeries[] series = new ISeries[1]; // more than one series possible to draw in the same graph      

            ObservablePoint[] tempValues = new ObservablePoint[TempCurveCalc.LayerTemps.Count()]; // represents the temperature points

            for (int i = 0; i < TempCurveCalc.LayerTemps.Count(); i++)
            {
                double x = TempCurveCalc.LayerTemps.Keys.ElementAt(i); // Position in cm
                double y = Math.Round(TempCurveCalc.LayerTemps.Values.ElementAt(i), 2); // Temperature in °C
                tempValues[i] = new ObservablePoint(x, y);
            }

            // Set properties & add temperature points to the series
            LineSeries<ObservablePoint> tempCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = tempValues,
                Fill = null,               
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                //properties of the connecting dots  
                GeometryFill = new SolidColorPaint(SKColors.Red),
                GeometryStroke = new SolidColorPaint(SKColors.Red),
                GeometrySize = 6,
                //Stroke = new LinearGradientPaint(new[] { new SKColor(), new SKColor(255, 212, 96) }) { StrokeThickness = 3 },
                //GeometryStroke = new LinearGradientPaint(new[] { new SKColor(45, 64, 89), new SKColor(255, 212, 96) }) { StrokeThickness = 3 }
                TooltipLabelFormatter = (chartPoint) => $"Temperature: {chartPoint.PrimaryValue} °C",
                //When multiple axes:
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            series[0] = tempCurveSeries;

            return series;
        }
        private Axis[] DrawXAxes()
        {
            Axis[] axes = new Axis[1];
            
            axes[0] = new Axis
            {
                Name = "Element thickness [cm]",
                NameTextSize = 16,
                NamePaint = new SolidColorPaint(SKColors.Black),
                //Labels = new string[] { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5" },
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 14,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
            };
            /*axes[1] = new Axis
            {
                Name = "Layer Nr.",
                NameTextSize = 16,
                NamePaint = new SolidColorPaint(SKColors.Black),
                Labels = new string[] { "1", "2", "3", "4" },
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 14
            };*/
            return axes;
        }
        private Axis[] DrawYAxes()
        {
            Axis[] axes = new Axis[1];

            axes[0] = new Axis
            {
                Name = "Temperature curve [°C]",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 14,
                NameTextSize = 16,
                Position = LiveChartsCore.Measure.AxisPosition.Start,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect(new float[] { 3, 3 })
                }
            };
            return axes;
            /*
            axes[1] = new Axis
            {
                Name = "test",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 14,
                NameTextSize = 16,
                Position = LiveChartsCore.Measure.AxisPosition.End,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect(new float[] { 3, 3 })
                },
                ShowSeparatorLines = true
            };*/
        }
    }
}
