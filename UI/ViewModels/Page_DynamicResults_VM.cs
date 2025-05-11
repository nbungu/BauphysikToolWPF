using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.UI.Enums;
using Axis = LiveChartsCore.SkiaSharpView.Axis;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_DynamicResults_VM : ObservableObject
    {
        // Don't use Session.CalcResults: calculate TempCurve always homogeneous;
        // Manually Trigger Calculation
        private static DynamicTempCalc _dynamicTempCalc = new DynamicTempCalc();
        private readonly CrossSectionDrawing _verticalCut = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 360, 450), DrawingType.VerticalCut);

        public Page_DynamicResults_VM()
        {
            if (Session.SelectedElement is null) return;

            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedElementChanged += RefreshXamlBindings;

            _dynamicTempCalc = new DynamicTempCalc(Session.SelectedElement, Session.ThermalValuesCalcConfig);
            _dynamicTempCalc.CalculateHomogeneous();
            _dynamicTempCalc.CalculateDynamicValues();
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DataPoints_i))]
        [NotifyPropertyChangedFor(nameof(DataPoints_e))]
        [NotifyPropertyChangedFor(nameof(YAxes_i))]
        [NotifyPropertyChangedFor(nameof(YAxes_e))]
        private double ti_Mean = 25;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DataPoints_i))]
        [NotifyPropertyChangedFor(nameof(DataPoints_e))]
        [NotifyPropertyChangedFor(nameof(YAxes_i))]
        [NotifyPropertyChangedFor(nameof(YAxes_e))]
        private double te_Mean = 25;

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
        private double te_Amplitude = 10;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the Values above
         */

        public string Title => Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Dynamisches Bauteilverhalten" : "";
        public Element SelectedElement => Session.SelectedElement;

        // Vertical Cut
        public List<IDrawingGeometry> VerticalCutDrawing => _verticalCut.DrawingGeometries;
        public Rectangle CanvasSizeVerticalCut => _verticalCut.CanvasSize;
        public ISeries[] DataPoints_i => GetDataPoints_i();
        public ISeries[] DataPoints_e => GetDataPoints_e();
        public Axis[] YAxes_i => DrawYAxes();
        public Axis[] YAxes_e => DrawYAxes("right");

        public List<IPropertyItem> DynamicThermalValues => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.RValueDynamic, () => _dynamicTempCalc.DynamicRValue),
            new PropertyItem<double>(Symbol.UValueDynamic, () => _dynamicTempCalc.DynamicUValue),
            new PropertyItem<double>(Symbol.TemperatureAmplitudeDamping, () => _dynamicTempCalc.TAD),
            new PropertyItem<double>(Symbol.TemperatureAmplitudeRatio, () => _dynamicTempCalc.TAV),
            new PropertyItem<double>(Symbol.TimeShift, () => Math.Round(Convert.ToDouble(_dynamicTempCalc.TimeShift) / 3600, 2)),
            new PropertyItem<double>(Symbol.EffectiveThermalMass, () => _dynamicTempCalc.EffectiveThermalMass),
            new PropertyItem<double>(Symbol.DecrementFactor, () => _dynamicTempCalc.DecrementFactor),
        };
        public List<IPropertyItem> DynamicThermalValuesInterior => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.TimeShift, () => Math.Round(Convert.ToDouble(_dynamicTempCalc.TimeShift_i) / 3600, 2)) { SymbolSubscriptText = "1"},
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => _dynamicTempCalc.ArealHeatCapacity_i) { SymbolBaseText = "K" , SymbolSubscriptText = "1"},
            new PropertyItem<double>(Symbol.ThermalAdmittance, () => _dynamicTempCalc.ThermalAdmittance_i) { SymbolSubscriptText = "11"},
            new PropertyItem<double>(Symbol.DynamicThermalAdmittance, () => _dynamicTempCalc.DynamicThermalAdmittance_i) { SymbolSubscriptText = "12"},
        };
        public List<IPropertyItem> DynamicThermalValuesExterior => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.TimeShift, () => Math.Round(Convert.ToDouble(_dynamicTempCalc.TimeShift_e) / 3600, 2)) { SymbolSubscriptText = "2" },
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => _dynamicTempCalc.ArealHeatCapacity_e) { SymbolBaseText = "K", SymbolSubscriptText = "2"},
            new PropertyItem<double>(Symbol.ThermalAdmittance, () => _dynamicTempCalc.ThermalAdmittance_e) { SymbolSubscriptText = "22" },
        };

        public LiveChartsCore.Measure.Margin ChartMargin_i { get; private set; } = new LiveChartsCore.Measure.Margin(64, 16, 0, 64);
        public LiveChartsCore.Measure.Margin ChartMargin_e { get; private set; } = new LiveChartsCore.Measure.Margin(0, 16, 64, 64);
        public SolidColorPaint TooltipBackgroundPaint { get; private set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; private set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };
        public Axis[] XAxes => DrawXAxes();

        /*
         * private Methods
         */

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(SelectedElement));
        }

        private ISeries[] GetDataPoints_e()
        {
            if (!_dynamicTempCalc.IsValid) return Array.Empty<ISeries>();

            LineSeries<ObservablePoint> surfaceTemp_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Oberflächentemperatur",
                // TODO let user change Rsi, Rse
                Values = _dynamicTempCalc.CreateDataPoints(Symbol.TemperatureSurfaceExterior, Te_Mean, Ti_Mean, Ti_Amplitude, Te_Amplitude),
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                XToolTipLabelFormatter = (chartPoint) => $"Oberflächentemperatur: {chartPoint.Coordinate.PrimaryValue} °C",
                YToolTipLabelFormatter = null,
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            LineSeries<ObservablePoint> temp_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Lufttemperatur",
                Values = _dynamicTempCalc.CreateDataPoints(Symbol.TemperatureExterior, Te_Mean, Ti_Mean, Ti_Amplitude, Te_Amplitude),
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2, PathEffect = new DashEffect(new float[] { 3, 3 }) },
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                XToolTipLabelFormatter = (chartPoint) => $"Außenlufttemperatur: {chartPoint.Coordinate.PrimaryValue} °C ({chartPoint.Coordinate.SecondaryValue} s)",
                YToolTipLabelFormatter = null,
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            return new ISeries[] { temp_e, surfaceTemp_e };
        }
        private ISeries[] GetDataPoints_i()
        {
            if (!_dynamicTempCalc.IsValid) return Array.Empty<ISeries>();

            LineSeries<ObservablePoint> surfaceTemp_i = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Oberflächentemperatur",
                // TODO add qStatic!
                Values = _dynamicTempCalc.CreateDataPoints(Symbol.TemperatureSurfaceInterior, Te_Mean, Ti_Mean, Ti_Amplitude, Te_Amplitude),
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                XToolTipLabelFormatter = (chartPoint) => $"Oberflächentemperatur: {chartPoint.Coordinate.PrimaryValue} °C ({chartPoint.Coordinate.SecondaryValue} s)",
                YToolTipLabelFormatter = null,
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            LineSeries<ObservablePoint> temp_i = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Name = "Lufttemperatur",
                Values = _dynamicTempCalc.CreateDataPoints(Symbol.TemperatureInterior, Te_Mean, Ti_Mean, Ti_Amplitude, Te_Amplitude),
                Fill = null,
                LineSmoothness = 0, // where 0 is a straight line and 1 the most curved
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2, PathEffect = new DashEffect(new float[] { 3, 3 }) },
                GeometrySize = 4,
                GeometryFill = null,
                GeometryStroke = null,
                XToolTipLabelFormatter = (chartPoint) => $"Raumlufttemperatur: {chartPoint.Coordinate.PrimaryValue} °C",
                YToolTipLabelFormatter = null,
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };
            return new ISeries[] { temp_i, surfaceTemp_i }; // more than one series possible to draw in the same graph
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
                    MinLimit = 0,
                    MaxLimit = DynamicTempCalc.PeriodDuration
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
