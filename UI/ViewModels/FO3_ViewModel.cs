using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.SessionData;
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
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.UI.Drawing;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class FO3_ViewModel : ObservableObject
    {
        private readonly GlaserCalc _glaser = FO3_Moisture.GlaserCalculation;

        /*
         * Regular Instance Variables as Properties
         * 
         * Not depending on UI changes. No Observable function.
         */

        public string Title => "Moisture";
        public double Ti { get; private set; } = UserSaved.Ti;
        public double Te { get; private set; } = UserSaved.Te;
        public double Rel_Fi { get; private set; } = UserSaved.Rel_Fi;
        public double Rel_Fe { get; private set; } = UserSaved.Rel_Fe;
        public List<OverviewItem> OverviewItems => GetOverviewItemsList();
        public ISeries[] DataPoints => GetDataPoints();
        public RectangularSection[] LayerSections => DrawLayerSections();
        public Axis[] XAxes => DrawXAxes();
        public Axis[] YAxes => DrawYAxes();
        public SolidColorPaint TooltipBackgroundPaint { get; set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };

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
        private void EditElement() // Binding in XAML via 'EditElementCommand'
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditElementWindow().ShowDialog();

            // Update XAML Binding Property by fetching from DB
            OnPropertyChanged(nameof(SelectedElement));
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        /*
         * private Methods
         */

        private List<OverviewItem> GetOverviewItemsList()
        {
            if (!_glaser.IsValid) return new List<OverviewItem>();

            return new List<OverviewItem>
            {
                new OverviewItem { SymbolBase = "θ", SymbolSubscript = "si", Value = _glaser.Tsi, RequirementValue = _glaser.TaupunktMax_i, IsRequirementMet = _glaser.Tsi >= _glaser.TaupunktMax_i, Unit = "°C" },
                new OverviewItem { SymbolBase = "θ", SymbolSubscript = "se", Value = _glaser.Tse, RequirementValue = null, IsRequirementMet = true, Unit = "°C" },
                new OverviewItem { SymbolBase = "f", SymbolSubscript = "Rsi", Value = _glaser.FRsi, RequirementValue = 0.7, IsRequirementMet = _glaser.FRsi >= 0.7 },
                new OverviewItem { SymbolBase = "Φ", SymbolSubscript = "i", Value = UserSaved.Rel_Fi, RequirementValue = _glaser.PhiMax, IsRequirementMet = UserSaved.Rel_Fi < _glaser.PhiMax, Unit = "%" }
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

            ObservablePoint[] p_Curve_Values = new ObservablePoint[_glaser.LayerP.Count()]; // represents the temperature points
            for (int i = 0; i < _glaser.LayerP.Count(); i++)
            {
                double x = _glaser.LayerP.ElementAt(i).Key; // Position in cm
                double y = Math.Round(_glaser.LayerP.ElementAt(i).Value, 2); // Temperature in °C
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
                XToolTipLabelFormatter = (chartPoint) => $"pi: {chartPoint.Coordinate.PrimaryValue} Pa",
                YToolTipLabelFormatter = null
            };
            ObservablePoint[] p_sat_Curve_Values = new ObservablePoint[_glaser.LayerPsat.Count()]; // represents the temperature points
            for (int i = 0; i < _glaser.LayerPsat.Count(); i++)
            {
                double x = _glaser.LayerPsat.ElementAt(i).Key; // Position in cm
                double y = Math.Round(_glaser.LayerPsat.ElementAt(i).Value, 2); // Temperature in °C
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
                XToolTipLabelFormatter = (chartPoint) => $"p_sat_i: {chartPoint.Coordinate.PrimaryValue} Pa",
                YToolTipLabelFormatter = null
            };
            return new ISeries[] { p_Curve, p_sat_Curve };
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
