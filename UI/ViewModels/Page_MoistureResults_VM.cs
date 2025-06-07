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
    public partial class Page_MoistureResults_VM : ObservableObject
    {
        // Don't use Session.CalcResults: calculate TempCurve always homogeneous;
        // Manually Trigger Calculation
        private static GlaserCalc _glaser = new GlaserCalc();

        public Page_MoistureResults_VM()
        {
            if (Session.SelectedElement is null) return;

            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedElementChanged += RefreshXamlBindings;

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

        public string Title => Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Glaser-Diagramm" : "";
        public Element SelectedElement => Session.SelectedElement;
        public Visibility NoLayersVisibility => Session.SelectedElement?.Layers.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ResultsChartVisibility => Session.SelectedElement?.Layers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public double Ti => _glaser.Ti;
        public double Te => _glaser.Te;
        public double RelFi => _glaser.RelFi;
        public double RelFe => _glaser.RelFe;
        public List<OverviewItem> OverviewItems => GetOverviewItemsList();
        public ISeries[] DataPoints => GetDataPoints();
        public RectangularSection[] LayerSections => DrawLayerSections();
        public Axis[] XAxes => DrawXAxes();
        public Axis[] YAxes => DrawYAxes();
        public SolidColorPaint TooltipBackgroundPaint { get; set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };


        /*
         * private Methods
         */

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(OverviewItems));
        }
        private List<OverviewItem> GetOverviewItemsList()
        {
            if (!_glaser.IsValid) return new List<OverviewItem>();

            return new List<OverviewItem>
            {
                new OverviewItem { Symbol = Symbol.TemperatureSurfaceInterior, Value = _glaser.Tsi, RequirementValue = _glaser.TaupunktMax_i, IsRequirementMet = _glaser.Tsi >= _glaser.TaupunktMax_i, Unit = "°C" },
                new OverviewItem { Symbol = Symbol.TemperatureSurfaceExterior, Value = _glaser.Tse, RequirementValue = null, IsRequirementMet = true, Unit = "°C" },
                new OverviewItem { Symbol = Symbol.FRsi, Value = _glaser.FRsi, RequirementValue = 0.7, IsRequirementMet = _glaser.FRsi >= 0.7 },
                new OverviewItem { Symbol = Symbol.RelativeHumidityInterior, Value = Session.RelFi, RequirementValue = _glaser.PhiMax, IsRequirementMet = Session.RelFi < _glaser.PhiMax, Unit = "%" }
            };
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
