using BauphysikToolWPF.Repository;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Repository.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Models;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => EditSelectedLayer ? $"Ausgewählte Schicht bearbeiten: {Session.SelectedLayer}" : "Neue Schicht erstellen";

        public AddLayerWindow_VM()
        {
            if (EditSelectedLayer)
            {
                SelectedTabIndex = Session.SelectedLayer.Material.IsUserDefined ? 1 : 0;
                SelectedCategoryIndex = (int)Session.SelectedLayer.Material.Category;
                SelectedListViewItem = Session.SelectedLayer.Material;
            }
            
            PropertyItem<string>.PropertyChanged += MaterialPropertiesChanged;
            PropertyItem<int>.PropertyChanged += MaterialPropertiesChanged;
            PropertyItem<double>.PropertyChanged += MaterialPropertiesChanged;
            PropertyItem<MaterialCategory>.PropertyChanged += MaterialPropertiesChanged;
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void ApplyChanges()
        {
            if (Thickness <= 0 || SelectedListViewItem is null) return;

            int materialId;
            Material material;

            // Update or Create new Material if necessary
            if (SelectedListViewItem.IsUserDefined)
            {
                DatabaseAccess.UpdateMaterial(SelectedListViewItem);
                materialId = SelectedListViewItem.Id;
                material = SelectedListViewItem;
            }
            else
            {
                // Check if current Material from Database List was edited
                var dbMaterial = DatabaseAccess.GetMaterialsQuery().First(m => m.Id == SelectedListViewItem.Id);
                if (!SelectedListViewItem.Equals(dbMaterial))
                {
                    // Create new Material
                    var customMaterial = SelectedListViewItem.Copy();
                    customMaterial.Name = SelectedListViewItem.Name + " (Edited)";
                    customMaterial.IsUserDefined = true;
                    // Create in Database
                    DatabaseAccess.CreateMaterial(customMaterial);
                    materialId = customMaterial.Id;
                    material = customMaterial;
                }
                else
                {
                    materialId = SelectedListViewItem.Id;
                    material = SelectedListViewItem;
                }
            }

            // Update Material in existing Layer or Add new Layer
            if (EditSelectedLayer)
            {
                Session.SelectedLayer.Material = material;
            }
            else
            {
                // LayerPosition is always at end of List 
                int layerCount = Session.SelectedElement.Layers.Count;

                Layer layer = new Layer
                {
                    //LayerId gets set by SQLite DB (AutoIncrement)
                    LayerPosition = layerCount,
                    InternalId = layerCount,
                    Thickness = Convert.ToDouble(Thickness),
                    IsEffective = true,
                    MaterialId = materialId,
                    Material = material,
                    ElementId = Session.SelectedElement.Id,
                    Element = Session.SelectedElement
                };
                Session.SelectedElement.AddLayer(layer);
            }
            // Trigger Event to Update Layer Window
            Session.OnSelectedLayerChanged();

        }

        [RelayCommand]
        private void DeleteMaterial()
        {
            if (SelectedListViewItem != null && SelectedListViewItem.IsUserDefined)
            {
                if (IsUsedInLayer || IsUsedInSubConstr)
                {
                    AddLayerWindow.ShowToast($"Cannot delete custom Material: {SelectedListViewItem}. It is still being used!", ToastType.Warning);
                    Logger.LogWarning($"Cannot delete custom Material: {SelectedListViewItem}. It is still being used!");
                }
                else
                {
                    DatabaseAccess.DeleteMaterial(SelectedListViewItem);
                    AddLayerWindow.ShowToast($"Deleted custom Material: {SelectedListViewItem}", ToastType.Success);
                    Logger.LogInfo($"Deleted custom Material: {SelectedListViewItem}");
                    SelectedListViewItem = null;
                    SelectedTabIndex = -1;
                    SelectedTabIndex = 1;
                }
            }
        }
        [RelayCommand]
        private void CreateMaterial()
        {
            Material newMaterial;
            if (SelectedListViewItem != null)
            {
                newMaterial = SelectedListViewItem.Copy();
                newMaterial.IsUserDefined = true;
                newMaterial.Name += " (Kopie)";
            }
            else
            {
                newMaterial = new Material()
                {
                    Name = "Neu erstelltes Material",
                    IsUserDefined = true,
                    Category = (MaterialCategory)SelectedCategoryIndex
                };
            }
            DatabaseAccess.CreateMaterial(newMaterial);
            SelectedTabIndex = -1;
            SelectedTabIndex = 1;
            SelectedListViewItem = newMaterial;
        }

        [RelayCommand]
        private void ResetMaterialList()
        {
            SearchString = "";
        }

        partial void OnSelectedListViewItemChanged(Material? value)
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
        private static int _selectedCategoryIndex = (int)MaterialCategory.NotDefined;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        [NotifyPropertyChangedFor(nameof(Tab0Header))]
        [NotifyPropertyChangedFor(nameof(Tab1Header))]
        [NotifyPropertyChangedFor(nameof(MaterialProperties))]
        private static int _selectedTabIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AllowDelete))]
        [NotifyPropertyChangedFor(nameof(MaterialProperties))]
        private Material? _selectedListViewItem;

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public string Tab0Header => $"Datenbank ({DatabaseAccess.GetMaterialsQuery().Count(m => !m.IsUserDefined)})";
        public string Tab1Header => $"Eigene Materialien ({DatabaseAccess.GetMaterialsQuery().Count(m => m.IsUserDefined)})";
        public List<IPropertyItem> MaterialProperties => SelectedListViewItem is null ? new List<IPropertyItem>() : new List<IPropertyItem>()
        {
            new PropertyItem<string>("Materialbezeichnung", () => SelectedListViewItem.Name, value => SelectedListViewItem.Name = value),
            new PropertyItem<MaterialCategory>("Kategorie", () => SelectedListViewItem.Category, value => SelectedListViewItem.Category = value)
            {
                PropertyValues = Enum.GetValues(typeof(MaterialCategory)).Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.ThermalConductivity, () => SelectedListViewItem.ThermalConductivity, value => SelectedListViewItem.ThermalConductivity = value) { DecimalPlaces = 3},
            new PropertyItem<int>(Symbol.RawDensity, () => SelectedListViewItem.BulkDensity, value => SelectedListViewItem.BulkDensity = value),
            new PropertyItem<int>(Symbol.SpecificHeatCapacity, () => SelectedListViewItem.SpecificHeatCapacity, value => SelectedListViewItem.SpecificHeatCapacity = value),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => SelectedListViewItem.DiffusionResistance, value => SelectedListViewItem.DiffusionResistance = value),
            new PropertyItem<bool>("Material in Benutzung", () => IsUsedInLayer || IsUsedInSubConstr),
        };

        public List<Material> Materials => GetMaterials();
        public bool AllowDelete => SelectedListViewItem?.IsUserDefined ?? false;
        public bool EditSelectedLayer => AddLayerWindow.EditExistingLayer;
        public string ButtonText => EditSelectedLayer ? "Änderung übernehmen" : "Schicht hinzufügen";
        public bool IsUsedInLayer => DatabaseAccess.GetLayersQuery().Any(l => SelectedListViewItem != null && l.MaterialId == SelectedListViewItem.Id);
        public bool IsUsedInSubConstr => DatabaseAccess.GetSubConstructionQuery().Any(s => SelectedListViewItem != null && s.MaterialId == SelectedListViewItem.Id);
        
        // TODO: implement QueryFilterConfig...
        private List<Material> GetMaterials()
        {
            if (SelectedCategoryIndex == (int)MaterialCategory.NotDefined)
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
                        m.Category == (MaterialCategory)SelectedCategoryIndex &&
                        m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return DatabaseAccess.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1) &&
                    m.Category == (MaterialCategory)SelectedCategoryIndex).ToList();
            }
        }

        private void MaterialPropertiesChanged()
        {
            if (SelectedListViewItem is null || !SelectedListViewItem.IsUserDefined) return;
            DatabaseAccess.UpdateMaterial(SelectedListViewItem);
            SelectedTabIndex = -1;
            SelectedTabIndex = 1;
        }
    }
}
