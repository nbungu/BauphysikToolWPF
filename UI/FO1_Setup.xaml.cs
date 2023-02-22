using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.Helper;
using BauphysikToolWPF.UI.ViewModels;
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
        private static int currentElementId { get; set; } = -1; 

        // Recalculate Flags - Save computation time by avoiding unnecessary new instances
        public static bool RecalculateTemp { get; set; } = false;
        public static bool RecalculateGlaser { get; set; } = false;

        // Instance Variables - only for "MainPage" Instances
        private List<Layer> currentLayers = DatabaseAccess.QueryLayersByElementId(currentElementId);

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_Setup()
        {
            // If Element is not set (-1) or has changed, Update Class Variables
            if (currentElementId != FO0_LandingPage.SelectedElementId)
            {
                currentElementId = FO0_LandingPage.SelectedElementId;
                RecalculateTemp = true;
                RecalculateGlaser = true;
            }

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            // Drawing
            new DrawLayerCanvas(layers_Canvas, currentLayers);         // Initial Draw of the Canvas
            new DrawMeasurementLine(measurement_Grid, currentLayers);  // Initial Draw of the measurement line

            // Event Subscription
            DatabaseAccess.LayersChanged += DB_LayersChanged;   // register with event, when Layers changed
            UserSaved.EnvVarsChanged += Session_EnvVarsChanged; // register with event, when EnvVars changed
        }

        // event handlers - subscribers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Bring them in correct order again
            ReorderLayerPosition(DatabaseAccess.QueryLayersByElementId(currentElementId));

            // Update UI
            new DrawLayerCanvas(layers_Canvas, currentLayers);         // Redraw Canvas
            new DrawMeasurementLine(measurement_Grid, currentLayers);  // Redraw measurement line

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

        // Sets the 'LayerPosition' of a Layer List from 1 to N, without missing values inbetween
        private void ReorderLayerPosition(List<Layer> layers)
        {
            if (layers.Count > 0)
            {
                // Update the Id property of the remaining objects
                for (int i = 0; i < layers.Count; i++)
                {
                    layers[i].LayerPosition = i + 1;
                    DatabaseAccess.UpdateLayer(layers[i], false);
                }
            }
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
        private void Ti_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //EnvVars currentEnvVar = DatabaseAccess.QueryEnvVarsBySymbol("Ti").Find(e => e.Comment == item);
            // Add m:n realtion to Database
            //TODO implement again
            //UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rsi_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            // Add m:n realtion to Database
            //UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rel_Fi_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            // Add m:n realtion to Database
            //UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Te_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            // Add m:n realtion to Database
            //UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rse_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            // Add m:n realtion to Database
            //UpdateElementEnvVars(ElementId, currentEnvVar);
        }
        private void Rel_Fe_ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            // Add m:n realtion to Database
            //UpdateElementEnvVars(ElementId, currentEnvVar);
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
            /*switch ((sender as TextBox).Name)
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
            }*/
        }

        // Redraw on every Click on Canvas to show current selected Layer with blue border
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

            // Redraw to show selected layer, but take Layers from ItemsSource to keep the IsSelected Property
            new DrawLayerCanvas(layers_Canvas, layers_ListView.ItemsSource as List<Layer>);
        }

        // Save current canvas as image, just before closing FO1_Setup Page
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Element currentElement = DatabaseAccess.QueryElementById(currentElementId);
            currentElement.Image = DrawLayerCanvas.SaveAsBLOB(layers_Canvas);

            // Update in Database
            DatabaseAccess.UpdateElement(currentElement);
        }
    }
}
