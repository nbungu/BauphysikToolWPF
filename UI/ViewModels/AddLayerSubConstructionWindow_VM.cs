using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerSubConstructionWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerSubConstructionWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerSubConstructionWindow.cs due to Class-Binding in xaml via DataContext
        public string Title = "AddLayerSubConstructionWindow";

        // All changes are being made to this Instance first
        private readonly LayerSubConstruction _tempConstruction;

        public AddLayerSubConstructionWindow_VM()
        {
            if (!UserSaved.SelectedLayer.HasSubConstruction)
            {
                _tempConstruction = new LayerSubConstruction()
                {
                    Width = 4,
                    Thickness = UserSaved.SelectedLayer.Thickness,
                    Spacing = 18,
                    MaterialId = UserSaved.SelectedLayer.MaterialId,
                    Material = UserSaved.SelectedLayer.Material
                };
            }
            else
            {
                _tempConstruction = UserSaved.SelectedLayer.SubConstruction;
            }
            SelectedListViewItem = _tempConstruction.Material;
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void AddSubConstructionLayer(Window? window)
        {
            // To be able to Close Window from within this ViewModel
            if (window is null) return;

            if (!_tempConstruction.IsValid) return;
            UserSaved.SelectedLayer.SubConstruction = _tempConstruction;

            //DatabaseAccess.CreateLayer(layer);

            // Trigger Event to Update Layer Window
            UserSaved.OnSelectedLayerChanged();
            window.Close();
        }

        [RelayCommand]
        private void ResetMaterialList()
        {
            SearchString = "";
        }

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Material value)
        {
            if (value is null) return;
            _tempConstruction.Material.Id = value.Id;
            _tempConstruction.Material = value;
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string _searchString = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private MaterialCategory _selectedCategory = UserSaved.SelectedLayer.SubConstruction?.Material.Category ?? MaterialCategory.None;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        private Material _selectedListViewItem = new Material();

        /*
         * MVVM Capsulated Properties: Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, No direct User Input involved
         */

        public List<IPropertyItem> SubConstructionProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => _tempConstruction.Material.Name) { TriggerPropertyChanged = false },
            new PropertyItem<SubConstructionDirection>("Ausrichtung", () => _tempConstruction.SubConstructionDirection, value => _tempConstruction.SubConstructionDirection = value)
            {
                PropertyValues = Enum.GetValues(typeof(SubConstructionDirection)).Cast<object>().ToArray(),
                TriggerPropertyChanged = false,
            },
            new PropertyItem<double>(Symbol.Thickness, () => _tempConstruction.Thickness, value => _tempConstruction.Thickness = value) { TriggerPropertyChanged = false },
            new PropertyItem<double>(Symbol.Width, () => _tempConstruction.Width, value => _tempConstruction.Width = value) { TriggerPropertyChanged = false },
            new PropertyItem<double>(Symbol.Distance, () => _tempConstruction.Spacing, value => _tempConstruction.Spacing = value) { TriggerPropertyChanged = false },
        };

        public List<Material> Materials => SearchString != "" ? DatabaseAccess.QueryMaterialByCategory(SelectedCategory).Where(m => m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList() : DatabaseAccess.QueryMaterialByCategory(SelectedCategory);
    }
}
