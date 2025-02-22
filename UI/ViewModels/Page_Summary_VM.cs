﻿using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Models;
using BauphysikToolWPF.UI.Services;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for Page_Summary.xaml: Used in xaml as "DataContext"
    public partial class Page_Summary_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext

        private readonly CrossSectionDrawing _verticalCut = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 400, 880), DrawingType.VerticalCut);
        private readonly CrossSectionDrawing _crossSection = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);

        public Page_Summary_VM()
        {
            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedElementChanged += RefreshXamlBindings;
        }

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
            new AddElementWindow(editExsiting: true).ShowDialog();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values (int default = 0)
         */

        [ObservableProperty]
        private Element _selectedElement = Session.SelectedElement;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

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

        public List<IPropertyItem> ElementProperties => new List<IPropertyItem>
        {
            new PropertyItem<int>("Schichten", () => Session.SelectedElement.Layers.Count),
            new PropertyItem<double>(Symbol.Thickness, () => Session.SelectedElement.Thickness),
            new PropertyItem<double>(Symbol.RValueElement, () => Session.SelectedElement.RGesValue),
            new PropertyItem<double>(Symbol.RValueTotal, () => Session.SelectedElement.RTotValue),
            new PropertyItem<double>(Symbol.UValue, () => Session.SelectedElement.UValue),
            new PropertyItem<double>(Symbol.HeatFluxDensity, () => Session.SelectedElement.QValue),
            new PropertyItem<double>(Symbol.SdThickness, () => Session.SelectedElement.SdThickness),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => Session.SelectedElement.AreaMassDens),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => Session.SelectedElement.ArealHeatCapacity),
        };

        public List<IPropertyItem> EnvironmentProperties => new List<IPropertyItem>
        {
            new PropertyItem<double>(Symbol.TemperatureInterior, () => Session.Ti) { DecimalPlaces = 1},
            new PropertyItem<double>(Symbol.TemperatureExterior, () => Session.Te) { DecimalPlaces = 1},
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceInterior, () => Session.Rsi),
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceExterior, () => Session.Rse),
            new PropertyItem<double>(Symbol.RelativeHumidityInterior, () => Session.Rel_Fi) { DecimalPlaces = 1},
            new PropertyItem<double>(Symbol.RelativeHumidityExterior, () => Session.Rel_Fe) { DecimalPlaces = 1},
        };

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(SelectedElement));
        }
    }
}
