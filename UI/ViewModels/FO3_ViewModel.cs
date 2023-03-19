using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
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

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class FO3_ViewModel : ObservableObject
    {
        public string Title { get; } = "Moisture";

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

        // TODO: Rework as MVVM

        public GlaserCalc Glaser { get; set; } = FO3_Moisture.GlaserCalculation;
        public RectangularSection[] LayerSections { get; set; }
        public ISeries[] DataPoints { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint TooltipBackgroundPaint { get; set; }
        public SolidColorPaint TooltipTextPaint { get; set; }

        // Called by 'InitializeComponent()' from FO3_Moisture.cs due to Class-Binding in xaml via DataContext
        public FO3_ViewModel()
        {
            // For Drawing the Chart
            this.LayerSections = DrawLayerSections();
            this.DataPoints = DrawGlaserCurvePoints();
            this.XAxes = DrawXAxes();
            this.YAxes = DrawYAxes();
            this.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(255, 255, 255));
            this.TooltipTextPaint = new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0),
                SKTypeface = SKTypeface.FromFamilyName("SegoeUI"),
            };
        }
        private RectangularSection[] DrawLayerSections()
        {
            if (Glaser.Element.Layers.Count == 0)
                return new RectangularSection[0];

            RectangularSection[] rects = new RectangularSection[Glaser.Element.Layers.Count];

            //TODO: Round values of left and right
            double left = 0;
            foreach (Layer layer in Glaser.Element.Layers)
            {
                double layerWidth = layer.Sd_Thickness;
                double right = left + layerWidth; // start drawing from left side (beginning with INSIDE Layer, which is first list element)

                // Set properties of the layer rectangle at the desired position
                rects[layer.LayerPosition] = new RectangularSection
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
                left = right; // Add new layer at left edge of previous layer
            }
            return rects;
        }
        private ISeries[] DrawGlaserCurvePoints()
        {
            if (Glaser.Element.Layers.Count == 0)
                return new ISeries[0];

            ISeries[] series = new ISeries[2];

            ObservablePoint[] p_Curve_Values = new ObservablePoint[Glaser.LayerP.Count()]; // represents the temperature points
            for (int i = 0; i < Glaser.LayerP.Count(); i++)
            {
                double x = Glaser.LayerP.ElementAt(i).Key; // Position in cm
                double y = Math.Round(Glaser.LayerP.ElementAt(i).Value, 2); // Temperature in °C
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
            ObservablePoint[] p_sat_Curve_Values = new ObservablePoint[Glaser.LayerPsat.Count()]; // represents the temperature points
            for (int i = 0; i < Glaser.LayerPsat.Count(); i++)
            {
                double x = Glaser.LayerPsat.ElementAt(i).Key; // Position in cm
                double y = Math.Round(Glaser.LayerPsat.ElementAt(i).Value, 2); // Temperature in °C
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
