using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
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

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_Setup()
        {

            InitializeComponent();                          // Initializes xaml objects
                                                            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
                                                            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
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

            new DrawLayerCanvas(layers, layers_Canvas); // Draw Rectangles
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
            // ItemsSource gets nulled after changing window due to new Class Instance of FO1_Summary.cs
            Ti_Category_Picker.ItemsSource = Temperatures.TiData.Keys.ToList();        //Pass data to picker
            Rsi_Category_Picker.ItemsSource = RSurfaces.RsiData.Keys.ToList();  //Pass data to picker
            Te_Category_Picker.ItemsSource = Temperatures.TeData.Keys.ToList();        //Pass data to picker
            Rse_Category_Picker.ItemsSource = RSurfaces.RseData.Keys.ToList();  //Pass data to picker

            // Set initial values if no user data saved
            Ti_Category_Picker.SelectedItem = (Temperatures.selectedTi == null) ? Temperatures.TiData.First().Key : Temperatures.selectedTi.First().Key; //set default entry -> invokes SelectedIndexChanged            
            Rsi_Category_Picker.SelectedItem = (RSurfaces.selectedRsi == null) ? RSurfaces.RsiData.First().Key : RSurfaces.selectedRsi.First().Key; //set default entry -> invokes SelectedIndexChanged              
            Te_Category_Picker.SelectedItem = (Temperatures.selectedTe == null) ? Temperatures.TeData.First().Key : Temperatures.selectedTe.First().Key; //set default entry -> invokes SelectedIndexChanged          
            Rse_Category_Picker.SelectedItem = (RSurfaces.selectedRse == null) ? RSurfaces.RseData.First().Key : RSurfaces.selectedRse.First().Key; //set default entry -> invokes SelectedIndexChanged
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
            double tiVal = Temperatures.TiData.GetValueOrDefault(Ti_Category_Picker.SelectedItem.ToString());
            Ti_Input.Text = tiVal.ToString(); //get corresponding value
            Temperatures.selectedTi = new Dictionary<string, double>() { { Ti_Category_Picker.SelectedItem.ToString(), tiVal } };
        }

        private void Te_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double teVal = Temperatures.TeData.GetValueOrDefault(Te_Category_Picker.SelectedItem.ToString());
            Te_Input.Text = teVal.ToString(); //get corresponding value
            Temperatures.selectedTe = new Dictionary<string, double>() { { Te_Category_Picker.SelectedItem.ToString(), teVal } };
        }

        private void Rsi_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double rsiVal = RSurfaces.RsiData.GetValueOrDefault(Rsi_Category_Picker.SelectedItem.ToString());
            Rsi_Input.Text = rsiVal.ToString(); //get corresponding value
            RSurfaces.selectedRsi = new Dictionary<string, double>() { { Rsi_Category_Picker.SelectedItem.ToString(), rsiVal } };
        }

        private void Rse_Category_Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double rseVal = RSurfaces.RseData.GetValueOrDefault(Rse_Category_Picker.SelectedItem.ToString());
            Rse_Input.Text = rseVal.ToString(); //get corresponding value
            RSurfaces.selectedRse = new Dictionary<string, double>() { { Rse_Category_Picker.SelectedItem.ToString(), rseVal } };
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,-]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);

            double convUserInput = Convert.ToDouble(e.Text); // TODO error when negative

            switch (((TextBox)sender).Name)
            {
                case "Ti_Input":
                    Temperatures.selectedTi = new Dictionary<string, double>() { { Ti_Category_Picker.SelectedItem.ToString(), convUserInput } };
                    return;
                case "Te_Input":
                    Temperatures.selectedTe = new Dictionary<string, double>() { { Te_Category_Picker.SelectedItem.ToString(), convUserInput } };
                    return;
                case "Rsi_Input":
                    RSurfaces.selectedRsi = new Dictionary<string, double>() { { Rsi_Category_Picker.SelectedItem.ToString(), convUserInput } };
                    return;
                case "Rse_Input":
                    RSurfaces.selectedRse = new Dictionary<string, double>() { { Rse_Category_Picker.SelectedItem.ToString(), convUserInput } };
                    return;
                default: throw new ArgumentException("Could not assign value");
            }

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
            new DrawLayerCanvas(layers_ListView.ItemsSource as List<Layer>, layers_Canvas);

        }
    }
}
