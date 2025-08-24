using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerSubConstructionWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerSubConstructionWindow_VM : ObservableObject
    {
        private readonly Element _element;
        private readonly Layer? _targetLayer;
        private readonly LayerSubConstruction _tempConstruction;
        
        // Called by 'InitializeComponent()' from AddLayerSubConstructionWindow.cs due to Class-Binding in xaml via DataContext
        public AddLayerSubConstructionWindow_VM()
        {
            if (Session.SelectedElement is null) return;

            _element = Session.SelectedElement;
            _targetLayer = _element.Layers.FirstOrDefault(l => l?.InternalId == AddLayerSubConstructionWindow.TargetLayerInternalId, null);

            if (_targetLayer is null) return;
            
            PropertyItem<int>.PropertyChanged += UpdateXamlBindings;

            if (_targetLayer.SubConstruction != null)
            {
                _tempConstruction = _targetLayer.SubConstruction.Copy();
            }
            else
            {
                _tempConstruction = new LayerSubConstruction()
                {
                    Width = 4,
                    Thickness = _targetLayer.Thickness,
                    Spacing = 18,
                    MaterialId = _targetLayer.MaterialId,
                    LayerNumber = _targetLayer.LayerNumber,
                    IsEffective = _targetLayer.IsEffective,
                };
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
            if (_targetLayer is null) return;

            // Replace/Add
            _targetLayer.SubConstruction = _tempConstruction;

            // Trigger Event to Update Layer Window
            Session.OnSelectedLayerChanged();
            window.Close();
        }

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Material? value)
        {
            if (value is null) return;
            _tempConstruction.MaterialId = value.Id;
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
        private static int _selectedCategoryIndex = (int)MaterialCategory.NotDefined;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        private static int _selectedTabIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        private Material _selectedListViewItem = new Material();

        /*
         * MVVM Capsulated Properties: Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, No direct User Input involved
         */

        public string Title => _targetLayer != null && _targetLayer.SubConstruction != null ? $"Balkenlage bearbeiten: {_targetLayer.SubConstruction} | Schicht: {_targetLayer}" : $"Neue Balkenlage erstellen | Schicht: {_targetLayer}";
        public List<Material> Materials => GetMaterials();
        public string Tab0Header => $"Datenbank ({DatabaseAccess.GetMaterialsQuery().Count(m => !m.IsUserDefined)})";
        public string Tab1Header => $"Eigene Materialien ({DatabaseAccess.GetMaterialsQuery().Count(m => m.IsUserDefined)})";
        public string ButtonText => _targetLayer?.SubConstruction != null ? "Änderung übernehmen" : "Balkenlage hinzufügen";
        public List<IPropertyItem> SubConstructionProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => _tempConstruction.Material.Name),
            new PropertyItem<int>("Ausrichtung", () => (int)_tempConstruction.Direction, value => _tempConstruction.Direction = (ConstructionDirection)value)
            {
                PropertyValues = SubConstructionDirectionMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.Thickness, () => _tempConstruction.Thickness, value => _tempConstruction.Thickness = value),
            new PropertyItem<double>(Symbol.Width, () => _tempConstruction.Width, value => _tempConstruction.Width = value),
            new PropertyItem<double>(Symbol.Distance, () => _tempConstruction.Spacing, value => _tempConstruction.Spacing = value),
        };

        public IEnumerable<string> MaterialCategoryList => MaterialCategoryMapping.Values;

        
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
            if (SearchString != "")
            {
                return DatabaseAccess.GetMaterialsQuery().Where(m =>
                    m.IsUserDefined == (SelectedTabIndex == 1) &&
                    m.MaterialCategory == (MaterialCategory)SelectedCategoryIndex &&
                    m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            return DatabaseAccess.GetMaterialsQuery().Where(m =>
                m.IsUserDefined == (SelectedTabIndex == 1) &&
                m.MaterialCategory == (MaterialCategory)SelectedCategoryIndex).ToList();
            
        }
        
        /// <summary>
        /// Updates XAML Bindings
        /// </summary>
        private void UpdateXamlBindings()
        {
            OnPropertyChanged(nameof(SubConstructionProperties));  
        }
    }
}
