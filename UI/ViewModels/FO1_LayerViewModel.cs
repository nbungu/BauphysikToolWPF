using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using Geometry;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_SetupLayer.xaml: Used in xaml as "DataContext"
    public partial class FO1_LayerViewModel : ObservableObject
    {
        public FO1_LayerViewModel()
        {
            UserSaved.SelectedLayerId = -1;
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedElementChanged += RefreshLayerProperties;
            UserSaved.SelectedElementChanged += RefreshXamlBindings;

            UserSaved.SelectedLayerChanged += RefreshLayerProperties;
            UserSaved.SelectedLayerChanged += RefreshXamlBindings;

            // For values changed in PropertyDataGrid TextBox
            PropertyItem<double>.PropertyChanged += RefreshXamlBindings;
        }

        // Called by 'InitializeComponent()' from FO1_SetupLayer.cs due to Class-Binding in xaml via DataContext
        public static string Title = "SetupLayer";

        private static CanvasDrawingService _drawing = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 880, 400));


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

            UserSaved.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            // Delete selected Layer
            UserSaved.SelectedElement.Layers.Remove(UserSaved.SelectedLayer);

            UserSaved.OnSelectedElementChanged();
        }
        
        [RelayCommand]
        private void DuplicateLayer()
        {
            var copy = UserSaved.SelectedLayer.Copy();
            copy.LayerPosition = UserSaved.SelectedElement.Layers.Count;
            copy.InternalId = UserSaved.SelectedElement.Layers.Count;
            UserSaved.SelectedElement.Layers.Add(copy);

            UserSaved.OnSelectedElementChanged();
            SelectedListViewItem = copy;
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            // When Layer is already at the bottom of the List (last in the List)
            if (UserSaved.SelectedLayer.LayerPosition == UserSaved.SelectedElement.Layers.Count - 1) return;

            // Change Position of Layer below
            Layer neighbour = UserSaved.SelectedElement.Layers.Find(e => e.LayerPosition == UserSaved.SelectedLayer.LayerPosition + 1);
            neighbour.LayerPosition -= 1;
            // Change Position of selected Layer
            UserSaved.SelectedLayer.LayerPosition += 1;

            UserSaved.OnSelectedLayerChanged();
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            // When Layer is already at the top of the List (first in the List)
            if (UserSaved.SelectedLayer.LayerPosition == 0) return;

            // Change Positions
            Layer neighbour = UserSaved.SelectedElement.Layers.Find(e => e.LayerPosition == UserSaved.SelectedLayer.LayerPosition - 1);
            neighbour.LayerPosition += 1;
            UserSaved.SelectedLayer.LayerPosition -= 1;

            UserSaved.OnSelectedLayerChanged();
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        private void RefreshLayerProperties()
        {
            UserSaved.SelectedElement.SortLayers(); // Always in sorted order
            UserSaved.SelectedElement.AssignEffectiveLayers(); // Update Effective Layer Property
        }
        
        private void RefreshXamlBindings()
        {
            _drawing.UpdateCanvasSize();

            LayerList = new List<Layer>();
            LayerList = UserSaved.SelectedElement.Layers;
            //SelectedElement = new Element();
            //SelectedElement = UserSaved.SelectedElement;
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
        }

        /*
         * MVVM Properties: Observable, if user can change these properties via frontend directly
         * 
         * Initialized and Assigned with Default Values
         */

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
        private Element _selectedElement = UserSaved.SelectedElement;

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
        public string LayerTitle => string.Format("Schicht {0}", UserSaved.SelectedLayer.LayerPosition);

        public List<IDrawingGeometry> DrawingGeometries => _drawing.DrawingGeometries;
        public Rectangle CanvasSize => _drawing.CanvasSize;

        //public Rectangle CanvasSize { get; set; } = new Rectangle(new Point(0, 0), 880, 400);
        //public List<IDrawingGeometry> DrawingGeometries => UserSaved.SelectedElement.GetCrossSectionDrawing(CanvasSize);

        // Using a Single-Item Collection, since ItemsSource of XAML Element expects IEnumerable iface
        public List<DrawingGeometry> LayerMeasurement => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers).ToList();
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementChain.GetMeasurementChain(_drawing.DrawingGeometries.Where(g => g.ZIndex == 1), Axis.X).ToList();
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
