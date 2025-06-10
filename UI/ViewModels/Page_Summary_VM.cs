using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows;
using static BauphysikToolWPF.Models.UI.Enums;
using Point = BT.Geometry.Point;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for Page_Summary.xaml: Used in xaml as "DataContext"
    public partial class Page_Summary_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext

        private readonly CrossSectionDrawing _verticalCut = new CrossSectionDrawing();
        private readonly CrossSectionDrawing _crossSection = new CrossSectionDrawing();
        private readonly CheckRequirements _requirementValues = new CheckRequirements();

        public Page_Summary_VM()
        {
            if (Session.SelectedProject is null) return;
            if (Session.SelectedElement is null) return;

            _verticalCut = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 400, 880), DrawingType.VerticalCut);
            _crossSection = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);

            // TODO: this could be 'Element' property and be fetched from the SelectedElement directly
            // -> just set Update flag to true
            _requirementValues = new CheckRequirements(Session.SelectedElement, Session.CheckRequirementsConfig);
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
         * Initialized and Assigned with Default Values (int default = 0)
         */


        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public string Title { get; } = Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Zusammenfassung " : "";
        public string SelectedElementColorCode { get; } = Session.SelectedElement?.ColorCode ?? string.Empty;
        public string SelectedElementConstructionName { get; } = Session.SelectedElement?.Construction.TypeName ?? string.Empty;
        public Visibility NoLayersVisibility { get; } = Session.SelectedElement?.Layers.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        // Cross Section

        public List<IDrawingGeometry> CrossSectionDrawing => _crossSection.DrawingGeometries;
        public Rectangle CanvasSizeCrossSection => _crossSection.CanvasSize;
        public List<DrawingGeometry> LayerMeasurementCrossSection => MeasurementDrawing.GetLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> SubConstructionMeasurementCrossSection => MeasurementDrawing.GetSubConstructionMeasurementChain(_crossSection);
        public List<DrawingGeometry> LayerMeasurementFullCrossSection => MeasurementDrawing.GetFullLayerMeasurementChain(_crossSection);

        // Vertical Cut
        public List<IDrawingGeometry> VerticalCutDrawing => _verticalCut.DrawingGeometries;
        public Rectangle CanvasSizeVerticalCut => _verticalCut.CanvasSize;
        public List<DrawingGeometry> LayerMeasurementVerticalCut => MeasurementDrawing.GetLayerMeasurementChain(_verticalCut);
        public List<DrawingGeometry> SubConstructionMeasurementVerticalCut => MeasurementDrawing.GetSubConstructionMeasurementChain(_verticalCut);
        public List<DrawingGeometry> LayerMeasurementFullVerticalCut => MeasurementDrawing.GetFullLayerMeasurementChain(_verticalCut);

        // Results

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

        public List<IPropertyItem> ElementProperties => Session.SelectedElement != null ? new List<IPropertyItem>
        {
            new PropertyItem<int>("Schichten", () => Session.SelectedElement.Layers.Count),
            new PropertyItem<double>(Symbol.Thickness, () => Session.SelectedElement.Thickness),
            new PropertyItem<double>(Symbol.RValueElement, () => Session.SelectedElement.RGesValue),
            new PropertyItem<double>(Symbol.RValueTotal, () => Session.SelectedElement.RTotValue),
            new PropertyItem<double>(Symbol.UValue, () => Session.SelectedElement.UValue) { DecimalPlaces = 3 },
            new PropertyItem<double>(Symbol.HeatFluxDensity, () => Session.SelectedElement.QValue),
            new PropertyItem<double>(Symbol.SdThickness, () => Session.SelectedElement.SdThickness) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.AreaMassDensity, () => Session.SelectedElement.AreaMassDens),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => Session.SelectedElement.ArealHeatCapacity),
        } : new List<IPropertyItem>(0);

        public List<IPropertyItem> EnvironmentProperties => new List<IPropertyItem>
        {
            new PropertyItem<double>(Symbol.TemperatureInterior, () => Session.Ti) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.TemperatureExterior, () => Session.Te) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceInterior, () => Session.Rsi),
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceExterior, () => Session.Rse),
            new PropertyItem<double>(Symbol.RelativeHumidityInterior, () => Session.RelFi) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.RelativeHumidityExterior, () => Session.RelFe) { DecimalPlaces = 1 },
        };
    }
}
