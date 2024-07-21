using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // LayerPosition is always at end of List 
            int layerCount = UserSaved.SelectedElement.Layers.Count;

            Layer layer = new Layer
            {
                //LayerId gets set by SQLite DB (AutoIncrement)
                LayerPosition = layerCount,
                InternalId = layerCount,
                Thickness = Convert.ToDouble(Thickness),
                IsEffective = true,
                MaterialId = _selectedListViewItem.Id,
                Material = _selectedListViewItem,
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
        private MaterialCategory _selectedCategory = MaterialCategory.NotDefined;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Materials))]
        private int _selectedTabIndex;

        [ObservableProperty]
        private Material _selectedListViewItem = new Material();

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<IPropertyItem> ThicknessProperties => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.Thickness, () => Thickness, value => Thickness = value),
        };

        public List<MaterialCategory> CustomCategories => DatabaseAccess.GetMaterialsQuery().Where(m => m.IsUserDefined).Select(m => m.Category).ToList();

        //public List<Material> Materials => SearchString != "" ? DatabaseAccess.GetMaterialsQuery().Where(m => m.Category == SelectedCategory).Where(m => m.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList() : DatabaseAccess.QueryMaterialByCategory(SelectedCategory);
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
