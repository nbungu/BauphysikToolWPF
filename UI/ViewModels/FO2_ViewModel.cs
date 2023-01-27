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
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Reflection.Emit;
using System.Windows.Shapes;
using System.Windows.Ink;
using LiveChartsCore.Measure;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO2_Temperature.cs: Used in xaml as "DataContext"
    [ObservableObject]
    public partial class FO2_ViewModel
    {
        public string Title { get; } = "Temperature";
        public StationaryTempCalc StationaryTempCalculation { get; set; }
        public RectangularSection[] LayerSections { get; set; }
        public ISeries[] DataPoints { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint TooltipTextPaint { get; set; }
        public SolidColorPaint TooltipBackgroundPaint { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; }
        public SolidColorPaint LegendBackgroundPaint { get; set; }
        public double Ti { get; set; } = 0;
        public double Te { get; set; } = 0;
        public double Rel_Fi { get; set; } = 0;

        public FO2_ViewModel() // Called by 'InitializeComponent()' from FO2_Calculate.cs due to Class-Binding in xaml via DataContext
        {
            this.StationaryTempCalculation = FO2_Temperature.StationaryTempCalculation;
            this.Rel_Fi = StationaryTempCalculation.Rel_Fi;
            this.Ti = StationaryTempCalculation.Ti;
            this.Te = StationaryTempCalculation.Te;
            this.LayerSections = DrawLayerSections();
            this.DataPoints = DrawTempCurvePoints();
            this.XAxes = DrawXAxes();
            this.YAxes = DrawYAxes();
            this.TooltipTextPaint = new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0),
                SKTypeface = SKTypeface.FromFamilyName("SegoeUI"),
            };
            this.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(255, 255, 255));
        }
        private RectangularSection[] DrawLayerSections()
        {
            if (StationaryTempCalculation.Layers.Count == 0)
                return new RectangularSection[0];

            RectangularSection[] rects = new RectangularSection[StationaryTempCalculation.Layers.Count];

            double fullWidth = StationaryTempCalculation.TotalElementWidth;
            double right = fullWidth;

            foreach (Layer layer in StationaryTempCalculation.Layers)
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
            /*fRsi frsi = new fRsi(StationaryTempCalculation.LayerTemps.First().Value, Temperatures.selectedTi.First().Value, Temperatures.selectedTe.First().Value);
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
            if (StationaryTempCalculation.Layers.Count == 0)
                return new ISeries[0];

            ISeries[] series = new ISeries[3]; // more than one series possible to draw in the same graph      

            double tsi_Pos = StationaryTempCalculation.LayerTemps.First().Key;
            double tsi = StationaryTempCalculation.LayerTemps.First().Value;
            double deltaTi = Math.Abs(Ti - tsi);
            LineSeries<ObservablePoint> rsiCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tsi_Pos+0.8, tsi-0.9*deltaTi),
                    null, // cuts the line between the points
                    new ObservablePoint(tsi_Pos, tsi),
                    new ObservablePoint(tsi_Pos+0.8, tsi+0.9*deltaTi),
                    new ObservablePoint(tsi_Pos+2, Ti)
                },
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                TooltipLabelFormatter = null
            };
            
            ObservablePoint[] tempValues = new ObservablePoint[StationaryTempCalculation.LayerTemps.Count()]; // represents the temperature points
            for (int i = 0; i < StationaryTempCalculation.LayerTemps.Count(); i++)
            {
                double x = StationaryTempCalculation.LayerTemps.ElementAt(i).Key; // Position in cm
                double y = Math.Round(StationaryTempCalculation.LayerTemps.ElementAt(i).Value, 2); // Temperature in °C
                tempValues[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
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

            double tse_Pos = StationaryTempCalculation.LayerTemps.Last().Key;
            double tse = StationaryTempCalculation.LayerTemps.Last().Value;
            double deltaTe = Math.Abs(Te - tse);
            LineSeries<ObservablePoint> rseCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {   
                    new ObservablePoint(tse_Pos-2, Te),
                    new ObservablePoint(tse_Pos-0.8, tse-0.9*deltaTe),
                    new ObservablePoint(tse_Pos, tse),
                    null, // cuts the line between the points
                    new ObservablePoint(tse_Pos-0.8, tse+0.9*deltaTe),

                },
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                TooltipLabelFormatter = null
            };
            series[0] = rsiCurveSeries;
            series[1] = tempCurveSeries;
            series[2] = rseCurveSeries;

            return series;
        }
        private Axis[] DrawXAxes()
        {
            Axis[] axes = new Axis[1];
            
            axes[0] = new Axis
            {
                Name = "Element thickness [cm]",
                NameTextSize = 14,
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
                NameTextSize = 14,
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
