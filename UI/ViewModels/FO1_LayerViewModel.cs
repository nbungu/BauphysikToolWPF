using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using BauphysikToolWPF.UI.Helper;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_SetupLayer.xaml: Used in xaml as "DataContext"
    public partial class FO1_LayerViewModel : ObservableObject
    {
        public FO1_LayerViewModel()
        {
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedElementChanged += RefreshXamlBindings;
            UserSaved.SelectedElementChanged += RefreshLayers;
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
        private void EditLayer(int selectedLayerId)
        {
            UserSaved.SelectedLayerId = selectedLayerId;

            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditLayerWindow().ShowDialog();
        }

        [RelayCommand]
        private void DeleteLayer(int selectedLayerId)
        {
            UserSaved.SelectedLayerId = selectedLayerId;

            // Delete selected Layer
            UserSaved.SelectedElement.Layers.Remove(UserSaved.SelectedLayer);

            RefreshLayers();
            RefreshXamlBindings();
        }
        
        [RelayCommand]
        private void DuplicateLayer(int selectedLayerId)
        {
            UserSaved.SelectedLayerId = selectedLayerId;

            var copy = UserSaved.SelectedLayer.Copy();
            copy.LayerPosition = UserSaved.SelectedElement.Layers.Count;
            copy.InternalId = UserSaved.SelectedElement.Layers.Count;
            UserSaved.SelectedElement.Layers.Add(copy);

            RefreshLayers();
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void MoveLayerDown(int selectedLayerId)
        {
            UserSaved.SelectedLayerId = selectedLayerId;

            // When Layer is already at the bottom of the List (last in the List)
            if (UserSaved.SelectedLayer.LayerPosition == UserSaved.SelectedElement.Layers.Count - 1) return;

            // Change Position of Layer below
            Layer neighbour = UserSaved.SelectedElement.Layers.Find(e => e.LayerPosition == UserSaved.SelectedLayer.LayerPosition + 1);
            neighbour.LayerPosition -= 1;
            // Change Position of selected Layer
            UserSaved.SelectedLayer.LayerPosition += 1;

            RefreshLayers();
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void MoveLayerUp(int selectedLayerId)
        {
            UserSaved.SelectedLayerId = selectedLayerId;

            // When Layer is already at the top of the List (first in the List)
            if (UserSaved.SelectedLayer.LayerPosition == 0) return;

            // Change Position of Layer above
            Layer neighbour = UserSaved.SelectedElement.Layers.Find(e => e.LayerPosition == UserSaved.SelectedLayer.LayerPosition - 1);
            neighbour.LayerPosition += 1;
            // Change Position of selected Layer
            UserSaved.SelectedLayer.LayerPosition -= 1;

            RefreshLayers();
            RefreshXamlBindings();
        }

        private void RefreshLayers()
        {
            // Always in sorted order
            UserSaved.SelectedElement.SortLayers();
            // Update Effective Layer Property
            UserSaved.SelectedElement.AssignEffectiveLayers();
            // GUI Stuff
            UserSaved.SelectedElement.ScaleAndStackLayers();
        }
        
        private void RefreshXamlBindings()
        {
            Layers = new List<Layer>();
            Layers = UserSaved.SelectedElement.Layers;
            SelectedLayerIndex = -1;
            SelectedLayerIndex = UserSaved.SelectedLayer?.LayerPosition ?? -1;
            SelectedElement = new Element();
            SelectedElement = UserSaved.SelectedElement;
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private List<Layer> _layers = UserSaved.SelectedElement.Layers;

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        [ObservableProperty]
        private int _selectedLayerIndex = -1;

        // Using a Single-Item Collection, since ItemsSource of XAML Element expects IEnumerable iface
        [ObservableProperty]
        private List<MeasurementChain> _measurementChain = new List<MeasurementChain>(){ new MeasurementChain(UserSaved.SelectedElement.Layers) };

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by 'layers' above
         */

        /*public List<LayerGeometry> LayerGeometries
        {
            get
            {
                var geometries = new List<LayerGeometry>();
                UserSaved.SelectedElement.Layers.ForEach(l => geometries.Add(new LayerGeometry(l)));
                return geometries.ScaleAndStack(320, 400);
            }
        }*/
    }
}
