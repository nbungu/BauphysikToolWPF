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
        public List<string> DistinctCategories
        {
            get
            {
                List<string> distinctCategories = DatabaseAccess.GetMaterials().Select(m => m.CategoryName).ToList().Distinct().ToList();
                distinctCategories.Insert(0, "Alle anzeigen");
                return distinctCategories;
            }
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void AddLayer(Material? selectedMaterial)
        {
            if (selectedMaterial is null) return;
            if (Thickness == "" || Convert.ToDouble(Thickness) <= 0) return;

            // LayerPosition is always at end of List 
            int layerCount = UserSaved.SelectedElement.Layers.Count;

            Layer layer = new Layer
            {
                //LayerId gets set by SQLite DB (AutoIncrement)
                LayerPosition = layerCount,
                InternalId = layerCount,
                LayerThickness = Convert.ToDouble(Thickness),
                IsEffective = true,
                MaterialId = selectedMaterial.MaterialId,
                Material = selectedMaterial,
                ElementId = UserSaved.SelectedElement.ElementId,
                Element = UserSaved.SelectedElement
            };
            //DatabaseAccess.CreateLayer(layer);
            UserSaved.SelectedElement.Layers.Add(layer);
            // Trigger Event to Update Layer Window
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void CreateMaterial()
        {
            //TODO
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string _selectedCategory = "Alle anzeigen";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _thickness = "6";

        /*
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string searchString = "";
        */

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<Material> Materials => SelectedCategory == "Alle anzeigen" ? DatabaseAccess.QueryMaterialByCategory("*") : DatabaseAccess.QueryMaterialByCategory(SelectedCategory);
    }
}
