using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => Session.SelectedLayer != null && EditSelectedLayer ? $"Ausgewählte Schicht bearbeiten: {Session.SelectedLayer}" : "Neue Schicht erstellen";

        public AddLayerWindow_VM()
        {
            if (EditSelectedLayer && Session.SelectedLayer != null)
            {
                SelectedTabIndex = Session.SelectedLayer.Material.IsUserDefined ? 1 : 0;
                SelectedMaterialCategoryIndex = (int)Session.SelectedLayer.Material.Category;
                SelectedListViewItem = Session.SelectedLayer.Material;
            }
            
            PropertyItem<string>.PropertyChanged += MaterialPropertiesChanged;
            PropertyItem<int>.PropertyChanged += MaterialPropertiesChanged;
            PropertyItem<double>.PropertyChanged += MaterialPropertiesChanged;
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

            // Update or Create new Material if necessary
            if (SelectedListViewItem.IsUserDefined)
            {
                DatabaseAccess.UpdateMaterial(SelectedListViewItem);
                materialId = SelectedListViewItem.Id;
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
                }
                else
                {
                    materialId = SelectedListViewItem.Id;
                }
            }

            // Update Material in existing Layer or Add new Layer
            if (EditSelectedLayer && Session.SelectedLayer != null)
            {
                // TODO: Correct?
                Session.SelectedLayer.MaterialId = materialId;
            }
            else if (Session.SelectedElement != null)
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
                    Category = (MaterialCategory)SelectedMaterialCategoryIndex
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
            Thickness = DefaultLayerWidthMapping[value.Category];
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
        private static int _selectedMaterialCategoryIndex = (int)MaterialCategory.NotDefined;

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
        public List<IPropertyItem> MaterialProperties => SelectedListViewItem != null ? new List<IPropertyItem>()
        {
            new PropertyItem<string>("Materialbezeichnung", () => SelectedListViewItem.Name, value => SelectedListViewItem.Name = value),
            new PropertyItem<int>("Kategorie", () => (int)SelectedListViewItem.Category, value => SelectedListViewItem.Category = (MaterialCategory)value)
            {
                PropertyValues = MaterialCategoryMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.ThermalConductivity, () => SelectedListViewItem.ThermalConductivity, value => SelectedListViewItem.ThermalConductivity = value) { DecimalPlaces = 3},
            new PropertyItem<int>(Symbol.RawDensity, () => SelectedListViewItem.BulkDensity, value => SelectedListViewItem.BulkDensity = value),
            new PropertyItem<int>(Symbol.SpecificHeatCapacity, () => SelectedListViewItem.SpecificHeatCapacity, value => SelectedListViewItem.SpecificHeatCapacity = value),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => SelectedListViewItem.DiffusionResistance, value => SelectedListViewItem.DiffusionResistance = value),
            new PropertyItem<bool>("Material in Benutzung", () => IsUsedInLayer || IsUsedInSubConstr),
        } : new List<IPropertyItem>(0);

        public List<Material> Materials => GetMaterials();
        public bool AllowDelete => SelectedListViewItem?.IsUserDefined ?? false;
        public bool EditSelectedLayer => AddLayerWindow.EditExistingLayer;
        public string ButtonText => EditSelectedLayer ? "Änderung übernehmen" : "Schicht hinzufügen";

        // TODO: fix -> check only for current project
        public bool IsUsedInLayer => true; // DatabaseAccess.GetLayersQuery().Any(l => SelectedListViewItem != null && l.MaterialId == SelectedListViewItem.Id);
        public bool IsUsedInSubConstr => true; //DatabaseAccess.GetSubConstructionQuery().Any(s => SelectedListViewItem != null && s.MaterialId == SelectedListViewItem.Id);
        public List<string> MaterialCategoryList => MaterialCategoryMapping.Values.ToList();

        // TODO: implement QueryFilterConfig...
        private List<Material> GetMaterials()
        {
            if (SelectedMaterialCategoryIndex == (int)MaterialCategory.NotDefined)
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
                        m.Category == (MaterialCategory)SelectedMaterialCategoryIndex &&
                        m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return DatabaseAccess.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1) &&
                    m.Category == (MaterialCategory)SelectedMaterialCategoryIndex).ToList();
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
