using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO1_Setup.xaml
    /// </summary>
    public partial class FO1_Setup : UserControl
    {
        // Class Variables - Belongs to the Class-Type itself and stay the same
        public static int ElementId { get; set; } = -1; // Default: no element set

        // Recalculate Flags - Save computation time by avoiding unnecessary new instances
        public static bool RecalculateTemp { get; set; } = false;
        public static bool RecalculateGlaser { get; set; } = false;

        // Instance Variables - only for "MainPage" Instances
        //

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_Setup()
        {
            // If Element is not set (-1) or has changed, Update Class Variables
            if (ElementId != FO0_LandingPage.SelectedElement.ElementId)
            {
                ElementId = FO0_LandingPage.SelectedElement.ElementId;
                RecalculateTemp = true;
                RecalculateGlaser = true;
            }

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            // Drawing
            LoadDropDownSelections(); // Loads last selection of the dropdown box (Picker)
            new DrawLayerCanvas(layers_Canvas, FO0_LandingPage.SelectedElement.Layers);         // Initial Draw of the Canvas
            new DrawMeasurementLine(measurement_Grid, FO0_LandingPage.SelectedElement.Layers);  // Initial Draw of the measurement line

            // Event Subscription
            DatabaseAccess.LayersChanged += DB_LayersChanged;   // register with event, when Layers changed
            UserSaved.EnvVarsChanged += Session_EnvVarsChanged; // register with event, when EnvVars changed
        }

        // event handlers - subscribers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Update SelectedElement Class Variable
            FO0_LandingPage.SelectedElement = DatabaseAccess.QueryElementById(ElementId);

            // Bring them in correct order again
            ReorderLayerPosition(FO0_LandingPage.SelectedElement.Layers);        

            // Update UI
            layers_ListView.ItemsSource = FO0_LandingPage.SelectedElement.Layers;               // Update LVItemsSource
            new DrawLayerCanvas(layers_Canvas, FO0_LandingPage.SelectedElement.Layers);         // Redraw Canvas
            new DrawMeasurementLine(measurement_Grid, FO0_LandingPage.SelectedElement.Layers);  // Redraw measurement line

            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateGlaser = true;
        }
        public void Session_EnvVarsChanged()
        {
            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateGlaser = true;
        }

        // custom Methods
        // Fixes wrong layer Positions in this Element after deleting Layers.
        private void ReorderLayerPosition(List<Layer> layers)
        {
            if (layers.Count > 0)
            {
                // Update the Id property of the remaining objects
                for (int i = 0; i < layers.Count; i++)
                {
                    layers[i].LayerPosition = i + 1;
                    DatabaseAccess.UpdateLayer(layers[i]);
                }
            }
        }
        private void LoadDropDownSelections()
        {
            // TODO implement in xaml
            Ti_ComboBox.SelectedIndex = UserSaved.ComboBoxSelection["Ti_ComboBox"];
            Te_ComboBox.SelectedIndex = UserSaved.ComboBoxSelection["Te_ComboBox"];
            Rsi_ComboBox.SelectedIndex = UserSaved.ComboBoxSelection["Rsi_ComboBox"];
            Rse_ComboBox.SelectedIndex = UserSaved.ComboBoxSelection["Rse_ComboBox"];
            Rel_Fi_ComboBox.SelectedIndex = UserSaved.ComboBoxSelection["Rel_Fi_ComboBox"];
            Rel_Fe_ComboBox.SelectedIndex = UserSaved.ComboBoxSelection["Rel_Fe_ComboBox"];
        }

        public void UpdateElementEnvVars(int elementID, EnvVars envVar)
        {
            // Add m:n realtion to Database
            ElementEnvVars elemEnvVars = new ElementEnvVars()
            {
                //Id gets set by SQLite (Autoincrement)
                ElementId = elementID,
                EnvVarId = envVar.EnvVarId,
            };
            // Only insert every envVar once!            
            if (DatabaseAccess.UpdateElementEnvVars(elemEnvVars) == 0)
                // If no row is updated ( == 0), create a new one
                DatabaseAccess.CreateElementEnvVars(elemEnvVars);
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
        private void Ti_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserSaved.ComboBoxSelection[(sender as ComboBox).Name] = (sender as ComboBox).SelectedIndex; // Save current Selection

            // On custom User Input
            if ((sender as ComboBox).SelectedIndex == -1)
                return;

            // On Selection from List
            string item = (sender as ComboBox).SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Ti").Find(e => e.Comment == item);
            Ti_Input.Text = currentEnvVar.Value.ToString();
            UserSaved.Ti = currentEnvVar.Value;

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rsi_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            UserSaved.ComboBoxSelection[(sender as ComboBox).Name] = (sender as ComboBox).SelectedIndex; // Save current Selection

            // On custom User Input
            if ((sender as ComboBox).SelectedIndex == -1)
                return;

            // On Selection from List
            string item = (sender as ComboBox).SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Find(e => e.Comment == item);
            Rsi_Input.Text = currentEnvVar.Value.ToString();
            UserSaved.Rsi = currentEnvVar.Value;

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rel_Fi_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            UserSaved.ComboBoxSelection[(sender as ComboBox).Name] = (sender as ComboBox).SelectedIndex; // Save current Selection
            
            // On custom User Input
            if ((sender as ComboBox).SelectedIndex == -1)
                return;

            // On Selection from List
            string item = (sender as ComboBox).SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Find(e => e.Comment == item);
            Rel_Fi_Input.Text = currentEnvVar.Value.ToString();
            UserSaved.Rel_Fi = currentEnvVar.Value;

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Te_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            UserSaved.ComboBoxSelection[(sender as ComboBox).Name] = (sender as ComboBox).SelectedIndex; // Save current Selection
            
            if ((sender as ComboBox).SelectedIndex == -1) // empty selection
                return;

            string item = (sender as ComboBox).SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Te").Find(e => e.Comment == item);
            Te_Input.Text = currentEnvVar.Value.ToString();
            UserSaved.Te = currentEnvVar.Value;

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rse_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            UserSaved.ComboBoxSelection[(sender as ComboBox).Name] = (sender as ComboBox).SelectedIndex; // Save current Selection
            
            if ((sender as ComboBox).SelectedIndex == -1) // empty selection
                return;

            string item = (sender as ComboBox).SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rse").Find(e => e.Comment == item);
            Rse_Input.Text = currentEnvVar.Value.ToString();
            UserSaved.Rse = currentEnvVar.Value;

            // Add m:n realtion to Database
            UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rel_Fe_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            UserSaved.ComboBoxSelection[(sender as ComboBox).Name] = (sender as ComboBox).SelectedIndex; // Save current Selection

            if ((sender as ComboBox).SelectedIndex == -1) // empty selection
                return;

            string item = (sender as ComboBox).SelectedItem.ToString();
            EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Find(e => e.Comment == item);
            Rel_Fe_Input.Text = currentEnvVar.Value.ToString();
            UserSaved.Rel_Fe = currentEnvVar.Value;            

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
            if (userInput == "." && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            //set new value as UserSaved Data
            switch ((sender as TextBox).Name)
            {
                case "Ti_Input":
                    UserSaved.Ti = Convert.ToDouble(Ti_Input.Text + userInput);
                    Ti_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Te_Input":
                    UserSaved.Te = Convert.ToDouble(Te_Input.Text + userInput);
                    Te_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rsi_Input":
                    UserSaved.Rsi = Convert.ToDouble(Rsi_Input.Text + userInput);
                    Rsi_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rse_Input":
                    UserSaved.Rse = Convert.ToDouble(Rse_Input.Text + userInput);
                    Rse_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rel_Fi_Input":
                    UserSaved.Rel_Fi = Convert.ToDouble(Rel_Fi_Input.Text + userInput);
                    Rel_Fi_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rel_Fe_Input":
                    UserSaved.Rel_Fe = Convert.ToDouble(Rel_Fe_Input.Text + userInput);
                    Rel_Fe_ComboBox.SelectedIndex = -1; // set empty selection
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
            new DrawLayerCanvas(layers_Canvas, layers_ListView.ItemsSource as List<Layer>);
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // TODO fix path problem
            //DrawLayerCanvas.SaveAsImg(layers_Canvas, "./Resources/ElementImages/");
        }
    }
}
