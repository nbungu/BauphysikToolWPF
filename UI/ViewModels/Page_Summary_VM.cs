using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for Page_Summary.xaml: Used in xaml as "DataContext"
    public partial class Page_Summary_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext

        private readonly CanvasDrawingService _verticalCut = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 400, 880), DrawingType.VerticalCut);
        private readonly CanvasDrawingService _crossSection = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);

        public Page_Summary_VM()
        {
            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedElementChanged += RefreshXamlBindings;
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
            new AddElementWindow().ShowDialog();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values (int default = 0)
         */

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        // Cross Section
        
        public List<IDrawingGeometry> CrossSectionDrawing => _crossSection.DrawingGeometries;
        public Rectangle CanvasSizeCrossSection => _crossSection.CanvasSize;
        public List<DrawingGeometry> LayerMeasurementCrossSection => MeasurementChain.GetLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> SubConstructionMeasurementCrossSection => MeasurementChain.GetSubConstructionMeasurementChain(_crossSection);
        public List<DrawingGeometry> LayerMeasurementFullCrossSection => MeasurementChain.GetFullLayerMeasurementChain(_crossSection);

        // Vertical Cut
        public List<IDrawingGeometry> VerticalCutDrawing => _verticalCut.DrawingGeometries;
        public Rectangle CanvasSizeVerticalCut => _verticalCut.CanvasSize;
        public List<DrawingGeometry> LayerMeasurementVerticalCut => MeasurementChain.GetLayerMeasurementChain(_verticalCut);
        public List<DrawingGeometry> SubConstructionMeasurementVerticalCut => MeasurementChain.GetSubConstructionMeasurementChain(_verticalCut);
        public List<DrawingGeometry> LayerMeasurementFullVerticalCut => MeasurementChain.GetFullLayerMeasurementChain(_verticalCut);

        public List<IPropertyItem> ElementProperties => new List<IPropertyItem>
        {
            new PropertyItem<int>("Schichten", () => UserSaved.SelectedElement.Layers.Count),
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedElement.Thickness),
            new PropertyItem<double>(Symbol.RValueElement, () => UserSaved.SelectedElement.RGesValue),
            new PropertyItem<double>(Symbol.RValueTotal, () => UserSaved.SelectedElement.RTotValue),
            new PropertyItem<double>(Symbol.UValue, () => UserSaved.SelectedElement.UValue),
            new PropertyItem<double>(Symbol.HeatFluxDensity, () => UserSaved.SelectedElement.QValue),
            new PropertyItem<double>(Symbol.SdThickness, () => UserSaved.SelectedElement.SdThickness),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => UserSaved.SelectedElement.AreaMassDens),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => UserSaved.SelectedElement.ArealHeatCapacity),
        };

        public List<IPropertyItem> EnvironmentProperties => new List<IPropertyItem>
        {
            new PropertyItem<double>(Symbol.TemperatureInterior, () => UserSaved.Ti) { DecimalPlaces = 1},
            new PropertyItem<double>(Symbol.TemperatureExterior, () => UserSaved.Te) { DecimalPlaces = 1},
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceInterior, () => UserSaved.Rsi),
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceExterior, () => UserSaved.Rse),
            new PropertyItem<double>(Symbol.RelativeHumidityInterior, () => UserSaved.Rel_Fi) { DecimalPlaces = 1},
            new PropertyItem<double>(Symbol.RelativeHumidityExterior, () => UserSaved.Rel_Fe) { DecimalPlaces = 1},
        };

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(SelectedElement));
        }
    }
}
