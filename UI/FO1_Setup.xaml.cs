using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO1_Setup.xaml
    /// </summary>
    public partial class FO1_Setup : UserControl //Page
    {
        // Class Variables - Belongs to the Class-Type itself and stays the same
        //

        // Instance Variables - only for "MainPage" instances
        //

        // (Instance-) Contructor - when 'new' Keyword is used to create class
        public FO1_Setup()
        {
            InitializeComponent();                          // loads xaml of this page
            LoadEnvironmentData();                          // loads saved environment data
            LoadLayers();                                   // Initial loading of the layers
            DatabaseAccess.LayerAdded += DB_LayerAdded;     // register with an event (when Layers have been added)
            DatabaseAccess.LayerDeleted += DB_LayerDeleted; // register with an event (when Layers have been deleted)
        }

        // event handler
        public void DB_LayerAdded() // has to match the signature of the delegate (return type void, no input parameters)
        {
            LoadLayers();
        }

        public void DB_LayerDeleted()
        {
            ReorderLayerPosition();
            LoadLayers();
        }

        // override Methods
        /*protected override void OnAppearing() //default Methode wird überschrieben
        {
            base.OnAppearing(); // base refers to 'ContentPage', since F01_Setup inherits all its public members
        }*/

        // custom Methods
        private void LoadLayers()
        {
            List<Layer> layers = DatabaseAccess.GetLayers();

            layers_ListView.ItemsSource = layers;                   // Refresh ListView Items
            DrawLayerCanvas.DrawRectanglesFromLayers(layers, layers_Canvas);   // Draw Rectangles
        }

        private void ReorderLayerPosition()
        {
            List<Layer> layers = DatabaseAccess.GetLayers();

            if (layers.Count > 0)
            {
                // Update the Id property of the remaining objects
                for (int i = 0; i < layers.Count; i++)
                {
                    layers[i].LayerPosition = i + 1; // TODO: im getter die LayerPosition validieren: Problem PrimaryKeys können nicht verändert werden
                    DatabaseAccess.UpdateLayer(layers[i]);
                }
            }
        }

        private void LoadEnvironmentData()
        {
            var tiData = ReferenceTemp.GetTiData();
            Ti_Category_Picker.ItemsSource = tiData.Keys.ToList(); //Pass data to picker
            Ti_Category_Picker.SelectedItem = tiData.Keys.ToList().First(); //set default entry
            Ti_Input.Text = tiData.GetValueOrDefault(Ti_Category_Picker.SelectedItem.ToString()).ToString(); //get corresponding value

            var rsiData = SurfaceResistance.GetRsiData();
            Rsi_Category_Picker.ItemsSource = rsiData.Keys.ToList(); //Pass data to picker
            Rsi_Category_Picker.SelectedItem = rsiData.Keys.ToList().First(); //set default entry
            Rsi_Input.Text = rsiData.GetValueOrDefault(Rsi_Category_Picker.SelectedItem.ToString()).ToString(); //get corresponding value

            var teData = ReferenceTemp.GetTeData();
            Te_Category_Picker.ItemsSource = teData.Keys.ToList(); //Pass data to picker
            Te_Category_Picker.SelectedItem = teData.Keys.ToList().First(); //set default entry
            Te_Input.Text = teData.GetValueOrDefault(Te_Category_Picker.SelectedItem.ToString()).ToString(); //get corresponding value

            var rseData = SurfaceResistance.GetRseData();
            Rse_Category_Picker.ItemsSource = rseData.Keys.ToList(); //Pass data to picker
            Rse_Category_Picker.SelectedItem = rseData.Keys.ToList().First(); //set default entry
            Rse_Input.Text = rseData.GetValueOrDefault(Rse_Category_Picker.SelectedItem.ToString()).ToString(); //get corresponding value
        }


        // UI Methods
        private void AddLayerClicked(object sender, EventArgs e)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new AddLayerWindow();

            //window.Owner = this;
            //window.ShowDialog();    // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.Show();          // Open as modeless
        }

        private void DeleteLayerClicked(object sender, EventArgs e)
        {
            if (layers_ListView.SelectedItem is null)
                return;

            var layer = layers_ListView.SelectedItem as Layer;
            if (layer is null)
                return;

            DatabaseAccess.DeleteLayer(layer);
        }

        private void deleteAllLayers_Button_Clicked(object sender, EventArgs e)
        {
            DatabaseAccess.DeleteAllLayers();
        }

        private void Ti_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double tiVal = ReferenceTemp.GetTiData().GetValueOrDefault(Ti_Category_Picker.SelectedItem.ToString());
            Ti_Input.Text = tiVal.ToString(); //get corresponding value
            ReferenceTemp.selectedTiValue = tiVal;
        }

        private void Te_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double teVal = ReferenceTemp.GetTeData().GetValueOrDefault(Te_Category_Picker.SelectedItem.ToString());
            Te_Input.Text = teVal.ToString(); //get corresponding value
            ReferenceTemp.selectedTeValue = teVal;
        }

        private void Rsi_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double rsiVal = SurfaceResistance.GetRsiData().GetValueOrDefault(Rsi_Category_Picker.SelectedItem.ToString());
            Rsi_Input.Text = rsiVal.ToString(); //get corresponding value
            SurfaceResistance.selectedRsiValue = rsiVal;
        }

        private void Rse_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double rseVal = SurfaceResistance.GetRseData().GetValueOrDefault(Rse_Category_Picker.SelectedItem.ToString());
            Rse_Input.Text = rseVal.ToString(); //get corresponding value
            SurfaceResistance.selectedRseValue = rseVal;
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);
        }

        private void layers_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (layers_ListView.SelectedItem is null)
                return;

            var layer = layers_ListView.SelectedItem as Layer;
            if (layer is null)
                return;

            // deselect every other layer
            foreach (Layer l in layers_ListView.ItemsSource)
            {
                l.IsSelected = false;
            }
            // select only one at a time
            layer.IsSelected = true;

            // Layers are not being updated inside the local DB 
            DrawLayerCanvas.DrawRectanglesFromLayers(layers_ListView.ItemsSource as List<Layer>, layers_Canvas);

        }
    }
}
