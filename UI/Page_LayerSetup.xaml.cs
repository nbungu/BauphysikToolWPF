using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class Page_LayerSetup : UserControl
    {
        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call of this Class.

        // Class Variables - Belongs to the Class-Type itself and stay the same


        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            if (UserSaved.SelectedElement != null)
            {
                UserSaved.SelectedElement.SortLayers();
                UserSaved.SelectedElement.AssignEffectiveLayers();
                UserSaved.SelectedElement.AssignInternalIdsToLayers();
            }

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
        }

        // Save current canvas as image, just before closing Page_LayerSetup Page
        // 'Unloaded' event was called after FO0 Initialize();
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only save if leaving this page
            if (IsVisible) return;
            UserSaved.SelectedElement.Layers.ForEach(l => l.IsSelected = false);
            UserSaved.SelectedElement.Image = (UserSaved.SelectedElement.Layers.Count != 0) ? SaveCanvas.SaveAsBLOB(LayersCanvas) : Array.Empty<byte>();
        }

        // Handle Custom User Input - Regex Check
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
