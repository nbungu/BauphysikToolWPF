using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BauphysikToolWPF.Models.Helper;

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
                Width = Convert.ToDouble(Width, CultureInfo.CurrentCulture),
                Height = Convert.ToDouble(Height, CultureInfo.CurrentCulture),
                Spacing = Convert.ToDouble(Spacing, CultureInfo.CurrentCulture),
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
        private MaterialCategory _selectedCategory = UserSaved.SelectedLayer.SubConstruction?.Material.Category ?? MaterialCategory.None;

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _width = UserSaved.SelectedLayer.SubConstruction?.Width.ToString(CultureInfo.CurrentCulture) ?? "4,8";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _height = UserSaved.SelectedLayer.SubConstruction?.Height.ToString(CultureInfo.CurrentCulture) ?? "2,4";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _spacing = UserSaved.SelectedLayer.SubConstruction?.Spacing.ToString(CultureInfo.CurrentCulture) ?? "18";

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
