﻿using System;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Helper;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;

namespace BauphysikToolWPF.UI
{
    public partial class FO1_SetupLayer : UserControl
    {
        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call of this Class.

        //

        // Class Variables - Belongs to the Class-Type itself and stay the same
        public static bool RecalculateTemp { get; set; } = true;
        public static bool RecalculateDynTemp { get; set; } = true;
        public static bool RecalculateGlaser { get; set; } = true;

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_SetupLayer()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            // Event Subscription - Register with Events
            DatabaseAccess.LayersChanged += DB_LayersChanged;
            UserSaved.EnvVarsChanged += Session_EnvVarsChanged;
            UserSaved.SelectedElementChanged += Session_SelectedElementChanged;
        }

        // event handlers - subscribers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateDynTemp = true;
            RecalculateGlaser = true;
        }
        public void Session_EnvVarsChanged()
        {
            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateDynTemp = true;
            RecalculateGlaser = true;
        }

        public void Session_SelectedElementChanged()
        {
            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateDynTemp = true;
            RecalculateGlaser = true;
        }

        // custom Methods

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
            if (DatabaseAccess.UpdateElementEnvVars(elemEnvVars) == 0) DatabaseAccess.CreateElementEnvVars(elemEnvVars); // If no row is updated ( == 0), create a new one
        }

        // UI Methods

        // Save current canvas as image, just before closing FO1_SetupLayer Page
        // 'Unloaded' event was called after FO0 Initialize();
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only save if leaving this page
            if (IsVisible) return;

            UserSaved.SelectedElement.Image = (UserSaved.SelectedElement.Layers.Count != 0) ? SaveCanvas.SaveAsBLOB(layers_ItemsControl) : Array.Empty<byte>();
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // If typed input over a selected Text, delete the old value in the TB
                if (tb.SelectedText != "")
                    tb.Text = "";

                //Handle the input
                string userInput = e.Text;
                Regex regex = new Regex("[^0-9,-]+"); //regex that matches disallowed text
                e.Handled = regex.IsMatch(userInput);

                // only allow one decimal point
                if (userInput == "," && tb.Text.IndexOf(',') > -1)
                {
                    e.Handled = true;
                }
                // only allow one minus char
                if (userInput == "-" && tb.Text.IndexOf('-') > -1)
                {
                    e.Handled = true;
                }
            }
        }
    }


}
