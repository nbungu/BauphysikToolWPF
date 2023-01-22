using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.ViewModels;
using LiveChartsCore.Kernel;
using Newtonsoft.Json.Linq;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO1_Setup.xaml
    /// </summary>
    public partial class FO1_Setup : UserControl
    {
        // Class Variables - Belongs to the Class-Type itself and stay the same
        public static int ElementId { get; set; } = -1; // no element set
        public static List<Layer> Layers { get; private set; } = new List<Layer>(); // for FO1, FO2 & FO3 ViewModel
        public static List<EnvVars> ElementEnvVars { get; private set; } = new List<EnvVars>();

        // Instance Variables - only for "MainPage" instances
        //

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_Setup()
        {
            // If Element is not set (-1) or has changed, update class variables
            if(ElementId != FO0_LandingPage.SelectedElement.ElementId)
            {
                ElementId = FO0_LandingPage.SelectedElement.ElementId;
                Layers = DatabaseAccess.QueryLayersByElementId(ElementId); //
                ElementEnvVars = DatabaseAccess.QueryElementsById(ElementId).EnvVars;
            }
            InitializeComponent();                              // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
            new DrawLayerCanvas(Layers, layers_Canvas);         // Initial Draw of the Canvas
            new DrawMeasurementLine(measurement_Grid, Layers);  // Initial Draw of the measurement line
            DatabaseAccess.LayersChanged += DB_LayersChanged;   // register with an event (when Layers have been changed)
            DatabaseAccess.ElementEnvVarsChanged += DB_ElementEnvVarsChanged;   // register with an event (when Layers have been changed)
        }

        // event handlers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            Layers = DatabaseAccess.QueryLayersByElementId(ElementId);  // Update Layer variable in this class
            ReorderLayerPosition(Layers);                               // Establish correct LayerPosition 
            layers_ListView.ItemsSource = Layers;       // Update LVItemsSource
            new DrawLayerCanvas(Layers, layers_Canvas); // Redraw Canvas
            new DrawMeasurementLine(measurement_Grid, Layers); // Redraw measurement line
        }
        public void DB_ElementEnvVarsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            ElementEnvVars = DatabaseAccess.QueryElementsById(ElementId).EnvVars;
        }

        // custom Methods
        private void ReorderLayerPosition(List<Layer> layers)
        {
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

        // UI Methods
        private void addLayerClicked(object sender, EventArgs e)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new AddLayerWindow();

            //window.Owner = this;
            window.ShowDialog();    // Open as modal (Parent window pauses, waiting for the window to be closed)
            //window.Show();          // Open as modeless
        }
        private void deleteLayerClicked(object sender, EventArgs e)
        {
            if (layers_ListView.SelectedItem is null)
                return;

            var layer = layers_ListView.SelectedItem as Layer;
            if (layer is null)
                return;

            DatabaseAccess.DeleteLayer(layer);
        }
        private void deleteAllLayersClicked(object sender, EventArgs e)
        {
            DatabaseAccess.DeleteAllLayers();
        }
        private void editLayerClicked(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        public void UpdateElementEnvVars(int elementID, EnvVars envVar)
        {
            // Add m:n realtion to Database
            ElementEnvVars elemEnvVars = new ElementEnvVars()
            {
                //Id gets set by SQLite (Autoincrement)
                ElementId = elementID,
                ElementName = DatabaseAccess.QueryElementsById(elementID).Name,
                EnvVarId = envVar.EnvVarId,
                EnvVarSymbol = envVar.Symbol
            };
            // Only insert every envVar once!            
            if (DatabaseAccess.UpdateElementEnvVars(elemEnvVars) == 0)
                // If no row is updated ( == 0), create a new one
                DatabaseAccess.CreateElementEnvVars(elemEnvVars);   
        }
        private void Ti_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Ti_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string item = Ti_Category_Picker.SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Ti").Find(e => e.Comment == item);

            // Set corresponding value in the TB
            Ti_Input.Text = currentEnvVar.Value.ToString();

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rsi_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Rsi_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string item = Rsi_Category_Picker.SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Find(e => e.Comment == item);
            
            // Set corresponding value in the TB
            Rsi_Input.Text = currentEnvVar.Value.ToString();

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);         
        }
        private void Rel_Fi_Category_Picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Rel_Fi_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string item = Rel_Fi_Category_Picker.SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Find(e => e.Comment == item);

            // Set corresponding value in the TB
            Rel_Fi_Input.Text = currentEnvVar.Value.ToString();

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Te_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Te_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string item = Te_Category_Picker.SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Te").Find(e => e.Comment == item);

            // Set corresponding value in the TB
            Te_Input.Text = currentEnvVar.Value.ToString();

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rse_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Rse_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string item = Rse_Category_Picker.SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rse").Find(e => e.Comment == item);

            // Set corresponding value in the TB
            Rse_Input.Text = currentEnvVar.Value.ToString();

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rel_Fe_Category_Picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Rel_Fe_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string item = Rel_Fe_Category_Picker.SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Find(e => e.Comment == item);

            // Set corresponding value in the TB
            Rel_Fe_Input.Text = currentEnvVar.Value.ToString();

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Handle the input
            string userInput = e.Text;
            Regex regex = new Regex("[^0-9,-]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);

            // only allow one decimal point
            if ((userInput == ".") && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }

            //set new value as UserSaved Data
            switch (((TextBox)sender).Name)
            {
                case "Ti_Input":
                    UserSaved.Ti = Convert.ToDouble(Ti_Input.Text + userInput);
                    Ti_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Te_Input":
                    UserSaved.Te = Convert.ToDouble(Te_Input.Text + userInput);
                    Te_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rsi_Input":
                    UserSaved.Rsi = Convert.ToDouble(Rsi_Input.Text + userInput);
                    Rsi_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rse_Input":
                    UserSaved.Rse = Convert.ToDouble(Rse_Input.Text + userInput);
                    Rse_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rel_Fi_Input":
                    UserSaved.Rel_Fi = Convert.ToDouble(Rel_Fi_Input.Text + userInput);
                    Rel_Fi_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rel_Fe_Input":
                    UserSaved.Rel_Fe = Convert.ToDouble(Rel_Fe_Input.Text + userInput);
                    Rel_Fe_Category_Picker.SelectedIndex = -1; // empty selection
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

            // Redraw to show selected layer 
            new DrawLayerCanvas(layers_ListView.ItemsSource as List<Layer>, layers_Canvas);
        }
    }   
}
