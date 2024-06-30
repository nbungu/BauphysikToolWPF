using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Foundation;

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
            if (Thickness == "" || Convert.ToDouble(Thickness) <= 0) return;
            if (Spacing == "" || Convert.ToDouble(Spacing) <= 0) return;

            var subConstruction = new LayerSubConstruction()
            {
                Width = Convert.ToDouble(Width, CultureInfo.CurrentCulture),
                Thickness = Convert.ToDouble(Thickness, CultureInfo.CurrentCulture),
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
        [NotifyPropertyChangedFor(nameof(Materials))]
        private MaterialCategory _selectedCategory = UserSaved.SelectedLayer.SubConstruction?.Material.Category ?? MaterialCategory.None;

        [ObservableProperty]
        private SubConstructionDirection _selectedConstructionDirection = UserSaved.SelectedLayer.SubConstruction?.SubConstructionDirection ?? SubConstructionDirection.Horizontal;

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _width = UserSaved.SelectedLayer.SubConstruction?.Width.ToString(CultureInfo.CurrentCulture) ?? "4,8";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _thickness = UserSaved.SelectedLayer.SubConstruction?.Thickness.ToString(CultureInfo.CurrentCulture) ?? "2,4";

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsThicknessValid))]
        private string _spacing = UserSaved.SelectedLayer.SubConstruction?.Spacing.ToString(CultureInfo.CurrentCulture) ?? "18";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string _searchString = "";


        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<PropertyItem> LayerSubConstrProperties => new List<PropertyItem>
        {
            new PropertyItem("Ausrichtung", SubConstructionDirection.Horizontal.ToString(), (string[])Enum.GetNames(typeof(SubConstructionDirection)) ),
            new PropertyItem(Symbol.Width, Width),
            new PropertyItem(Symbol.Thickness, Thickness),
            new PropertyItem(Symbol.Length, Spacing),
        };

        public List<Material> Materials => SearchString != "" ? DatabaseAccess.QueryMaterialByCategory(SelectedCategory).Where(m => m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList() : DatabaseAccess.QueryMaterialByCategory(SelectedCategory);

    }
}
