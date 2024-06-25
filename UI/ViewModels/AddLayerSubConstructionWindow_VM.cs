using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerSubConstructionWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerSubConstructionWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerSubConstructionWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => "AddLayerSubConstructionWindow";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void AddSubConstructionLayer(Material? selectedMaterial)
         {
            if (selectedMaterial is null) return;
            if (Width == "" || Convert.ToDouble(Width) <= 0) return;
            if (Height == "" || Convert.ToDouble(Height) <= 0) return;
            if (Spacing == "" || Convert.ToDouble(Spacing) <= 0) return;

            var subConstruction = new LayerSubConstruction()
            {
                Width = Convert.ToDouble(Width),
                Height = Convert.ToDouble(Height),
                Spacing = Convert.ToDouble(Spacing),
                MaterialId = selectedMaterial.Id,
                Material = selectedMaterial,
            };
            //DatabaseAccess.CreateLayer(layer);
            UserSaved.SelectedLayer.SubConstruction = subConstruction;
            // Trigger Event to Update Layer Window
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void SearchMaterial()
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
        private MaterialCategory _selectedCategory = MaterialCategory.None;

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _width = "6";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _height = "6";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _spacing = "40";

        /*
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string searchString = "";
        */

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<Material> Materials => DatabaseAccess.QueryMaterialByCategory(SelectedCategory);
    }
}
