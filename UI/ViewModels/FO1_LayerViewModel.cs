using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using BauphysikToolWPF.UI.Drawing;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_SetupLayer.xaml: Used in xaml as "DataContext"
    public partial class FO1_LayerViewModel : ObservableObject
    {
        private MeasurementChain _measurementDrawer = new MeasurementChain();
        
        public FO1_LayerViewModel()
        {
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedElementChanged += RefreshLayerProperties;
            UserSaved.SelectedElementChanged += RefreshXamlBindings;
        }

        // Called by 'InitializeComponent()' from FO1_SetupLayer.cs due to Class-Binding in xaml via DataContext
        public string Title => "SetupLayer";
        
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
        private void DeleteSubConstructionLayer()
        {
            UserSaved.SelectedLayer.RemoveSubConstruction();

            RefreshLayerProperties();
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void EditLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditLayerWindow().ShowDialog();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            // Delete selected Layer
            UserSaved.SelectedElement.Layers.Remove(UserSaved.SelectedLayer);

            RefreshLayerProperties();
            RefreshXamlBindings();
        }
        
        [RelayCommand]
        private void DuplicateLayer()
        {
            var copy = UserSaved.SelectedLayer.Copy();
            copy.LayerPosition = UserSaved.SelectedElement.Layers.Count;
            copy.InternalId = UserSaved.SelectedElement.Layers.Count;
            UserSaved.SelectedElement.Layers.Add(copy);

            RefreshLayerProperties();
            RefreshXamlBindings();
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

            RefreshLayerProperties();
            RefreshXamlBindings();
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

            RefreshLayerProperties();
            RefreshXamlBindings();
            SelectedListViewItem = UserSaved.SelectedLayer;
        }

        private void RefreshLayerProperties()
        {
            // Always in sorted order
            UserSaved.SelectedElement.SortLayers();
            // Update Effective Layer Property
            UserSaved.SelectedElement.AssignEffectiveLayers();
        }
        
        private void RefreshXamlBindings()
        {
            LayerList = new List<Layer>();
            LayerList = UserSaved.SelectedElement.Layers;
            SelectedElement = new Element();
            SelectedElement = UserSaved.SelectedElement;
        }

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Layer value)
        {
            if (value is null) return;
            UserSaved.SelectedLayerId = value.InternalId;
        }

        /*
         * MVVM Properties: Observable, if user can change these properties via frontend directly
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ElementProperties))]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(MeasurementChain))]
        [NotifyPropertyChangedFor(nameof(MeasurementChainFull))]
        [NotifyPropertyChangedFor(nameof(DrawingGeometries))]
        private List<Layer> _layerList = UserSaved.SelectedElement.Layers;

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        private Layer _selectedListViewItem;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, No direct User Input involved
         */

        public List<DrawingGeometry> DrawingGeometries => UserSaved.SelectedElement.GetLayerDrawings();

        // Using a Single-Item Collection, since ItemsSource of XAML Element expects IEnumerable iface
        public List<MeasurementChain> MeasurementChain => new MeasurementChain(UserSaved.SelectedElement.Layers).ToList();

        // Using a Single-Item Collection, since ItemsSource of XAML Element expects IEnumerable iface
        public List<MeasurementChain> MeasurementChainFull => UserSaved.SelectedElement.Layers.Count > 1 ? new MeasurementChain(new[] { 400.0 }, new[] { UserSaved.SelectedElement.ElementWidth }).ToList() : new List<MeasurementChain>();
        
        public List<PropertyItem> ElementProperties => new List<PropertyItem>
        {
            new PropertyItem(Symbol.Thickness, UserSaved.SelectedElement.Thickness_cm) { IsReadonly = false },
            new PropertyItem(Symbol.RValueElement, UserSaved.SelectedElement.RValue),
            new PropertyItem(Symbol.AreaMassDensity, UserSaved.SelectedElement.AreaMassDens),
            new PropertyItem(Symbol.SdThickness, UserSaved.SelectedElement.SdThickness),
        };

        public List<PropertyItem> LayerProperties => new List<PropertyItem>
        {
            new PropertyItem(Symbol.Thickness, UserSaved.SelectedLayer.Thickness),
            new PropertyItem(Symbol.ThermalConductivity, UserSaved.SelectedLayer.Material.ThermalConductivity),
            new PropertyItem(Symbol.RawDensity, UserSaved.SelectedLayer.Material.BulkDensity),
            new PropertyItem(Symbol.SpecificHeatCapacity, UserSaved.SelectedLayer.Material.SpecificHeatCapacity),
            new PropertyItem(Symbol.RValueLayer, UserSaved.SelectedLayer.R_Value)
            {
                SymbolSubscriptText = UserSaved.SelectedLayer.LayerPosition.ToString()
            },
            new PropertyItem(Symbol.AreaMassDensity, UserSaved.SelectedLayer.AreaMassDensity),
            new PropertyItem(Symbol.SdThickness, UserSaved.SelectedLayer.Sd_Thickness),
            new PropertyItem(Symbol.None, UserSaved.SelectedLayer.IsEffective),
        };
    }
}
