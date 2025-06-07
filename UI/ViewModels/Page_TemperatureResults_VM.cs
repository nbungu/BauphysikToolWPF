using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
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
using System.Linq;
using System.Windows;
using BauphysikToolWPF.Services.UI;
using static BauphysikToolWPF.Models.UI.Enums;
using Axis = LiveChartsCore.SkiaSharpView.Axis;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_TemperatureResults.cs: Used in xaml as "DataContext"
    public partial class Page_TemperatureResults_VM : ObservableObject
    {
        // Don't use Session.CalcResults: calculate TempCurve always homogeneous;
        // Manually Trigger Calculation
        //private static TemperatureCurveCalc _tempCurve = new TemperatureCurveCalc();

        private static GlaserCalc _glaser = new GlaserCalc();

        public Page_TemperatureResults_VM()
        {
            if (Session.SelectedElement is null) return;
            
            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedElementChanged += RefreshXamlBindings;

            //_tempCurve = new TemperatureCurveCalc(Session.SelectedElement, Session.ThermalValuesCalcConfig);
            //_tempCurve.CalculateHomogeneous();
            //_tempCurve.CalculateTemperatureCurve();

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
         * Not Observable, because Triggered and Changed by the elementType Value above
         */

        public string Title => Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Temperaturverlauf" : "";
        public Element? SelectedElement => Session.SelectedElement;
        public Visibility NoLayersVisibility => Session.SelectedElement?.Layers.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ResultsChartVisibility => Session.SelectedElement?.Layers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public double Ti => _glaser.Ti;
        public double Te => _glaser.Te;
        public double Rsi => _glaser.Rsi;
        public double Rse => _glaser.Rse;
        public ISeries[] DataPoints => GetDataPoints();
        public RectangularSection[] LayerSections => DrawLayerSections();
        public Axis[] XAxes => DrawXAxes();
        public Axis[] YAxes => DrawYAxes();
        public SolidColorPaint TooltipBackgroundPaint { get; private set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; private set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };
        public CheckRequirements RequirementValues => new CheckRequirements(SelectedElement, Session.CheckRequirementsConfig);

        public double? UValue => RequirementValues.Element?.UValue;
        public string UValueCaption => RequirementValues.UMaxCaption;
        public string UValueTooltip => SymbolMapping[Symbol.UValue].comment;
        public double UValueScaleMin => 0.0;
        public double UValueScaleMax => 2 * UValueRefMarker ?? 2 * UValue ?? 1.0;
        public double? UValueRefMarker => RequirementValues.UMax;
        public string UValueUnitString => GetUnitStringFromSymbol(Symbol.UValue);
        

        public double? RValue => RequirementValues.Element?.RGesValue;
        public string RValueCaption => RequirementValues.RMinRequirementSourceName != null ? $"Grenzwert nach {RequirementValues.RMinRequirementSourceName}" : "";
        public string RValueTooltip => SymbolMapping[Symbol.RValueElement].comment;
        public double RValueScaleMin => 0.0;
        public double RValueScaleMax => 2 * RValueRefMarker ?? 2 * RValue ?? 1.0;
        public double? RValueRefMarker => RequirementValues.RMin;
        public string RValueUnitString => GetUnitStringFromSymbol(Symbol.RValueElement);
        public string RValueSymbolBase => SymbolMapping[Symbol.RValueElement].baseText;
        public string RValueSymbolSubscript => SymbolMapping[Symbol.RValueElement].subscriptText;


        public double QValue => RequirementValues.Element.QValue;
        public string QValueCaption => "kein Grenzwert einzuhalten";
        public string QValueTooltip => SymbolMapping[Symbol.HeatFluxDensity].comment;
        public double QValueScaleMin => 0.0;
        public double QValueScaleMax => 2 * QValueRefMarker ?? 1.0;
        public double? QValueRefMarker => RequirementValues.QMax;
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




        //  new OverviewItem { Symbol = Symbol.RelativeHumidityInterior, Value = Session.RelFi, RequirementValue = _glaser.PhiMax, IsRequirementMet = Session.RelFi < _glaser.PhiMax, Unit = "%" }


        //public List<OverviewItem> OverviewItems
        //{
        //    // TODO Rework: -> ProperyItem
        //    get
        //    {
        //        if (!_tempCurve.IsValid) return new List<OverviewItem>();
        //        return new List<OverviewItem>
        //        {
        //            new OverviewItem { Symbol = Symbol.RValueElement, Value = RequirementValues.Element.RGesValue, RequirementValue = RequirementValues.RMin >= 0 ? RequirementValues.RMin : null, IsRequirementMet = RequirementValues.IsRValueOk, Unit = "m²K/W" },
        //            new OverviewItem { Symbol = Symbol.RValueTotal, Value = RequirementValues.Element.RTotValue, RequirementValue = null, IsRequirementMet = RequirementValues.IsRValueOk, Unit = "m²K/W" },
        //            new OverviewItem { Symbol = Symbol.UValue, Value = RequirementValues.Element.UValue, RequirementValue = RequirementValues.UMax >= 0 ? RequirementValues.UMax : null, IsRequirementMet = RequirementValues.IsUValueOk, Unit = "W/m²K" },
        //            new OverviewItem { Symbol = Symbol.HeatFluxDensity, Value = RequirementValues.Element.QValue, RequirementValue = RequirementValues.QMax >= 0 ? RequirementValues.QMax : null, IsRequirementMet = RequirementValues.IsQValueOk, Unit = "W/m²" },

        //            new OverviewItem { Symbol = Symbol.TemperatureSurfaceInterior, Value = _tempCurve.Tsi, RequirementValue = _tempCurve.TsiMin, IsRequirementMet = _tempCurve.Tsi >= _tempCurve.TsiMin, Unit = "°C" },
        //            new OverviewItem { Symbol = Symbol.TemperatureSurfaceExterior, Value = _tempCurve.Tse, RequirementValue = null, IsRequirementMet = true, Unit = "°C" },
        //            new OverviewItem { Symbol = Symbol.FRsi, Value = _tempCurve.FRsi, RequirementValue = 0.7, IsRequirementMet = _tempCurve.FRsi >= 0.7 },
        //        };
        //    }
        //}

        /*
         * private Methods
         */

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(RequirementValues));
            //OnPropertyChanged(nameof(OverviewItems));
        }

        private RectangularSection[] DrawLayerSections()
        {
            if (!_glaser.IsValid) return Array.Empty<RectangularSection>();

            RectangularSection[] rects = new RectangularSection[_glaser.Element.Layers.Count];

            double left = 0;
            foreach (Layer layer in _glaser.Element.Layers)
            {
                double layerWidth = layer.Thickness;
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
            //TODO: is hardcoded
            //fRsi frsi = new fRsi(_tempCalc.LayerTemps.First().Value, Temperatures.selectedTi.First().Value, Temperatures.selectedTe.First().Value);
            /*rects[position+1] = new RectangularSection
            {
                Yi = TemperatureCurveCalc.TsiMin,
                Yj = TemperatureCurveCalc.TsiMin,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.Red,
                    StrokeThickness = 1,
                    PathEffect = new DashEffect(new float[] { 2, 2 })
                }
            };*/
            return rects;
        }
        private ISeries[] GetDataPoints()
        {
            if (!_glaser.IsValid) return Array.Empty<ISeries>();

            double tsiPos = 0;
            double tsi = _glaser.Tsi;
            double deltaTi = Math.Abs(Ti - tsi);

            LineSeries<ObservablePoint> rsiCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tsiPos-0.8, tsi+0.9*deltaTi),
                    new ObservablePoint(), // null cuts the line between the points // TODO check if null works
                    new ObservablePoint(tsiPos, tsi),
                    new ObservablePoint(tsiPos-0.8, tsi+0.9*deltaTi),
                    new ObservablePoint(tsiPos-2, Ti)
                },
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                XToolTipLabelFormatter = null,
                YToolTipLabelFormatter = null
            };

            ObservablePoint[] tempValues = new ObservablePoint[_glaser.LayerTemps.Count]; // represents the temperature points
            for (int i = 0; i < _glaser.LayerTemps.Count; i++)
            {
                double x = _glaser.LayerTemps.ElementAt(i).Key; // Position in cm
                double y = Math.Round(_glaser.LayerTemps.ElementAt(i).Value, 2); // Temperature in °C
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
                XToolTipLabelFormatter = (chartPoint) => $"Temperature: {chartPoint.Coordinate.PrimaryValue} °C",
                YToolTipLabelFormatter = null,
                //When multiple axes:
                ScalesYAt = 0, // it will be scaled at the YAxes[0] instance
                ScalesXAt = 0 // it will be scaled at the XAxes[0] instance
            };

            double tsePos = _glaser.LayerTemps.Last().Key;
            double tse = _glaser.Tse;
            double deltaTe = Math.Abs(Te - tse);
            LineSeries<ObservablePoint> rseCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tsePos+2, Te),
                    new ObservablePoint(tsePos+0.8, tse-0.9*deltaTe),
                    new ObservablePoint(tsePos, tse),
                    new ObservablePoint(), // null cuts the line between the points // TODO check if null works
                    new ObservablePoint(tsePos+0.8, tse+0.9*deltaTe),
                },
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                XToolTipLabelFormatter = null,
                YToolTipLabelFormatter = null
            };
            return new ISeries[] { rsiCurveSeries, tempCurveSeries, rseCurveSeries };
        }
        private Axis[] DrawXAxes()
        {
            if (!_glaser.IsValid) return Array.Empty<Axis>();
            return new Axis[]
            {
                new Axis
                {
                    Name = "Element thickness [cm]",
                    NameTextSize = 14,
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    //Labels = new string[] { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5" },
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    MinLimit = - 2,
                    MaxLimit = _glaser.Element.Thickness + 2
                }
                /*new Axis
                {
                    Name = "Layer Nr.",
                    NameTextSize = 16,
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    Labels = new string[] { "1", "2", "3", "4" },
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14
                }*/
            };
        }
        private Axis[] DrawYAxes()
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = "Temperature curve [°C]",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,
                    NameTextSize = 14,
                    Position = LiveChartsCore.Measure.AxisPosition.Start,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 3, 3 }) }
                }
                /*
                new Axis
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
                }*/
            };
        }
    }
}
