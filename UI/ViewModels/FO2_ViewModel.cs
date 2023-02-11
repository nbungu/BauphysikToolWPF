using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO2_Temperature.cs: Used in xaml as "DataContext"
    [ObservableObject]
    public partial class FO2_ViewModel
    {
        public string Title { get; } = "Temperature";
        public StationaryTempCalc TempCalc { get; private set; } = FO2_Temperature.StationaryTempCalculation;
        public double Ti { get; private set; } = UserSaved.Ti;
        public double Te { get; private set; } = UserSaved.Te;
        public double Rel_Fi { get; private set; } = UserSaved.Rel_Fi;
        public CheckRequirements CheckRequirements { get; private set; }
        public bool IsUValueOK { get; private set; }
        public bool IsRValueOK { get; private set; }
        public string U_max { get; private set; }
        public string R_min { get; private set; }

        public RectangularSection[] LayerSections { get; private set; }
        public ISeries[] DataPoints { get; private set; }
        public Axis[] XAxes { get; private set; }
        public Axis[] YAxes { get; private set; }
        public SolidColorPaint TooltipBackgroundPaint { get; private set; }
        public SolidColorPaint TooltipTextPaint { get; private set; }

        public FO2_ViewModel() // Called by 'InitializeComponent()' from FO2_Calculate.cs due to Class-Binding in xaml via DataContext
        {
            // For the Requirement Checks (U-Value, R-Value)
            this.CheckRequirements = new CheckRequirements(TempCalc.UValue, TempCalc.SumOfLayersR);
            this.U_max = CheckRequirements.U_max > 0 ? CheckRequirements.U_max.ToString() : "Keine Anforderung";
            this.IsUValueOK = CheckRequirements.U_max > 0 ? CheckRequirements.IsUValueOK : true;
            this.R_min = CheckRequirements.R_min > 0 ? CheckRequirements.R_min.ToString() : "Keine Anforderung";
            this.IsRValueOK = CheckRequirements.R_min > 0 ? CheckRequirements.IsRValueOK : true;

            // For Drawing the Chart
            this.LayerSections = DrawLayerSections();
            this.DataPoints = DrawTempCurvePoints();
            this.XAxes = DrawXAxes();
            this.YAxes = DrawYAxes();
            this.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(255, 255, 255));
            this.TooltipTextPaint = new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0),
                SKTypeface = SKTypeface.FromFamilyName("SegoeUI"),
            };
        }
        private RectangularSection[] DrawLayerSections()
        {
            if (TempCalc.Element.Layers.Count == 0)
                return new RectangularSection[0];

            RectangularSection[] rects = new RectangularSection[TempCalc.Element.Layers.Count];

            double left = 0;
            foreach (Layer layer in TempCalc.Element.Layers)
            {
                int position = layer.LayerPosition - 1; // change to 0 based index
                double layerWidth = layer.LayerThickness;
                double right = left + layerWidth; // start drawing from left side (beginning with INSIDE Layer, which is first list element)

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
                left = right; // Add new layer at left edge of previous layer
            }

            //TODO: is hardcoded
            //fRsi frsi = new fRsi(TempCalc.LayerTemps.First().Value, Temperatures.selectedTi.First().Value, Temperatures.selectedTe.First().Value);
            /*rects[0] = new RectangularSection
            {
                Yi = StationaryTempCalc.TsiMin,
                Yj = StationaryTempCalc.TsiMin,
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
            if (TempCalc.Element.Layers.Count == 0)
                return new ISeries[0];

            ISeries[] series = new ISeries[3]; // more than one series possible to draw in the same graph      

            double tsi_Pos = TempCalc.LayerTemps.First().Key;
            double tsi = TempCalc.LayerTemps.First().Value;
            double deltaTi = Math.Abs(Ti - tsi);

            LineSeries<ObservablePoint> rsiCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tsi_Pos-0.8, tsi+0.9*deltaTi),
                    null, // cuts the line between the points
                    new ObservablePoint(tsi_Pos, tsi),
                    new ObservablePoint(tsi_Pos-0.8, tsi+0.9*deltaTi),
                    new ObservablePoint(tsi_Pos-2, Ti)
                },
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                TooltipLabelFormatter = null
            };

            ObservablePoint[] tempValues = new ObservablePoint[TempCalc.LayerTemps.Count()]; // represents the temperature points
            for (int i = 0; i < TempCalc.LayerTemps.Count(); i++)
            {
                double x = TempCalc.LayerTemps.ElementAt(i).Key; // Position in cm
                double y = Math.Round(TempCalc.LayerTemps.ElementAt(i).Value, 2); // Temperature in °C
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

            double tse_Pos = TempCalc.LayerTemps.Last().Key;
            double tse = TempCalc.LayerTemps.Last().Value;
            double deltaTe = Math.Abs(Te - tse);
            LineSeries<ObservablePoint> rseCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tse_Pos+2, Te),
                    new ObservablePoint(tse_Pos+0.8, tse-0.9*deltaTe),
                    new ObservablePoint(tse_Pos, tse),
                    null, // cuts the line between the points
                    new ObservablePoint(tse_Pos+0.8, tse+0.9*deltaTe),
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
