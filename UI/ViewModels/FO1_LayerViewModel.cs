﻿using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_SetupLayer.xaml: Used in xaml as "DataContext"
    public partial class FO1_LayerViewModel : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO1_SetupLayer.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "SetupLayer";

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
        private void AddLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new AddLayerWindow();
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            Layers = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId);
        }

        [RelayCommand]
        private void DeleteLayer(Layer? selectedLayer)
        {
            if (selectedLayer is null)
            {
                // If no specific Layer is selected, delete All
                DatabaseAccess.DeleteAllLayers();
            } else
            {
                // Delete selected Layer
                DatabaseAccess.DeleteLayer(selectedLayer);
            }
            // Update XAML Binding Property by fetching from DB
            Layers = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId, removeGaps: true);
        }

        [RelayCommand]
        private void EditLayer(Layer? selectedLayer)
        {
            if (selectedLayer is null)
                return;
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new EditLayerWindow(selectedLayer);
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            Layers = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId);
        }

        [RelayCommand] 
        private void EditElement(Element? selectedElement) // Binding in XAML via 'EditElementCommand'
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

        [RelayCommand]
        private void MoveLayerDown(Layer? selectedLayer)
        {
            if (selectedLayer is null)
                return;

            // When Layer is already at the bottom of the List (last in the List)
            if (selectedLayer.LayerPosition == DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId).Count - 1)
                return;

            // Change Position of Layer below
            Layer neighbour = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId).Where(e => e.LayerPosition == selectedLayer.LayerPosition + 1).First();            
            neighbour.LayerPosition -= 1;
            // Change Position of selected Layer
            selectedLayer.LayerPosition += 1;
            // Update both
            DatabaseAccess.UpdateLayer(selectedLayer, triggerUpdateEvent: false);
            DatabaseAccess.UpdateLayer(neighbour);

            // Update XAML Binding Property by fetching from DB
            Layers = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId);
        }

        [RelayCommand]
        private void MoveLayerUp(Layer? selectedLayer)
        {
            if (selectedLayer is null)
                return;

            // When Layer is already at the top of the List (first in the List)
            if (selectedLayer.LayerPosition == 0)
                return;

            // Change Position of Layer above
            Layer neighbour = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId).Where(e => e.LayerPosition == selectedLayer.LayerPosition - 1).First();
            neighbour.LayerPosition += 1;
            // Change Position of selected Layer
            selectedLayer.LayerPosition -= 1;
            // Update both
            DatabaseAccess.UpdateLayer(selectedLayer, triggerUpdateEvent: false);
            DatabaseAccess.UpdateLayer(neighbour);

            // Update XAML Binding Property by fetching from DB
            Layers = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId);
        }

        /*
         * MVVM Properties
         */

        [ObservableProperty]
        private List<Layer> layers = DatabaseAccess.QueryLayersByElementId(FO0_LandingPage.SelectedElementId);

        [ObservableProperty]
        private string elementName = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Name;

        [ObservableProperty]
        private string elementType = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Construction.Type;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public string TiValue { get; } = UserSaved.Ti.ToString();
        public string TeValue { get; } = UserSaved.Te.ToString();
        public string RsiValue { get; } = UserSaved.Rsi.ToString();
        public string RseValue { get; } = UserSaved.Rse.ToString();
        public string RelFiValue { get; } = UserSaved.Rel_Fi.ToString();
        public string RelFeValue { get; } = UserSaved.Rel_Fe.ToString();
    }
}
