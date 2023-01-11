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
using LiveChartsCore.Measure;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauphysikToolWPF.UI.ViewModels
{
    [ObservableObject]
    public partial class FO3_ViewModel
    {
        public string Title { get; } = "Moisture"; // Called by 'InitializeComponent()' due to Class-Binding in xaml via DataContext        
        public GlaserCalc GlaserCalculation { get; set; }
        public RectangularSection[] LayerSections { get; set; }
        public ISeries[] DataPoints { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint TooltipTextPaint { get; set; }
        public SolidColorPaint TooltipBackgroundPaint { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; }
        public SolidColorPaint LegendBackgroundPaint { get; set; }
        public FO3_ViewModel() // Called by 'InitializeComponent()' from FO3_Moisture.cs due to Class-Binding in xaml via DataContext
        {
            this.GlaserCalculation = FO3_Moisture.GlaserCalculation;
            this.LayerSections = DrawLayerSections();
            this.DataPoints = DrawGlaserCurvePoints();
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
            if (GlaserCalculation.Layers.Count == 0)
                return new RectangularSection[0];

            RectangularSection[] rects = new RectangularSection[GlaserCalculation.Layers.Count];

            double fullWidth = GlaserCalculation.TotalSdWidth;
            double right = fullWidth;

            //TODO: Round values of left and right
            foreach (Layer layer in GlaserCalculation.Layers)
            {
                int position = layer.LayerPosition - 1; // change to 0 based index
                double layerWidth = layer.Sd_Thickness;
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
            return rects;
        }
        private ISeries[] DrawGlaserCurvePoints()
        {
            if (GlaserCalculation.Layers.Count == 0)
                return new ISeries[0];

            ISeries[] series = new ISeries[2];     

            ObservablePoint[] p_Curve_Values = new ObservablePoint[GlaserCalculation.LayerP.Count()]; // represents the temperature points
            for (int i = 0; i < GlaserCalculation.LayerP.Count(); i++)
            {
                double x = GlaserCalculation.LayerP.ElementAt(i).Key; // Position in cm
                double y = Math.Round(GlaserCalculation.LayerP.ElementAt(i).Value, 2); // Temperature in °C
                p_Curve_Values[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> p_Curve = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = p_Curve_Values,
                Fill = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Blue, 2), 
                GeometryFill = new SolidColorPaint(SKColors.Blue),
                GeometryStroke = new SolidColorPaint(SKColors.Blue),
                GeometrySize = 6,
                TooltipLabelFormatter = (chartPoint) => $"pi: {chartPoint.PrimaryValue} Pa",
            };

            ObservablePoint[] p_sat_Curve_Values = new ObservablePoint[GlaserCalculation.LayerPsat.Count()]; // represents the temperature points
            for (int i = 0; i < GlaserCalculation.LayerPsat.Count(); i++)
            {
                double x = GlaserCalculation.LayerPsat.ElementAt(i).Key; // Position in cm
                double y = Math.Round(GlaserCalculation.LayerPsat.ElementAt(i).Value, 2); // Temperature in °C
                p_sat_Curve_Values[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> p_sat_Curve = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = p_sat_Curve_Values,
                Fill = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometryFill = new SolidColorPaint(SKColors.Red),
                GeometryStroke = new SolidColorPaint(SKColors.Red),
                GeometrySize = 6,
                TooltipLabelFormatter = (chartPoint) => $"p_sat_i: {chartPoint.PrimaryValue} Pa",
            };

            series[0] = p_Curve;
            series[1] = p_sat_Curve;

            return series;
        }
        private Axis[] DrawXAxes()
        {
            Axis[] axes = new Axis[1];

            axes[0] = new Axis
            {
                Name = "Element sd thickness [m]",
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
                Name = "p, psat [Pa]",
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
