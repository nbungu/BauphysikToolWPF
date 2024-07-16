using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_LayerSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_LayerSetup_VM : ObservableObject
    {
        public static string Title = "SetupLayer";

        private readonly CanvasDrawingService _drawingService = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 880, 400));

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM()
        {
            UserSaved.SelectedLayerId = -1;
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            //UserSaved.SelectedElementChanged += RefreshLayerProperties;
            UserSaved.SelectedElementChanged += RefreshXamlBindings;

            //UserSaved.SelectedLayerChanged += RefreshLayerProperties;
            UserSaved.SelectedLayerChanged += RefreshXamlBindings;

            // For values changed in PropertyDataGrid TextBox
            PropertyItem<double>.PropertyChanged += RefreshXamlBindings;
            PropertyItem<SubConstructionDirection>.PropertyChanged += RefreshXamlBindings;
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
            new EditElementWindow().ShowDialog();
        }

        [RelayCommand]
        private void AddLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerWindow().ShowDialog();
        }

        [RelayCommand]
        private void AddSubConstructionLayer()
        {
           
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerSubConstructionWindow().ShowDialog();
        }

        [RelayCommand]
        private void EditLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditLayerWindow().ShowDialog();
        }

        [RelayCommand]
        private void DeleteSubConstructionLayer()
        {
            UserSaved.SelectedLayer.RemoveSubConstruction();
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            UserSaved.SelectedElement.RemoveLayer(UserSaved.SelectedLayerId);
            RefreshXamlBindings();
        }
        
        [RelayCommand]
        private void DuplicateLayer()
        {
            UserSaved.SelectedElement.DuplicateLayer(UserSaved.SelectedLayerId);
            RefreshXamlBindings();
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            UserSaved.SelectedElement.MoveLayerPositionToOutside(UserSaved.SelectedLayerId);
            RefreshXamlBindings();
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            UserSaved.SelectedElement.MoveLayerPositionToInside(UserSaved.SelectedLayerId);
            RefreshXamlBindings();
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        private void RefreshLayerProperties()
        {
            UserSaved.SelectedElement.SortLayers(); // Always in sorted order
            UserSaved.SelectedElement.AssignEffectiveLayers(); // Update Effective Layer Property
        }
        
        private void RefreshXamlBindings()
        {
            _drawingService.UpdateDrawings();
            
            LayerList = null;
            LayerList = UserSaved.SelectedElement.Layers;
            SelectedElement = null;
            SelectedElement = UserSaved.SelectedElement;
            SelectedListViewItem = null;
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Layer value)
        {
            if (value is null) return;
            UserSaved.SelectedElement.Layers.ForEach(l => l.IsSelected = false);
            UserSaved.SelectedLayerId = value.InternalId;
            UserSaved.SelectedLayer.IsSelected = true;

            _drawingService.UpdateDrawings();
        }

        /*
         * MVVM Properties: Observable, if user can change these properties via frontend directly
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ElementProperties))]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(LayerMeasurement))]
        [NotifyPropertyChangedFor(nameof(LayerMeasurementFull))]
        [NotifyPropertyChangedFor(nameof(SubConstructionMeasurement))]
        [NotifyPropertyChangedFor(nameof(DrawingGeometries))]
        [NotifyPropertyChangedFor(nameof(CanvasSize))]
        private List<Layer> _layerList = UserSaved.SelectedElement.Layers;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLayerSelected))]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(ShowLayerExpander))]
        [NotifyPropertyChangedFor(nameof(ShowSubConstructionExpander))]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        [NotifyPropertyChangedFor(nameof(LayerTitle))]
        [NotifyPropertyChangedFor(nameof(DrawingGeometries))]
        private Layer _selectedListViewItem;
        
        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, No direct User Input involved
         */

        public bool IsLayerSelected => SelectedListViewItem != null;
        public bool ShowLayerExpander => IsLayerSelected;
        public bool ShowSubConstructionExpander => IsLayerSelected && SelectedListViewItem.HasSubConstruction;
        public bool ShowElementExpander => LayerList.Count > 0;
        public string LayerTitle => $"Schicht {UserSaved.SelectedLayer.LayerPosition}";
        public List<IDrawingGeometry> DrawingGeometries => _drawingService.DrawingGeometries;
        public Rectangle CanvasSize => _drawingService.CanvasSize;
        public List<DrawingGeometry> LayerMeasurement => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers).ToList();
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementChain.GetMeasurementChain(_drawingService.DrawingGeometries.Where(g => g.ZIndex == 1), Axis.X).ToList();
        public List<DrawingGeometry> LayerMeasurementFull => UserSaved.SelectedElement.Layers.Count > 1 ? MeasurementChain.GetMeasurementChain(new[] {0, 400.0 }).ToList() : new List<DrawingGeometry>();

        public List<IPropertyItem> LayerProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => UserSaved.SelectedLayer.Material.Name),
            new PropertyItem<string>("Kategorie", () => UserSaved.SelectedLayer.Material.CategoryName),
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedLayer.Thickness, value => UserSaved.SelectedLayer.Thickness = value),
            new PropertyItem<double>(Symbol.ThermalConductivity, () => UserSaved.SelectedLayer.Material.ThermalConductivity),
            new PropertyItem<double>(Symbol.RawDensity, () => UserSaved.SelectedLayer.Material.BulkDensity),
            new PropertyItem<double>(Symbol.SpecificHeatCapacity, () => UserSaved.SelectedLayer.Material.SpecificHeatCapacity),
            new PropertyItem<double>(Symbol.RValueLayer, () => UserSaved.SelectedLayer.R_Value)
            {
                SymbolSubscriptText = UserSaved.SelectedLayer.LayerPosition.ToString()
            },
            new PropertyItem<double>(Symbol.AreaMassDensity, () => UserSaved.SelectedLayer.AreaMassDensity),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => UserSaved.SelectedLayer.Material.DiffusionResistance),
            new PropertyItem<double>(Symbol.SdThickness, () => UserSaved.SelectedLayer.Sd_Thickness),
            new PropertyItem<bool>("Wirksame Schicht", () => UserSaved.SelectedLayer.IsEffective)
        };

        public List<IPropertyItem> SubConstructionProperties => UserSaved.SelectedLayer.HasSubConstruction ? new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => UserSaved.SelectedLayer.SubConstruction.Material.Name),
            new PropertyItem<string>("Kategorie", () => UserSaved.SelectedLayer.SubConstruction.Material.CategoryName),
            new PropertyItem<SubConstructionDirection>("Ausrichtung", () => UserSaved.SelectedLayer.SubConstruction.SubConstructionDirection, value => UserSaved.SelectedLayer.SubConstruction.SubConstructionDirection = value)
            {
                PropertyValues = Enum.GetValues(typeof(SubConstructionDirection)).Cast<object>().ToArray(),
            },
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedLayer.SubConstruction.Thickness, value => UserSaved.SelectedLayer.SubConstruction.Thickness = value),
            new PropertyItem<double>(Symbol.Width, () => UserSaved.SelectedLayer.SubConstruction.Width, value => UserSaved.SelectedLayer.SubConstruction.Width = value),
            new PropertyItem<double>(Symbol.Distance, () => UserSaved.SelectedLayer.SubConstruction.Spacing, value => UserSaved.SelectedLayer.SubConstruction.Spacing = value),
            new PropertyItem<double>("Achsenabstand", Symbol.Distance, () => UserSaved.SelectedLayer.SubConstruction.AxisSpacing, value => UserSaved.SelectedLayer.SubConstruction.AxisSpacing = value),
        } : new List<IPropertyItem>();

        public List<IPropertyItem> ElementProperties => new List<IPropertyItem>
        {
            new PropertyItem<int>("Schichten", () => UserSaved.SelectedElement.Layers.Count),
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedElement.Thickness),
            new PropertyItem<double>(Symbol.RValueElement, () => UserSaved.SelectedElement.RValue),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => UserSaved.SelectedElement.AreaMassDens),
            new PropertyItem<double>(Symbol.SdThickness, () => UserSaved.SelectedElement.SdThickness),
        };
    }
}
