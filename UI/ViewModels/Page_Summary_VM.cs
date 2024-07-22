using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for Page_Summary.xaml: Used in xaml as "DataContext"
    public partial class Page_Summary_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public string Title = "Zusammenfassung";

        private readonly CanvasDrawingService _drawingServiceV = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 400, 880), DrawingType.VerticalCut);
        private readonly CanvasDrawingService _drawingServiceH = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);

        /*
         * Static Class Properties:
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */


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
        
        public List<IDrawingGeometry> CrossSectionDrawing => _drawingServiceH.DrawingGeometries;
        public Rectangle CanvasSizeCrossSection => _drawingServiceH.CanvasSize;
        public List<DrawingGeometry> LayerMeasurementCrossSection => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers, Axis.Z).ToList();
        public List<DrawingGeometry> SubConstructionMeasurementCrossSection => MeasurementChain.GetMeasurementChain(_drawingServiceH.DrawingGeometries.Where(g => g.ZIndex == 1), Axis.X).ToList();
        public List<DrawingGeometry> LayerMeasurementFullCrossSection => UserSaved.SelectedElement.Layers.Count > 1 ? MeasurementChain.GetMeasurementChain(new[] { 0, 400.0 }).ToList() : new List<DrawingGeometry>();

        // Vertical Cut
        public List<IDrawingGeometry> VerticalCutDrawing => _drawingServiceV.DrawingGeometries;
        public Rectangle CanvasSizeVerticalCut => _drawingServiceV.CanvasSize;
        public List<DrawingGeometry> LayerMeasurementVerticalCut => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers, Axis.X).ToList();
        public List<DrawingGeometry> SubConstructionMeasurementVerticalCut => MeasurementChain.GetMeasurementChain(_drawingServiceV.DrawingGeometries.Where(g => g.ZIndex == 1), Axis.Z).ToList();
        public List<DrawingGeometry> LayerMeasurementFullVerticalCut => UserSaved.SelectedElement.Layers.Count > 1 ? MeasurementChain.GetMeasurementChain(new[] { 0, 400.0 }, Axis.X).ToList() : new List<DrawingGeometry>();
        


        public List<IPropertyItem> ElementProperties => new List<IPropertyItem>
        {
            new PropertyItem<int>("Schichten", () => UserSaved.SelectedElement.Layers.Count),
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedElement.Thickness),
            new PropertyItem<double>(Symbol.RValueElement, () => UserSaved.SelectedElement.RGesValue),
            new PropertyItem<double>(Symbol.RValueTotal, () => UserSaved.SelectedElement.RTotValue),
            new PropertyItem<double>(Symbol.SdThickness, () => UserSaved.SelectedElement.SdThickness),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => UserSaved.SelectedElement.AreaMassDens),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => UserSaved.SelectedElement.ArealHeatCapacity),
        };

        public List<IPropertyItem> EnvironmentProperties => new List<IPropertyItem>
        {
            new PropertyItem<double>(Symbol.TemperatureInterior, () => UserSaved.Ti),
            new PropertyItem<double>(Symbol.TemperatureExterior, () => UserSaved.Te),
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceInterior, () => UserSaved.Rsi),
            new PropertyItem<double>(Symbol.TransferResistanceSurfaceExterior, () => UserSaved.Rse),
            new PropertyItem<double>(Symbol.RelativeHumidityInterior, () => UserSaved.Rel_Fi),
            new PropertyItem<double>(Symbol.RelativeHumidityExterior, () => UserSaved.Rel_Fe),
        };
    }
}
