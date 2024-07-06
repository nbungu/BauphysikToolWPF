using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => "AddLayerWindow";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void AddLayer()
        {
            if (Thickness == "" || Convert.ToDouble(Thickness) <= 0) return;

            // LayerPosition is always at end of List 
            int layerCount = UserSaved.SelectedElement.Layers.Count;

            Layer layer = new Layer
            {
                //LayerId gets set by SQLite DB (AutoIncrement)
                LayerPosition = layerCount,
                InternalId = layerCount,
                Thickness = Convert.ToDouble(Thickness),
                IsEffective = true,
                MaterialId = _selectedListViewItem.Id,
                Material = _selectedListViewItem,
                ElementId = UserSaved.SelectedElement.Id,
                Element = UserSaved.SelectedElement
            };
            //DatabaseAccess.CreateLayer(layer);
            UserSaved.SelectedElement.Layers.Add(layer);
            // Trigger Event to Update Layer Window
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void ResetMaterialList()
        {
            SearchString = "";
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _thickness = "6";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string _searchString = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private MaterialCategory _selectedCategory = MaterialCategory.None;

        [ObservableProperty]
        private Material _selectedListViewItem = new Material();

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<Material> Materials => SearchString != "" ? DatabaseAccess.QueryMaterialByCategory(SelectedCategory).Where(m => m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList() : DatabaseAccess.QueryMaterialByCategory(SelectedCategory);
    }
}
