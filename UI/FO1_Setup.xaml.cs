﻿using BauphysikToolWPF.ComponentCalculations;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO1_Setup.xaml
    /// </summary>
    public partial class FO1_Setup : UserControl
    {
        // Class Variables - Belongs to the Class-Type itself and stays the same
        public static List<Layer> Layers { get; private set; } = new List<Layer>(); // avoid null value
        public static List<EnvVars> EnvVars { get; private set; } = new List<EnvVars>(); // avoid null value

        // Instance Variables - only for "MainPage" instances
        //

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_Setup()
        {
            Layers = DatabaseAccess.GetLayers();                // default init value
            EnvVars = DatabaseAccess.GetEnvVars();              // default init value
            InitializeComponent();                              // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
            new DrawLayerCanvas(Layers, layers_Canvas);         // Initial Draw of the Canvas
            DatabaseAccess.LayersChanged += DB_LayersChanged;   // register with an event (when Layers have been added)
        }

        // event handlers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            Layers = DatabaseAccess.GetLayers();        // Update Layer variable in this class
            ReorderLayerPosition(Layers);               // Establish correct LayerPosition 
            LoadLayers(Layers);                         // Redraw Canvas, Update LVItemsSource
        }

        // custom Methods
        private void LoadLayers(List<Layer> layers)
        {
            layers_ListView.ItemsSource = layers;       // Update LVItems
            new DrawLayerCanvas(layers, layers_Canvas); // Update Canvas
        }

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
            //window.ShowDialog();    // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.Show();          // Open as modeless
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

        }

        private void Ti_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Ti_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = Ti_Category_Picker.SelectedItem.ToString();
            double val = EnvVars.Where(e => e.Category == "Ti").ToList().Find(e => e.Key == key).Value;
            UserSaved.Ti = new KeyValuePair<string, double>( key, val);

            Ti_Input.Text = val.ToString();
        }

        private void Rsi_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Rsi_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = Rsi_Category_Picker.SelectedItem.ToString();
            double val = EnvVars.Where(e => e.Category == "Rsi").ToList().Find(e => e.Key == key).Value;
            UserSaved.Rsi = new KeyValuePair<string, double>(key, val);

            // Set corresponding value in the TB
            Rsi_Input.Text = val.ToString();
        }
        private void Rel_Fi_Category_Picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Rel_Fi_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = Rel_Fi_Category_Picker.SelectedItem.ToString();
            double val = EnvVars.Where(e => e.Category == "Rel_Fi").ToList().Find(e => e.Key == key).Value;
            UserSaved.Rel_Fi = new KeyValuePair<string, double>(key, val);

            //Set corresponding value in the TB
            Rel_Fi_Input.Text = val.ToString();
        }

        private void Te_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Te_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = Te_Category_Picker.SelectedItem.ToString();
            double val = EnvVars.Where(e => e.Category == "Te").ToList().Find(e => e.Key == key).Value;
            UserSaved.Te = new KeyValuePair<string, double>(key, val);

            Te_Input.Text = val.ToString();
        }
        private void Rse_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Rse_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = Rse_Category_Picker.SelectedItem.ToString();
            double val = EnvVars.Where(e => e.Category == "Rse").ToList().Find(e => e.Key == key).Value;
            UserSaved.Rse = new KeyValuePair<string, double>(key, val);

            // Set corresponding value in the TB
            Rse_Input.Text = val.ToString();
        }

        private void Rel_Fe_Category_Picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Rel_Fe_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = Rel_Fe_Category_Picker.SelectedItem.ToString();
            double val = DatabaseAccess.QueryEnvVarsByCategory("Rel_Fe").Where(e => e.Key == key).First().Value;
            UserSaved.Rel_Fe = new KeyValuePair<string, double>(key, val);

            Rel_Fe_Input.Text = val.ToString();
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
                    UserSaved.Ti = new KeyValuePair<string, double>("", Convert.ToDouble(Ti_Input.Text + userInput));
                    Ti_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Te_Input":
                    UserSaved.Te = new KeyValuePair<string, double>("", Convert.ToDouble(Te_Input.Text + userInput));
                    Te_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rsi_Input":
                    UserSaved.Rsi = new KeyValuePair<string, double>("", Convert.ToDouble(Rsi_Input.Text + userInput));
                    Rsi_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rse_Input":
                    UserSaved.Rse = new KeyValuePair<string, double>("", Convert.ToDouble(Rse_Input.Text + userInput));
                    Rse_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rel_Fi_Input":
                    UserSaved.Rel_Fi = new KeyValuePair<string, double>("", Convert.ToDouble(Rel_Fi_Input.Text + userInput));
                    Rel_Fi_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rel_Fe_Input":
                    UserSaved.Rel_Fe = new KeyValuePair<string, double>("", Convert.ToDouble(Rel_Fe_Input.Text + userInput));
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
