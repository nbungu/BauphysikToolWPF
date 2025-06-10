using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.UI.Enums;
using Axis = LiveChartsCore.SkiaSharpView.Axis;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_MoistureResults_VM : ObservableObject
    {
        // Don't use Session.CalcResults: calculate TempCurve always homogeneous;
        // Manually Trigger Calculation
        private static GlaserCalc _glaser = new GlaserCalc();
        private readonly CheckRequirements _requirementValues = new CheckRequirements();

        public Page_MoistureResults_VM()
        {
            if (Session.SelectedElement is null) return;

            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedElementChanged += RefreshXamlBindings;

            // TODO: those could be 'Element' properties and be fetched from the SelectedElement directly
            // -> just set Update flag to true
            _requirementValues = new CheckRequirements(Session.SelectedElement, Session.CheckRequirementsConfig);

            _glaser = new GlaserCalc(Session.SelectedElement, Session.ThermalValuesCalcConfig);
            _glaser.CalculateHomogeneous(); // Bauteil berechnen
            _glaser.CalculateTemperatureCurve(); // Temperaturkurve
            _glaser.CalculateGlaser(); // Glaser Kurve
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



        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the Values above
         */

        public string Title { get; } = Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Glaser-Diagramm" : "";
        public string SelectedElementColorCode { get; } = Session.SelectedElement?.ColorCode ?? string.Empty;
        public string SelectedElementConstructionName { get; } = Session.SelectedElement?.Construction.TypeName ?? string.Empty;
        public Visibility NoLayersVisibility => Session.SelectedElement?.Layers.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ResultsChartVisibility => Session.SelectedElement?.Layers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public double Ti => _glaser.Ti;
        public double Te => _glaser.Te;
        public double RelFi => _glaser.RelFi;
        public double RelFe => _glaser.RelFe;
        public ISeries[] DataPoints => GetDataPoints();
        public RectangularSection[] LayerSections => DrawLayerSections();
        public Axis[] XAxes => DrawXAxes();
        public Axis[] YAxes => DrawYAxes();
        public SolidColorPaint TooltipBackgroundPaint { get; set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };


        private GaugeItem? _uValueGauge;
        /// <summary>
        /// Gets the gauge configuration, lazily initializing it on first access.
        /// 
        /// This lazy pattern ensures that the GaugeItem is only constructed when needed, 
        /// avoiding repeated allocations from XAML bindings that may call this getter multiple times. 
        /// It improves performance and keeps initialization logic encapsulated.
        /// </summary>
        public GaugeItem UValueGauge
        {
            get
            {
                if (_uValueGauge == null)
                {
                    double? uMax = _requirementValues.UMax;
                    double elementUValue = _requirementValues.Element.UValue;
                    _uValueGauge = new GaugeItem(Symbol.UValue, elementUValue, uMax, _requirementValues.UMaxComparisonRequirement)
                    {
                        Caption = _requirementValues.UMaxCaption,
                        ScaleMin = 0.0,
                        ScaleMax = uMax.HasValue ? Math.Max(2 * uMax.Value, elementUValue + 0.1) : Math.Max(1.0, elementUValue + 0.1)
                    };
                }
                return _uValueGauge;
            }
        }

        private GaugeItem? _rValueGauge;
        /// <summary>
        /// Gets the gauge configuration, lazily initializing it on first access.
        /// 
        /// This lazy pattern ensures that the GaugeItem is only constructed when needed, 
        /// avoiding repeated allocations from XAML bindings that may call this getter multiple times. 
        /// It improves performance and keeps initialization logic encapsulated.
        /// </summary>
        public GaugeItem RValueGauge
        {
            get
            {
                if (_rValueGauge == null)
                {
                    var uValueGauge = UValueGauge; // ensure initialized once
                    double uValueNormalized = (uValueGauge.Value - uValueGauge.ScaleMin) / (uValueGauge.ScaleMax - uValueGauge.ScaleMin);
                    double targetRValueNormalized = 1.0 - uValueNormalized;

                    _rValueGauge = new GaugeItem(Symbol.RValueElement, _requirementValues.Element.RGesValue, _requirementValues.RMin, _requirementValues.RMinComparisonRequirement)
                    {
                        Caption = _requirementValues.RMinCaption,
                        ScaleMin = 0.0,
                        ScaleMax = _requirementValues.Element.RGesValue / targetRValueNormalized,
                    };
                }
                return _rValueGauge;
            }
        }


        public double QValue => _requirementValues.Element.QValue;
        public string QValueCaption => "kein Grenzwert einzuhalten";
        public string QValueTooltip => SymbolMapping[Symbol.HeatFluxDensity].comment;
        public double QValueScaleMin => 0.0;
        public double QValueScaleMax => 2 * QValueRefMarker ?? 1.0;
        public double? QValueRefMarker => _requirementValues.QMax;
        public string QValueUnitString => GetUnitStringFromSymbol(Symbol.HeatFluxDensity);
        public string QValueSymbolBase => SymbolMapping[Symbol.HeatFluxDensity].baseText;


        public double TsiValue => _glaser.Tsi;
        public string TsiCaption => "zur Vermeidung von Schimmelpilzbildung";
        public double TsiValueScaleMin => _glaser.Te;
        public double TsiValueScaleMax => _glaser.Ti;
        public double TsiValueRefMarker => _glaser.TsiMin;
        public string TsiValueUnitString => GetUnitStringFromSymbol(Symbol.TemperatureSurfaceInterior);
        public string TsiSymbolBase => SymbolMapping[Symbol.TemperatureSurfaceInterior].baseText;
        public string TsiSymbolSubscript => $"{SymbolMapping[Symbol.TemperatureSurfaceInterior].subscriptText}";
        public string TsiMarkerSymbolBase => SymbolMapping[Symbol.TemperatureSurfaceInterior].baseText;
        public string TsiMarkerSymbolSubscript => $"{SymbolMapping[Symbol.TemperatureSurfaceInterior].subscriptText}" + ",min";


        public double FRsiValue => _glaser.FRsi;
        public string FRsiCaption => "zur Vermeidung von Schimmelpilzbildung";
        public string FRsiTooltip => SymbolMapping[Symbol.FRsi].comment;
        public double FRsiValueScaleMin => 0.0;
        public double FRsiValueScaleMax => 1.0;
        public double FRsiValueRefMarker => 0.7; // TODO:
        public string FRsiValueUnitString => GetUnitStringFromSymbol(Symbol.FRsi);
        public string FRsiSymbolBase => SymbolMapping[Symbol.FRsi].baseText;
        public string FRsiSymbolSubscript => $"{SymbolMapping[Symbol.FRsi].subscriptText}";
        public string FRsiMarkerSymbolBase => SymbolMapping[Symbol.FRsi].baseText;
        public string FRsiMarkerSymbolSubscript => $"{SymbolMapping[Symbol.FRsi].subscriptText}" + ",min";

        public double RelFiValue => _glaser.RelFi;
        public string RelFiCaption => "zur Vermeidung von Schimmelpilzbildung";
        public double RelFiValueScaleMin => 0.0;
        public double RelFiValueScaleMax => 100.0;
        public double RelFiValueRefMarker => _glaser.PhiMax; // TODO:
        public string RelFiValueUnitString => GetUnitStringFromSymbol(Symbol.RelativeHumidityInterior);
        public string RelFiSymbolBase => SymbolMapping[Symbol.RelativeHumidityInterior].baseText;
        public string RelFiSymbolSubscript => $"{SymbolMapping[Symbol.RelativeHumidityInterior].subscriptText}";
        public string RelFiMarkerSymbolBase => SymbolMapping[Symbol.RelativeHumidityInterior].baseText;
        public string RelFiMarkerSymbolSubscript => $"{SymbolMapping[Symbol.RelativeHumidityInterior].subscriptText}" + ",max";

        /*
         * private Methods
         */

        private void RefreshXamlBindings()
        {
            
        }
        
        private RectangularSection[] DrawLayerSections()
        {
            if (!_glaser.IsValid) return Array.Empty<RectangularSection>();

            RectangularSection[] rects = new RectangularSection[_glaser.Element.Layers.Count];

            //TODO: Round values of left and right
            double left = 0;
            foreach (Layer layer in _glaser.Element.Layers)
            {
                double layerWidth = layer.Sd_Thickness;
                double right = left + layerWidth; // start drawing from left side (beginning with INSIDE Layer, which is first list element)

                // Set properties of the layer rectangle at the desired position
                rects[layer.LayerPosition] = new RectangularSection
                {
                    Xi = left,
                    Xj = right,
                    Fill = new SolidColorPaint(SKColor.Parse(layer.Material.ColorCode)),
                    Stroke = new SolidColorPaint { Color = SKColors.Black, StrokeThickness = 1 },
                    ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
                };
                left = right; // Add new layer at left edge of previous layer
            }
            return rects;
        }
        private ISeries[] GetDataPoints()
        {
            if (!_glaser.IsValid) return Array.Empty<ISeries>();

            ObservablePoint[] pCurveValues = new ObservablePoint[_glaser.LayerP.Count()]; // represents the temperature points
            for (int i = 0; i < _glaser.LayerP.Count(); i++)
            {
                double x = _glaser.LayerP.ElementAt(i).Key; // Position in cm
                double y = Math.Round(_glaser.LayerP.ElementAt(i).Value, 2); // Temperature in °C
                pCurveValues[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> pCurve = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = pCurveValues,
                Fill = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Blue, 2),
                GeometryFill = new SolidColorPaint(SKColors.Blue),
                GeometryStroke = new SolidColorPaint(SKColors.Blue),
                GeometrySize = 6,
                XToolTipLabelFormatter = (chartPoint) => $"pi: {chartPoint.Coordinate.PrimaryValue} Pa",
                YToolTipLabelFormatter = null
            };
            ObservablePoint[] pSatCurveValues = new ObservablePoint[_glaser.LayerPsat.Count()]; // represents the temperature points
            for (int i = 0; i < _glaser.LayerPsat.Count(); i++)
            {
                double x = _glaser.LayerPsat.ElementAt(i).Key; // Position in cm
                double y = Math.Round(_glaser.LayerPsat.ElementAt(i).Value, 2); // Temperature in °C
                pSatCurveValues[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            LineSeries<ObservablePoint> pSatCurve = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = pSatCurveValues,
                Fill = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometryFill = new SolidColorPaint(SKColors.Red),
                GeometryStroke = new SolidColorPaint(SKColors.Red),
                GeometrySize = 6,
                XToolTipLabelFormatter = (chartPoint) => $"p_sat_i: {chartPoint.Coordinate.PrimaryValue} Pa",
                YToolTipLabelFormatter = null
            };
            return new ISeries[] { pCurve, pSatCurve };
        }
        private Axis[] DrawXAxes()
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = "Element sd thickness [m]",
                    NameTextSize = 14,
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                }
            };
        }
        private Axis[] DrawYAxes()
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = "p, psat [Pa]",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,
                    NameTextSize = 14,
                    Position = LiveChartsCore.Measure.AxisPosition.Start,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 3, 3 }) }
                }
            };
        }
    }
}
