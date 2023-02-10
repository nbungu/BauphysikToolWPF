using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für AddLayerWindow.xaml
    /// </summary>
    public partial class AddLayerWindow : Window
    {
        private List<string> distinctCategories;
        private string selectedCategory;
        public AddLayerWindow()
        {
            InitializeComponent();
            // Call these Methods only once when Constructor is invoked (Categories stay constant)
            LoadDistinctCategories();
            LoadCategoriesList();

            LoadMaterialsByCategory(); // Update Material List, based on selected Category
        }

        private void LoadDistinctCategories()
        {
            //The lambda operator =>, may be read as “goes to” or “becomes”. Lambda function = anonymous function (function without a name)
            List<string> allCategories = DatabaseAccess.GetMaterials().Select(m => m.Category).ToList();
            distinctCategories = allCategories.Distinct().ToList();
        }

        private void LoadCategoriesList()
        {
            List<string> categoryList = new List<string>(distinctCategories);
            categoryList.Insert(0, "Alle anzeigen");
            collectionView_Categories.ItemsSource = categoryList;
        }
        private void LoadMaterialsByCategory(string category = "*")
        {
            collectionView_Materials.ItemsSource = DatabaseAccess.QueryMaterialByCategory(category);
        }

        private void LoadMaterialsFromQuery(string searchQuery)
        {
            collectionView_Materials.ItemsSource = DatabaseAccess.QueryMaterialBySearchString(searchQuery);
        }

        private void AddMaterial_Clicked(object sender, EventArgs e)
        {
            if (collectionView_Materials.SelectedItem is null)
                return;

            //ItemsSource of collectionView holds the List<Material>. 
            var material = collectionView_Materials.SelectedItem as Material;
            if (material is null)
            {
                return;
            }
            else
            {
                Layer layer = new Layer()
                {
                    //LayerId gets set by SQLite DB (AutoIncrement)
                    LayerPosition = 1,
                    LayerThickness = Convert.ToDouble(thickness_TextBox.Text),
                    MaterialId = material.MaterialId,
                    Material = material,
                    ElementId = FO0_LandingPage.SelectedElement.ElementId,
                    Element = FO0_LandingPage.SelectedElement
                };
                DatabaseAccess.CreateLayer(layer);
                this.Close();
            }
        }

        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMaterialsFromQuery(searchBar_Materials.Text);
        }

        private void button_CreateMaterial_Clicked(object sender, EventArgs e)
        {
            //this.ShowPopup(new CreateMaterialPopup(distinctCategories));
        }

        private void collectionView_Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (collectionView_Categories.SelectedItem is null)
                return;

            selectedCategory = collectionView_Categories.SelectedItem.ToString();
            if (selectedCategory is null)
                return;

            if (selectedCategory == "Alle anzeigen")
                LoadMaterialsByCategory();
            else
                LoadMaterialsByCategory(selectedCategory);
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
