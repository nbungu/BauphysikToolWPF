using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerWindow_VM : ObservableObject
    {
        private readonly Element _element = new Element();
        private readonly Layer? _targetLayer;

        // Called by 'InitializeComponent()' from AddLayerWindow.cs due to Class-Binding in xaml via DataContext
        public AddLayerWindow_VM()
        {
            if (Session.SelectedElement is null) return;

            _element = Session.SelectedElement;
            _targetLayer = _element.Layers.FirstOrDefault(l => l?.InternalId == AddLayerWindow.TargetLayerInternalId, null);

            if (_targetLayer != null)
            {
                SelectedTabIndex = _targetLayer.Material.IsUserDefined ? 1 : 0;
                SelectedMaterialCategoryIndex = (int)_targetLayer.Material.MaterialCategory;
                SelectedListViewItem = _targetLayer.Material;
                Thickness = _targetLayer.Thickness;
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
            if (SelectedListViewItem is null)
            {
                MainWindow.ShowToast("Schicht kann der Konstruktion nicht hinzugefügt werden.", ToastType.Info);
                return;
            }
            if (Thickness <= 0)
            {
                MainWindow.ShowToast("Schichtdicke muss größer 0,0 cm sein.", ToastType.Info);
                return;
            }

            int materialId;

            // Update or Create new Material if necessary
            if (SelectedListViewItem.IsUserDefined)
            {
                DatabaseManager.UpdateMaterial(SelectedListViewItem);
                materialId = SelectedListViewItem.Id;
            }
            else
            {
                // Check if current Material from Database List was edited
                var dbMaterial = DatabaseManager.QueryMaterialById(SelectedListViewItem.Id);
                if (!SelectedListViewItem.Equals(dbMaterial))
                {
                    // Create new Material
                    var customMaterial = SelectedListViewItem.Copy();
                    customMaterial.Name = SelectedListViewItem.Name + " (Edited)";
                    customMaterial.IsUserDefined = true;
                    // Create in Database
                    DatabaseManager.CreateMaterial(customMaterial);
                    materialId = customMaterial.Id;
                }
                else
                {
                    materialId = SelectedListViewItem.Id;
                }
            }

            // Update Material in existing Layer
            if (_targetLayer != null)
            {
                _targetLayer.MaterialId = materialId;
                _targetLayer.Thickness = Thickness;
                // Trigger event to update LayerWindow and all subscriber windows
                Session.OnSelectedLayerChanged();
            }
            // Add new Layer
            else
            {
                // LayerPosition is always at end of List 
                int layerCount = _element.Layers.Count;

                Layer layer = new Layer
                {
                    LayerPosition = layerCount,
                    InternalId = layerCount,
                    Thickness = Thickness,
                    IsEffective = true,
                    MaterialId = materialId,
                };
                if (AddLayerWindow.AddAtIndex != -1) layer.LayerPosition = AddLayerWindow.AddAtIndex;
                
                _element.AddLayer(layer);
               
                // Trigger event to update LayerWindow and all subscriber windows
                Session.OnSelectedElementChanged();
            }
        }

        [RelayCommand]
        private void DeleteMaterial()
        {
            if (SelectedListViewItem != null && SelectedListViewItem.IsUserDefined)
            {
                if (IsMaterialInUse)
                {
                    AddLayerWindow.ShowToast($"Löschen nicht möglich: {SelectedListViewItem} wird noch verwendet!", ToastType.Error);
                    Logger.LogWarning($"Cannot delete custom Material: {SelectedListViewItem}. It is still being used!");
                }
                else
                {
                    DatabaseManager.DeleteMaterial(SelectedListViewItem);
                    AddLayerWindow.ShowToast($"Eigenes Material gelöscht: {SelectedListViewItem}", ToastType.Success);
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
                    Name = $"Neues Material ({MaterialCategoryMapping[(MaterialCategory)SelectedMaterialCategoryIndex]})",
                    IsUserDefined = true,
                    MaterialCategory = (MaterialCategory)SelectedMaterialCategoryIndex
                };
            }
            DatabaseManager.CreateMaterial(newMaterial);
            SelectedTabIndex = -1;
            SelectedTabIndex = 1;
            SelectedListViewItem = newMaterial;
        }

        partial void OnSelectedListViewItemChanged(Material? value)
        {
            if (value is null) return;
            // Only preset the Thickness if the Layer is not already set
            if (_targetLayer is null) Thickness = DefaultLayerWidthMapping.TryGetValue(value.MaterialCategory, out var thickness) ? thickness : 1.0;
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private double _thickness = 6.0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredMaterials))]
        private string _searchString = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredMaterials))]
        private static int _selectedMaterialCategoryIndex = (int)MaterialCategory.NotDefined;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredMaterials))]
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

        public string Title => _targetLayer != null ? $"Ausgewählte Schicht bearbeiten: {_targetLayer}" : "Neue Schicht erstellen";
        public string Tab0Header => $"Datenbank ({DatabaseManager.GetMaterialsQuery().Count(m => !m.IsUserDefined)})";
        public string Tab1Header => $"Eigene Materialien ({DatabaseManager.GetMaterialsQuery().Count(m => m.IsUserDefined)})";
        public List<IPropertyItem> MaterialProperties => SelectedListViewItem != null ? new List<IPropertyItem>()
        {
            new PropertyItem<string>("Materialbezeichnung", () => SelectedListViewItem.Name, value => SelectedListViewItem.Name = value),
            new PropertyItem<int>("Kategorie", () => (int)SelectedListViewItem.MaterialCategory, value => SelectedListViewItem.MaterialCategory = (MaterialCategory)value)
            {
                PropertyValues = MaterialCategoryMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.ThermalConductivity, () => SelectedListViewItem.ThermalConductivity, value => SelectedListViewItem.ThermalConductivity = value) { DecimalPlaces = 3},
            new PropertyItem<int>(Symbol.RawDensity, () => SelectedListViewItem.BulkDensity, value => SelectedListViewItem.BulkDensity = value),
            new PropertyItem<int>(Symbol.SpecificHeatCapacity, () => SelectedListViewItem.SpecificHeatCapacity, value => SelectedListViewItem.SpecificHeatCapacity = value),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => SelectedListViewItem.DiffusionResistance, value => SelectedListViewItem.DiffusionResistance = value),
            new PropertyItem<bool>("Material in Benutzung", () => IsMaterialInUse),
        } : new List<IPropertyItem>(0);

        public List<Material> FilteredMaterials => GetFilteredMaterials();
        public bool AllowDelete => SelectedListViewItem?.IsUserDefined ?? false;
        public string ButtonText => _targetLayer != null ? "Änderung übernehmen" : "Schicht hinzufügen";
        public static IEnumerable<string> GetMaterialCategoryNames() => MaterialCategoryMapping.Values;
        public bool IsMaterialInUse => Session.SelectedProject?.IsMaterialInUse(SelectedListViewItem) ?? false;

        // TODO: implement QueryFilterConfig...
        private List<Material> GetFilteredMaterials()
        {
            if (SelectedMaterialCategoryIndex == (int)MaterialCategory.NotDefined)
            {
                if (SearchString != "")
                {
                    return DatabaseManager.GetMaterialsQuery().Where(m =>
                        m.IsUserDefined == (SelectedTabIndex == 1) &&
                        m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return DatabaseManager.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1)).ToList();
            }
            else
            {
                if (SearchString != "")
                {
                    return DatabaseManager.GetMaterialsQuery().Where(m =>
                        m.IsUserDefined == (SelectedTabIndex == 1) &&
                        m.MaterialCategory == (MaterialCategory)SelectedMaterialCategoryIndex &&
                        m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return DatabaseManager.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1) &&
                    m.MaterialCategory == (MaterialCategory)SelectedMaterialCategoryIndex).ToList();
            }
        }

        private void MaterialPropertiesChanged()
        {
            if (SelectedListViewItem is null || !SelectedListViewItem.IsUserDefined) return;
            DatabaseManager.UpdateMaterial(SelectedListViewItem);
            SelectedTabIndex = -1;
            SelectedTabIndex = 1;
        }
    }
}
