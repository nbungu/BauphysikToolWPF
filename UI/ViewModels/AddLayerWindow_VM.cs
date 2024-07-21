using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerWindow.cs due to Class-Binding in xaml via DataContext
        public string Title = "AddLayerWindow";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void AddLayer()
        {
            if (Thickness <= 0) return;

            int materialId;
            Material material;

            if (_selectedListViewItem.IsUserDefined)
            {
                DatabaseAccess.UpdateMaterial(_selectedListViewItem);
                materialId = _selectedListViewItem.Id;
                material = _selectedListViewItem;
            }
            else
            {
                // Check if current Material from Database List was edited
                var dbMaterial = DatabaseAccess.GetMaterialsQuery().First(m => m.Id == _selectedListViewItem.Id);
                if (!_selectedListViewItem.Equals(dbMaterial))
                {
                    // Create new Material
                    var customMaterial = _selectedListViewItem.Copy();
                    customMaterial.Name = _selectedListViewItem.Name + " (Edited)";
                    customMaterial.IsUserDefined = true;
                    // Create in Database
                    DatabaseAccess.CreateMaterial(customMaterial);
                    materialId = customMaterial.Id;
                    material = customMaterial;
                }
                else
                {
                    materialId = _selectedListViewItem.Id;
                    material = _selectedListViewItem;
                }
            }
            
            // LayerPosition is always at end of List 
            int layerCount = UserSaved.SelectedElement.Layers.Count;

            Layer layer = new Layer
            {
                //LayerId gets set by SQLite DB (AutoIncrement)
                LayerPosition = layerCount,
                InternalId = layerCount,
                Thickness = Convert.ToDouble(Thickness),
                IsEffective = true,
                MaterialId = materialId,
                Material = material,
                ElementId = UserSaved.SelectedElement.Id,
                Element = UserSaved.SelectedElement
            };
            UserSaved.SelectedElement.AddLayer(layer);

            // Trigger Event to Update Layer Window
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void ResetMaterialList()
        {
            SearchString = "";
        }

        partial void OnSelectedListViewItemChanged(Material value)
        {
            if (value is null) return;
            Thickness = Material.DefaultLayerWidthForCategory(value.Category);
        }


        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private double _thickness = 6.0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private string _searchString = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private static MaterialCategory _selectedCategory = MaterialCategory.NotDefined;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private int _selectedTabIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MaterialProperties))]
        private Material _selectedListViewItem = new Material();

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<IPropertyItem> MaterialProperties => _selectedListViewItem is null ? new List<IPropertyItem>() : new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => _selectedListViewItem.Name),
            new PropertyItem<string>("Kategorie", () => _selectedListViewItem.CategoryName),
            new PropertyItem<double>(Symbol.ThermalConductivity, () => _selectedListViewItem.ThermalConductivity, value => _selectedListViewItem.ThermalConductivity = value),
            new PropertyItem<int>(Symbol.RawDensity, () => _selectedListViewItem.BulkDensity, value => _selectedListViewItem.BulkDensity = value),
            new PropertyItem<int>(Symbol.SpecificHeatCapacity, () => _selectedListViewItem.SpecificHeatCapacity, value => _selectedListViewItem.SpecificHeatCapacity = value),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => _selectedListViewItem.DiffusionResistance, value => _selectedListViewItem.DiffusionResistance = value),
        };

        public List<MaterialCategory> CustomCategories => DatabaseAccess.GetMaterialsQuery().Where(m => m.IsUserDefined).Select(m => m.Category).ToList();
        public List<Material> Materials => GetMaterials();
        

        // TODO: implement QueryFilterConfig...
        private List<Material> GetMaterials()
        {
            if (SelectedCategory == MaterialCategory.NotDefined)
            {
                if (SearchString != "")
                {
                    return DatabaseAccess.GetMaterialsQuery().Where(m =>
                        m.IsUserDefined == (SelectedTabIndex == 1) &&
                        m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return DatabaseAccess.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1)).ToList();
            }
            else
            {
                if (SearchString != "")
                {
                    return DatabaseAccess.GetMaterialsQuery().Where(m =>
                        m.IsUserDefined == (SelectedTabIndex == 1) &&
                        m.Category == SelectedCategory &&
                        m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return DatabaseAccess.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1) &&
                    m.Category == SelectedCategory).ToList();
            }
        }
    }
}
