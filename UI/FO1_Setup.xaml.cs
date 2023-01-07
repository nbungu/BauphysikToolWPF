﻿using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.ViewModels;
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
        //

        // Instance Variables - only for "MainPage" instances
        //

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_Setup()
        {
            InitializeComponent();                          // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            LoadLayers();                                   // Init Canvas and Layers
            LoadEnvironmentData();                          // loads saved environment data
            DatabaseAccess.LayersChanged += DB_LayersChanged;     // register with an event (when Layers have been added)
        }

        // event handlers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            ReorderLayerPosition();
            LoadLayers();
        }

        // custom Methods
        private void LoadLayers()
        {
            List<Layer> layers = DatabaseAccess.GetLayers();
            layers_ListView.ItemsSource = layers;       // Update LVItems
            new DrawLayerCanvas(layers, layers_Canvas); // Update Canvas
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
            Ti_Category_Picker.SelectedItem = UserSaved.Ti;
            Rsi_Category_Picker.SelectedItem = UserSaved.Rsi;
            RelFi_Category_Picker.SelectedItem = UserSaved.Rel_Fi;
            Te_Category_Picker.SelectedItem = UserSaved.Te;
            Rse_Category_Picker.SelectedItem = UserSaved.Rse;
            RelFe_Category_Picker.SelectedItem = UserSaved.Rel_Fe;
            // -> invokes SelectedIndexChanged
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

            string tiKey = Ti_Category_Picker.SelectedItem.ToString();
            UserSaved.Ti = tiKey;

            // Set corresponding value in the TB
            double tiValue = DatabaseAccess.QueryEnvVarsByCategory("Ti").Where(e => e.Key == tiKey).First().Value;
            UserSaved.Ti_Value = tiValue;
            Ti_Input.Text = tiValue.ToString();
        }

        private void Te_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Te_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string teKey = Te_Category_Picker.SelectedItem.ToString();
            UserSaved.Te = teKey;

            // Set corresponding value in the TB
            double teValue = DatabaseAccess.QueryEnvVarsByCategory("Te").Where(e => e.Key == teKey).First().Value;
            UserSaved.Te_Value = teValue;
            Te_Input.Text = teValue.ToString();
        }

        private void Rsi_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Rsi_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string rsiKey = Rsi_Category_Picker.SelectedItem.ToString();
            UserSaved.Rsi = rsiKey;

            // Set corresponding value in the TB
            double rsiValue = DatabaseAccess.QueryEnvVarsByCategory("Rsi").Where(e => e.Key == rsiKey).First().Value;
            UserSaved.Rsi_Value = rsiValue;
            Rsi_Input.Text = rsiValue.ToString();
        }

        private void Rse_Category_Picker_SelectionChanged(object sender, EventArgs e)
        {
            if (Rse_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string rseKey = Rse_Category_Picker.SelectedItem.ToString();
            UserSaved.Rse = rseKey;

            //Set corresponding value in the TB
            double rseValue = DatabaseAccess.QueryEnvVarsByCategory("Rse").Where(e => e.Key == rseKey).First().Value;
            UserSaved.Rse_Value = rseValue;
            Rse_Input.Text = rseValue.ToString();
        }

        private void RelFe_Category_Picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RelFe_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = RelFe_Category_Picker.SelectedItem.ToString();
            UserSaved.Rel_Fe = key;

            //Set corresponding value in the TB
            double value = DatabaseAccess.QueryEnvVarsByCategory("Rel_Fe").Where(e => e.Key == key).First().Value;
            UserSaved.Rel_Fe_Value = value;
            RelFe_Input.Text = value.ToString();
        }

        private void RelFi_Category_Picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RelFi_Category_Picker.SelectedIndex == -1) // empty selection
                return;

            string key = RelFi_Category_Picker.SelectedItem.ToString();
            UserSaved.Rel_Fi = key;

            //Set corresponding value in the TB
            double value = DatabaseAccess.QueryEnvVarsByCategory("Rel_Fi").Where(e => e.Key == key).First().Value;
            UserSaved.Rel_Fi_Value = value;
            RelFi_Input.Text = value.ToString();
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,-]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);

            double userInput = Convert.ToDouble(e.Text); // TODO error when negative

            switch (((TextBox)sender).Name)
            {
                case "Ti_Input":
                    UserSaved.Ti = "";
                    UserSaved.Ti_Value = userInput;
                    Ti_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Te_Input":
                    UserSaved.Te = "";
                    UserSaved.Te_Value = userInput;
                    Te_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rsi_Input":
                    UserSaved.Rsi = "";
                    UserSaved.Rsi_Value = userInput;
                    Rsi_Category_Picker.SelectedIndex = -1; // empty selection
                    return;
                case "Rse_Input":
                    UserSaved.Rse = "";
                    UserSaved.Rse_Value = userInput;
                    Rse_Category_Picker.SelectedIndex = -1; // empty selection
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
