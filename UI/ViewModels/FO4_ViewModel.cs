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

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class FO4_ViewModel : ObservableObject
    {
        public string Title { get; } = "Dynamic";
        public DynamicTempCalc DynamicTempCalc { get; private set; } = FO4_Dynamic.DynamicTempCalculation;
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

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the elementType Value above
         */

        public List<OverviewItem> DynamicThermalValues
        {
            get
            {
                List<OverviewItem> list = new List<OverviewItem>
                {
                    new OverviewItem { SymbolBase = "R", SymbolSubscript = "dyn", Value = DynamicTempCalc.DynamicRValue, Unit = "m²K/W" },
                    new OverviewItem { SymbolBase = "U", SymbolSubscript = "dyn", Value = DynamicTempCalc.DynamicUValue, Unit = "W/m²K" },
                    new OverviewItem { SymbolBase = "TAD", SymbolSubscript = "", Value = DynamicTempCalc.TAD },
                    new OverviewItem { SymbolBase = "TAV", SymbolSubscript = "", Value = DynamicTempCalc.TAV },
                    new OverviewItem { SymbolBase = "Δt", SymbolSubscript = "f", Value = DynamicTempCalc.TimeShift, Unit = "h" },
                    new OverviewItem { SymbolBase = "M", SymbolSubscript = "", Value = DynamicTempCalc.EffectiveThermalMass, Unit = "kg/m²" },
                    new OverviewItem { SymbolBase = "f", SymbolSubscript = "", Value = DynamicTempCalc.DecrementFactor }
                };
                return list;
            }
        }

        public List<OverviewItem> DynamicThermalValues_i
        {
            get
            {
                List<OverviewItem> list = new List<OverviewItem>
                {
                    new OverviewItem { SymbolBase = "Δt", SymbolSubscript = "1", Value = DynamicTempCalc.TimeShift_i, Unit = "h" },
                    new OverviewItem { SymbolBase = "Κ", SymbolSubscript = "1", Value = DynamicTempCalc.ArealHeatCapacity_i, Unit = "kJ/(m²K)" },
                    new OverviewItem { SymbolBase = "Y", SymbolSubscript = "1", Value = DynamicTempCalc.ThermalAdmittance_i, Unit = "W/(m²K)" }
                };
                return list;
            }
        }
        public List<OverviewItem> DynamicThermalValues_e
        {
            get
            {
                List<OverviewItem> list = new List<OverviewItem>
                {
                    new OverviewItem { SymbolBase = "Δt", SymbolSubscript = "2", Value = DynamicTempCalc.TimeShift_e, Unit = "h" },
                    new OverviewItem { SymbolBase = "Κ", SymbolSubscript = "2", Value = DynamicTempCalc.ArealHeatCapacity_e, Unit = "kJ/(m²K)" },
                    new OverviewItem { SymbolBase = "Y", SymbolSubscript = "2", Value = DynamicTempCalc.ThermalAdmittance_e, Unit = "W/(m²K)" }
                };
                return list;
            }
        }

        // TODO: Rework as MVVM

        public RectangularSection[] LayerSections { get; private set; }
        public ISeries[] DataPoints { get; private set; }
        public Axis[] XAxes { get; private set; }
        public Axis[] YAxes { get; private set; }
        public SolidColorPaint TooltipBackgroundPaint { get; private set; }
        public SolidColorPaint TooltipTextPaint { get; private set; }

        public FO4_ViewModel() // Called by 'InitializeComponent()' from FO4_Dynamic.cs due to Class-Binding in xaml via DataContext
        {
            // For Drawing the Chart
            this.DataPoints = GetDataPoints();
            this.XAxes = DrawXAxes();
            this.YAxes = DrawYAxes();
            this.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(255, 255, 255));
            this.TooltipTextPaint = new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0),
                SKTypeface = SKTypeface.FromFamilyName("SegoeUI"),
            };
        }

        private ISeries[] GetDataPoints()
        {
            if (DynamicTempCalc.Element.Layers.Count == 0)
                return Array.Empty<ISeries>();

            ISeries[] series = new ISeries[3]; // more than one series possible to draw in the same graph      

            

            LineSeries<ObservablePoint> surfaceTemp_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[1],
                Fill = null,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometrySize = 0,
                TooltipLabelFormatter = null
            };

            ObservablePoint[] surfaceTempValues = new ObservablePoint[TempCalc.LayerTemps.Count]; // represents the temperature points
            for (int i = 0; i < TempCalc.LayerTemps.Count; i++)
            {
                double x = TempCalc.LayerTemps.ElementAt(i).Key; // Position in cm
                double y = Math.Round(TempCalc.LayerTemps.ElementAt(i).Value, 2); // Temperature in °C
                tempValues[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
            }
            // Set properties & add temperature points to the series
            LineSeries<ObservablePoint> airTemp_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
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

            LineSeries<ObservablePoint> surfaceHeatFlux_e = new LineSeries<ObservablePoint> // adds the temperature points to the series
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(tse_Pos+2, UserSaved.Te),
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
            series[0] = surfaceTemp_e;
            series[1] = airTemp_e;
            series[2] = surfaceHeatFlux_e;

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
        }
    }
}
