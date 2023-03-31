using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.Helper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class FO4_ViewModel : ObservableObject
    {
        /*
         * Regular Instance Variables
         * 
         * Not depending on UI changes. No Observable function. 
         * 
         */

        public string Title { get; } = "Dynamic";
        public DynamicTempCalc DynamicTempCalc { get; private set; } = FO4_Dynamic.DynamicTempCalculation;
        public List<OverviewItem> DynamicThermalValues
        {
            get
            {
                return new List<OverviewItem>(7)
                {
                    new OverviewItem { SymbolBase = "R", SymbolSubscript = "dyn", Value = DynamicTempCalc.DynamicRValue, Unit = "m²K/W" },
                    new OverviewItem { SymbolBase = "U", SymbolSubscript = "dyn", Value = DynamicTempCalc.DynamicUValue, Unit = "W/m²K" },
                    new OverviewItem { SymbolBase = "TAD", Value = DynamicTempCalc.TAD },
                    new OverviewItem { SymbolBase = "TAV", Value = DynamicTempCalc.TAV, Comment = "Temperaturamplitudenverhältnis" },
                    new OverviewItem { SymbolBase = "Δt", SymbolSubscript = "f", Value = Math.Round(Convert.ToDouble(DynamicTempCalc.TimeShift) / 3600, 2), Unit = "h", Comment = "Phasenverschiebung" },
                    new OverviewItem { SymbolBase = "M", Value = DynamicTempCalc.EffectiveThermalMass, Unit = "kg/m²", Comment = "speicherwirksame Masse" },
                    new OverviewItem { SymbolBase = "f", Value = DynamicTempCalc.DecrementFactor, Comment = "Dekrement" }
                };
            }
        }
        public List<OverviewItem> DynamicThermalValues_i
        {
            get
            {
                return new List<OverviewItem>(6)
                {
                    new OverviewItem { SymbolBase = "Δt", SymbolSubscript = "1", Value = Math.Round(Convert.ToDouble(DynamicTempCalc.TimeShift_i) / 3600, 2), Unit = "h" },
                    new OverviewItem { SymbolBase = "Κ", SymbolSubscript = "1", Value = DynamicTempCalc.ArealHeatCapacity_i, Unit = "kJ/(m²K)" },
                    new OverviewItem { SymbolBase = "Y", SymbolSubscript = "1", Value = DynamicTempCalc.ThermalAdmittance_i, Unit = "W/(m²K)" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "si,max", Value = 0, Unit = "°C" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "si,min", Value = 0, Unit = "°C" },
                    new OverviewItem { SymbolBase = "Δθ", SymbolSubscript = "si", Value = 0, Unit = "K" }
                };
            }
        }
        public List<OverviewItem> DynamicThermalValues_e
        {
            get
            {
                return new List<OverviewItem>(6)
                {
                    new OverviewItem { SymbolBase = "Δt", SymbolSubscript = "2", Value = Math.Round(Convert.ToDouble(DynamicTempCalc.TimeShift_e) / 3600, 2), Unit = "h" },
                    new OverviewItem { SymbolBase = "Κ", SymbolSubscript = "2", Value = DynamicTempCalc.ArealHeatCapacity_e, Unit = "kJ/(m²K)" },
                    new OverviewItem { SymbolBase = "Y", SymbolSubscript = "2", Value = DynamicTempCalc.ThermalAdmittance_e, Unit = "W/(m²K)" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "se,max", Value = 0, Unit = "°C" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "se,min", Value = 0, Unit = "°C" },
                    new OverviewItem { SymbolBase = "Δθ", SymbolSubscript = "se", Value = 0, Unit = "K" }
                };
            }
        }
        // TOD0 Make cleaner
        public List<LayerRect> LayerRects // When accessed via get: Draws new Layers on Canvas
        {
            get
            {
                List<LayerRect> rectangles = new List<LayerRect>();
                foreach (Layer layer in DynamicTempCalc.Element.Layers)
                {
                    layer.IsSelected = false;
                    rectangles.Add(new LayerRect(ElementWidth, 320, 400, layer, rectangles.LastOrDefault()));
                }
                return rectangles;
            }
        }
        public double ElementWidth
        {
            get
            {
                double fullWidth = 0;
                foreach (Layer layer in DynamicTempCalc.Element.Layers)
                {
                    fullWidth += layer.LayerThickness;
                }
                return fullWidth;
            }
        }
        public LiveChartsCore.Measure.Margin ChartMargin_i { get; private set; } = new LiveChartsCore.Measure.Margin(80, 32, 0, 80);
        public LiveChartsCore.Measure.Margin ChartMargin_e { get; private set; } = new LiveChartsCore.Measure.Margin(0, 32, 80, 80);
        public SolidColorPaint TooltipBackgroundPaint { get; private set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; private set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };
        public SolidColorPaint LegendTextPaint { get; private set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };

        public Axis[] XAxes
        {
            get
            {
                return DrawXAxes();
            }
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void EditElement(Element? selectedElement) // Binding in XAML via 'ElementChangeCommand'
        {
            if (selectedElement is null)
                selectedElement = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId);

            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new EditElementWindow(selectedElement);
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            ElementName = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Name;
            ElementType = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Construction.Type;
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string elementName = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Name;

        [ObservableProperty]
        private string elementType = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Construction.Type;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DataPoints_i))]
        [NotifyPropertyChangedFor(nameof(DataPoints_e))]
        [NotifyPropertyChangedFor(nameof(YAxes_i))]
        [NotifyPropertyChangedFor(nameof(YAxes_e))]
        private double ti_Mean = UserSaved.Ti;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DataPoints_i))]
        [NotifyPropertyChangedFor(nameof(DataPoints_e))]
        [NotifyPropertyChangedFor(nameof(YAxes_i))]
        [NotifyPropertyChangedFor(nameof(YAxes_e))]
        private double te_Mean = UserSaved.Ti;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DataPoints_i))]
        [NotifyPropertyChangedFor(nameof(DataPoints_e))]
        [NotifyPropertyChangedFor(nameof(YAxes_i))]
        [NotifyPropertyChangedFor(nameof(YAxes_e))]
        private double ti_Amplitude = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DataPoints_i))]
        [NotifyPropertyChangedFor(nameof(DataPoints_e))]
        [NotifyPropertyChangedFor(nameof(YAxes_i))]
        [NotifyPropertyChangedFor(nameof(YAxes_e))]
        private double te_Amplitude = 5.0;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the Values above
         */

        public ISeries[] DataPoints_i
        {
            get
            {
                return GetDataPoints_i();
            }
        }
        public ISeries[] DataPoints_e
        {
            get
            {
                return GetDataPoints_e();
            }
        }
        public Axis[] YAxes_i
        {
            get
            {
                return DrawYAxes();
            }
        }
        public Axis[] YAxes_e
        {
            get
            {
                return DrawYAxes("right");
            }
        }

        private ISeries[] GetDataPoints_e()
        {
            if (DynamicTempCalc.Element.Layers.Count == 0)
                return Array.Empty<ISeries>();

            ISeries[] series = new ISeries[2]; // more than one series possible to draw in the same graph      
            int iterations = DynamicTempCalc.PeriodDuration / DynamicTempCalc.IntervallSteps + 1;

            // θse(t) Data Points
            ObservablePoint[] surfaceTemp_e_Points = new ObservablePoint[iterations];
            for (int i = 0; i < iterations; i++)
            {
                int timePoint = i * DynamicTempCalc.IntervallSteps; // time axis [s]
                double x = timePoint;
                double y = DynamicTempCalc.SurfaceTemp_e_Function(timePoint, Te_Mean, Ti_Amplitude, Te_Amplitude); // temperature axis [°C]
                surfaceTemp_e_Points[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> surfaceTemp_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Oberflächentemperatur",
                Values = surfaceTemp_e_Points,
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 4,
                GeometryFill= null,
                GeometryStroke= null,
                TooltipLabelFormatter = (chartPoint) => $"Temperature: {chartPoint.PrimaryValue} °C",
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            // θe(t) Data Points
            ObservablePoint[] temp_e_Points = new ObservablePoint[iterations];
            for (int i = 0; i < iterations; i++)
            {
                int timePoint = i * DynamicTempCalc.IntervallSteps; // time axis [s]
                double x = timePoint;
                double y = DynamicTempCalc.AirTemp_Function(timePoint, Te_Mean, Te_Amplitude); // temperature axis [°C]
                temp_e_Points[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> temp_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Lufttemperatur",
                Values = temp_e_Points,
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2, PathEffect = new DashEffect(new float[] { 3, 3 }) },
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                TooltipLabelFormatter = (chartPoint) => $"Temperature: {chartPoint.PrimaryValue} °C ({chartPoint.SecondaryValue} s)",
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };

            series[0] = temp_e;
            series[1] = surfaceTemp_e;

            return series;
        }
        private ISeries[] GetDataPoints_i()
        {
            if (DynamicTempCalc.Element.Layers.Count == 0)
                return Array.Empty<ISeries>();

            ISeries[] series = new ISeries[2]; // more than one series possible to draw in the same graph      
            int iterations = DynamicTempCalc.PeriodDuration / DynamicTempCalc.IntervallSteps + 1;

            // θsi(t) Data Points
            ObservablePoint[] surfaceTemp_i_Points = new ObservablePoint[iterations];
            for (int i = 0; i < iterations; i++)
            {
                int timePoint = i * DynamicTempCalc.IntervallSteps; // time axis [s]
                double x = timePoint;
                double y = DynamicTempCalc.SurfaceTemp_i_Function(timePoint, Ti_Mean, Ti_Amplitude, Te_Amplitude); // temperature axis [°C]
                surfaceTemp_i_Points[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> surfaceTemp_i = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Oberflächentemperatur",
                Values = surfaceTemp_i_Points,
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                TooltipLabelFormatter = (chartPoint) => $"Temperature: {chartPoint.PrimaryValue} °C ({chartPoint.SecondaryValue} s)",
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            // θi(t) Data Points
            ObservablePoint[] temp_i_Points = new ObservablePoint[iterations];
            for (int i = 0; i < iterations; i++)
            {
                int timePoint = i * DynamicTempCalc.IntervallSteps; // time axis [s]
                double x = timePoint;
                double y = DynamicTempCalc.AirTemp_Function(timePoint, Ti_Mean, Ti_Amplitude); // temperature axis [°C]
                temp_i_Points[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> temp_i = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Lufttemperatur",
                Values = temp_i_Points,
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2, PathEffect = new DashEffect(new float[] { 3, 3 }) },
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                TooltipLabelFormatter = (chartPoint) => $"Temperature: {chartPoint.PrimaryValue} °C",
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            series[0] = temp_i;
            series[1] = surfaceTemp_i;
            //series[2] = surfaceHeatFlux_i;

            return series;
        }
        private Axis[] DrawXAxes(string side = "bottom")
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = "zeitlicher Tagesverlauf [s]",
                    NameTextSize = 14,
                    Position = (side == "bottom") ? LiveChartsCore.Measure.AxisPosition.Start : LiveChartsCore.Measure.AxisPosition.End,
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    // Hardcode min/max value to draw all the way to the chart edges
                    MinLimit= 0,
                    MaxLimit= 86400
                }
            };
        }
        private Axis[] DrawYAxes(string side = "left")
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = "Temperatur [°C]",
                    Position = (side == "left") ? LiveChartsCore.Measure.AxisPosition.Start : LiveChartsCore.Measure.AxisPosition.End,
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,
                    NameTextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 3, 3 }) },
                    // Force both Charts to have the same axis scale (To make them comparable). Use Scale of the chart with highest amplitudes. (+/- 1 as padding)
                    MinLimit = Math.Min(Ti_Mean - Ti_Amplitude, Te_Mean - Te_Amplitude) - 1,
                    MaxLimit = Math.Max(Ti_Mean + Ti_Amplitude, Te_Mean + Te_Amplitude) + 1
                }
            };
        }
    }
}
