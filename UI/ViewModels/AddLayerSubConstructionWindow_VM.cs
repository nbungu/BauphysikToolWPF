using BauphysikToolWPF.Repository;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Models;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddLayerSubConstructionWindow.xaml: Used in xaml as "DataContext"
    public partial class AddLayerSubConstructionWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddLayerSubConstructionWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => EditSelectedSubConstr ? $"Ausgewählte Balkenlage bearbeiten: {Session.SelectedLayer.SubConstruction}" : "Neue Balkenlage erstellen";
        
        // All changes are being made to this Instance first
        private readonly LayerSubConstruction _tempConstruction;

        public AddLayerSubConstructionWindow_VM()
        {
            PropertyItem<SubConstructionDirection>.PropertyChanged += UpdateXamlBindings;

            if (EditSelectedSubConstr && Session.SelectedLayer.SubConstruction != null)
            {
                _tempConstruction = Session.SelectedLayer.SubConstruction.Copy();
            }
            else
            {
                _tempConstruction = new LayerSubConstruction()
                {
                    Width = 4,
                    Thickness = Session.SelectedLayer.Thickness,
                    Spacing = 18,
                    MaterialId = Session.SelectedLayer.MaterialId,
                    Material = Session.SelectedLayer.Material,
                    LayerId = Session.SelectedLayer.Id,
                    Layer = Session.SelectedLayer,
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
            // Replace/Add
            Session.SelectedLayer.SubConstruction = _tempConstruction;

            // Trigger Event to Update Layer Window
            Session.OnSelectedLayerChanged();
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
            _tempConstruction.MaterialId = value.Id;
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

        public List<Material> Materials => GetMaterials();
        public string Tab0Header => $"Datenbank ({DatabaseAccess.GetMaterialsQuery().Count(m => !m.IsUserDefined)})";
        public string Tab1Header => $"Eigene Materialien ({DatabaseAccess.GetMaterialsQuery().Count(m => m.IsUserDefined)})";
        public bool EditSelectedSubConstr => AddLayerSubConstructionWindow.EditExistingSubConstr;
        public string ButtonText => EditSelectedSubConstr ? "Änderung übernehmen" : "Balkenlage hinzufügen";
        public List<IPropertyItem> SubConstructionProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => _tempConstruction.Material.Name),
            new PropertyItem<SubConstructionDirection>("Ausrichtung", () => _tempConstruction.Direction, value => _tempConstruction.Direction = value)
            {
                PropertyValues = Enum.GetValues(typeof(SubConstructionDirection)).Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.Thickness, () => _tempConstruction.Thickness, value => _tempConstruction.Thickness = value),
            new PropertyItem<double>(Symbol.Width, () => _tempConstruction.Width, value => _tempConstruction.Width = value),
            new PropertyItem<double>(Symbol.Distance, () => _tempConstruction.Spacing, value => _tempConstruction.Spacing = value),
        };

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
                    m.Category == (MaterialCategory)SelectedCategoryIndex &&
                    m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            return DatabaseAccess.GetMaterialsQuery().Where(m =>
                m.IsUserDefined == (SelectedTabIndex == 1) &&
                m.Category == (MaterialCategory)SelectedCategoryIndex).ToList();
            
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
