﻿using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
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

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO2_Temperature.cs: Used in xaml as "DataContext"
    public partial class FO2_ViewModel : ObservableObject
    {
        /*
         * Regular Instance Variables as Properties
         * 
         * Not depending on UI changes. No Observable function.
         */
        public string Title { get; } = "Temperature";
        public StationaryTempCalc TempCalc { get; private set; } = FO2_Temperature.StationaryTempCalculation;
        public double Ti { get; private set; } = UserSaved.Ti;
        public double Te { get; private set; } = UserSaved.Te;
        public double Rsi { get; private set; } = UserSaved.Rsi;
        public double Rse { get; private set; } = UserSaved.Rse;
        public ISeries[] DataPoints
        {
            get { return GetDataPoints(); }
        }
        public RectangularSection[] LayerSections
        {
            get { return DrawLayerSections(); }
        }
        public Axis[] XAxes
        {
            get { return DrawXAxes(); }
        }
        public Axis[] YAxes
        {
            get { return DrawYAxes(); }
        }
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
            selectedElement ??= DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId);

            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new EditElementWindow(selectedElement);
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            CurrentElement = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RequirementValues))]
        [NotifyPropertyChangedFor(nameof(OverviewItems))]
        private Element currentElement = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId);

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the elementType Value above
         */

        public CheckRequirements RequirementValues
        {
            get { return new CheckRequirements(CurrentElement); }
        }

        public List<OverviewItem> OverviewItems
        {
            get
            {
                return new List<OverviewItem>
                {
                    new OverviewItem { SymbolBase = "R", SymbolSubscript = "ges", Value = TempCalc.Element.RValue, RequirementValue = RequirementValues.R_min, IsRequirementMet = RequirementValues.IsRValueOK, Unit = "m²K/W" },
                    new OverviewItem { SymbolBase = "R", SymbolSubscript = "T", Value = TempCalc.RTotal, RequirementValue = null, IsRequirementMet = RequirementValues.IsRValueOK, Unit = "m²K/W" },
                    new OverviewItem { SymbolBase = "U", SymbolSubscript = "", Value = TempCalc.UValue, RequirementValue = RequirementValues.U_max, IsRequirementMet = RequirementValues.IsUValueOK, Unit = "W/m²K" },
                    new OverviewItem { SymbolBase = "q", SymbolSubscript = "", Value = TempCalc.QValue, RequirementValue = RequirementValues.Q_max, IsRequirementMet = RequirementValues.IsQValueOK, Unit = "W/m²" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "si", Value = TempCalc.Tsi, RequirementValue = TempCalc.Tsi_min, IsRequirementMet = TempCalc.Tsi >= TempCalc.Tsi_min, Unit = "°C" },
                    new OverviewItem { SymbolBase = "θ", SymbolSubscript = "se", Value = TempCalc.Tse, RequirementValue = null, IsRequirementMet = true, Unit = "°C" },
                    new OverviewItem { SymbolBase = "f", SymbolSubscript = "Rsi", Value = TempCalc.FRsi, RequirementValue = 0.7, IsRequirementMet = TempCalc.FRsi >= 0.7 },
                };
            }
        }

        /*
         * private Methods
         */

        private RectangularSection[] DrawLayerSections()
        {
            if (TempCalc.Element.Layers.Count == 0)
                return Array.Empty<RectangularSection>();

            RectangularSection[] rects = new RectangularSection[TempCalc.Element.Layers.Count];

            double left = 0;
            foreach (Layer layer in TempCalc.Element.Layers)
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
            //fRsi frsi = new fRsi(TempCalc.LayerTemps.First().Value, Temperatures.selectedTi.First().Value, Temperatures.selectedTe.First().Value);
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
            if (TempCalc.Element.Layers.Count == 0)
                return Array.Empty<ISeries>();

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

            ObservablePoint[] tempValues = new ObservablePoint[TempCalc.LayerTemps.Count]; // represents the temperature points
            for (int i = 0; i < TempCalc.LayerTemps.Count; i++)
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
            return new ISeries[] { rsiCurveSeries, tempCurveSeries, rseCurveSeries };
        }
        private Axis[] DrawXAxes()
        {
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
                    MaxLimit = TempCalc.Element.Thickness_cm + 2
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
