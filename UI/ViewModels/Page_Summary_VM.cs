using BauphysikToolWPF.Models.Domain;
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
        private readonly Element _element;

        public Page_Summary_VM()
        {
            if (Session.SelectedProject is null) return;
            if (Session.SelectedElement is null) return;

            _element = Session.SelectedElement;

            _verticalCut = new CrossSectionDrawing(_element, new Rectangle(new Point(0, 0), 400, 880), DrawingType.VerticalCut);
            _crossSection = new CrossSectionDrawing(_element, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage) => MainWindow.SetPage(desiredPage);

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

        public string Title => $"'{_element.Name}' - Zusammenfassung ";
        public string SelectedElementColorCode => _element.ColorCode;
        public string SelectedElementConstructionName => _element.Construction.TypeName;
        public Visibility NoLayersVisibility => _element.Layers.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

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
                    double? uMax = _element.Requirements.UMax;
                    double? elementUValue = _element.ThermalResults.IsValid ? _element.UValue : null;
                    double scaleMax = uMax.HasValue
                        ? Math.Max(2 * uMax.Value, (elementUValue ?? 0.0) + 0.1)
                        : Math.Max(1.0, (elementUValue ?? 0.0) + 0.1);

                    _uValueGauge = new GaugeItem(Symbol.UValue, elementUValue, uMax, _element.Requirements.UMaxComparisonRequirement)
                    {
                        Caption = _element.Requirements.UMaxCaption,
                        ScaleMin = 0.0,
                        ScaleMax = scaleMax,
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
                    double? uValueNormalized = (uValueGauge.Value - uValueGauge.ScaleMin) / (uValueGauge.ScaleMax - uValueGauge.ScaleMin);
                    double? targetRValueNormalized = 1.0 - uValueNormalized;
                    double? elementRValue = _element.ThermalResults.IsValid ? _element.RGesValue : null;
                    _rValueGauge = new GaugeItem(Symbol.RValueElement, elementRValue, _element.Requirements.RMin, _element.Requirements.RMinComparisonRequirement)
                    {
                        Caption = _element.Requirements.RMinCaption,
                        ScaleMin = 0.0,
                        ScaleMax = _element.RGesValue / targetRValueNormalized ?? 1.0,
                    };
                }
                return _rValueGauge;
            }
        }

        public List<IPropertyItem> ElementProperties => _element != null ? new List<IPropertyItem>
        {
            new PropertyItem<int>("Schichten", () => _element.Layers.Count),
            new PropertyItem<double>(Symbol.Thickness, () => _element.Thickness),
            new PropertyItem<double>(Symbol.RValueElement, () => _element.RGesValue),
            new PropertyItem<double>(Symbol.RValueTotal, () => _element.RTotValue),
            new PropertyItem<double>(Symbol.UValue, () => _element.UValue) { DecimalPlaces = 3 },
            new PropertyItem<double>(Symbol.HeatFluxDensity, () => _element.QValue),
            new PropertyItem<double>(Symbol.SdThickness, () => _element.SdThickness) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.AreaMassDensity, () => _element.AreaMassDens),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => _element.ArealHeatCapacity),
        } : new List<IPropertyItem>(0);

        public List<IPropertyItem> EnvironmentProperties => new List<IPropertyItem>
        {
            new PropertyItem<double>(Symbol.TemperatureInterior, () => _element.ThermalCalcConfig.Ti) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.TemperatureExterior, () => _element.ThermalCalcConfig.Te) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceInterior, () => _element.ThermalCalcConfig.Rsi),
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceExterior, () => _element.ThermalCalcConfig.Rse),
            new PropertyItem<double>(Symbol.RelativeHumidityInterior, () => _element.ThermalCalcConfig.RelFi) { DecimalPlaces = 1 },
            new PropertyItem<double>(Symbol.RelativeHumidityExterior, () => _element.ThermalCalcConfig.RelFe) { DecimalPlaces = 1 },
        };
    }
}
