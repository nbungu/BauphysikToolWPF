﻿using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Helper;
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
using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO2_Temperature.cs: Used in xaml as "DataContext"
    public partial class FO2_ViewModel : ObservableObject
    {
        private readonly StationaryTempCalc _tempCalc = FO2_Temperature.StationaryTempCalculation;

        /*
         * Regular Instance Variables as Properties
         * 
         * Not depending on UI changes. No Observable function.
         */
        public string Title => "Temperature";
        public double Ti { get; private set; } = UserSaved.Ti;
        public double Te { get; private set; } = UserSaved.Te;
        public double Rsi { get; private set; } = UserSaved.Rsi;
        public double Rse { get; private set; } = UserSaved.Rse;
        public ISeries[] DataPoints => GetDataPoints();
        public RectangularSection[] LayerSections => DrawLayerSections();
        public Axis[] XAxes => DrawXAxes();
        public Axis[] YAxes => DrawYAxes();
        public SolidColorPaint TooltipBackgroundPaint { get; private set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; private set; } = new SolidColorPaint { Color = new SKColor(0, 0, 0), SKTypeface = SKTypeface.FromFamilyName("SegoeUI") };

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
        private void EditElement(Element? selectedElement) // Binding in XAML via 'EditElementCommand'
        {
            // Set the currently selected Element
            UserSaved.SelectedElementId = selectedElement.ElementId;

            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new EditElementWindow();
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            CurrentElement = UserSaved.SelectedElement;
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RequirementValues))]
        [NotifyPropertyChangedFor(nameof(OverviewItems))]
        private Element _currentElement = UserSaved.SelectedElement;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the elementType Value above
         */

        public CheckRequirements RequirementValues
        {
            get
            {
                if (!_tempCalc.IsValid) return new CheckRequirements(CurrentElement, 0, 0); // TODO: make empty ctor for CheckRequirements
                return new CheckRequirements(CurrentElement, _tempCalc.UValue, _tempCalc.QValue);
            }
        }

        public List<OverviewItem> OverviewItems
        {
            get
            {
                if (!_tempCalc.IsValid) return new List<OverviewItem>();
                return new List<OverviewItem>
                {
                    new OverviewItem { SymbolBase = "R", SymbolSubscript = "ges", Value = _tempCalc.Element.RValue, RequirementValue = RequirementValues.R_min, IsRequirementMet = RequirementValues.IsRValueOk, Unit = "m²K/W" },
                    new OverviewItem { SymbolBase = "R", SymbolSubscript = "T", Value = _tempCalc.RTotal, RequirementValue = null, IsRequirementMet = RequirementValues.IsRValueOk, Unit = "m²K/W" },
                    new OverviewItem { SymbolBase = "U", SymbolSubscript = "", Value = _tempCalc.UValue, RequirementValue = RequirementValues.U_max, IsRequirementMet = RequirementValues.IsUValueOk, Unit = "W/m²K" },
                    new OverviewItem { SymbolBase = "q", SymbolSubscript = "", Value = _tempCalc.QValue, RequirementValue = RequirementValues.Q_max, IsRequirementMet = RequirementValues.IsQValueOk, Unit = "W/m²" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "si", Value = _tempCalc.Tsi, RequirementValue = _tempCalc.Tsi_min, IsRequirementMet = _tempCalc.Tsi >= _tempCalc.Tsi_min, Unit = "°C" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "se", Value = _tempCalc.Tse, RequirementValue = null, IsRequirementMet = true, Unit = "°C" },
                    new OverviewItem { SymbolBase = "f", SymbolSubscript = "Rsi", Value = _tempCalc.FRsi, RequirementValue = 0.7, IsRequirementMet = _tempCalc.FRsi >= 0.7 },
                };
            }
        }

        /*
         * private Methods
         */

        private RectangularSection[] DrawLayerSections()
        {
            if (!_tempCalc.IsValid) return Array.Empty<RectangularSection>();

            RectangularSection[] rects = new RectangularSection[_tempCalc.Element.Layers.Count];

            double left = 0;
            foreach (Layer layer in _tempCalc.Element.Layers)
            {
                double layerWidth = layer.LayerThickness;
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
        private ISeries[] GetDataPoints()
        {
            if (!_tempCalc.IsValid) return Array.Empty<ISeries>();

            double tsi_Pos = _tempCalc.LayerTemps.First().Key;
            double tsi = _tempCalc.LayerTemps.First().Value;
            double deltaTi = Math.Abs(Ti - tsi);

            LineSeries<ObservablePoint> rsiCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tsi_Pos-0.8, tsi+0.9*deltaTi),
                    new ObservablePoint(), // null cuts the line between the points // TODO check if null works
                    new ObservablePoint(tsi_Pos, tsi),
                    new ObservablePoint(tsi_Pos-0.8, tsi+0.9*deltaTi),
                    new ObservablePoint(tsi_Pos-2, Ti)
                },
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                XToolTipLabelFormatter = null,
                YToolTipLabelFormatter = null
            };

            ObservablePoint[] tempValues = new ObservablePoint[_tempCalc.LayerTemps.Count]; // represents the temperature points
            for (int i = 0; i < _tempCalc.LayerTemps.Count; i++)
            {
                double x = _tempCalc.LayerTemps.ElementAt(i).Key; // Position in cm
                double y = Math.Round(_tempCalc.LayerTemps.ElementAt(i).Value, 2); // Temperature in °C
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

            double tse_Pos = _tempCalc.LayerTemps.Last().Key;
            double tse = _tempCalc.LayerTemps.Last().Value;
            double deltaTe = Math.Abs(Te - tse);
            LineSeries<ObservablePoint> rseCurveSeries = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tse_Pos+2, Te),
                    new ObservablePoint(tse_Pos+0.8, tse-0.9*deltaTe),
                    new ObservablePoint(tse_Pos, tse),
                    new ObservablePoint(), // null cuts the line between the points // TODO check if null works
                    new ObservablePoint(tse_Pos+0.8, tse+0.9*deltaTe),
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
            if (!_tempCalc.IsValid) return Array.Empty<Axis>();
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
                    MaxLimit = _tempCalc.Element.Thickness_cm + 2
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
