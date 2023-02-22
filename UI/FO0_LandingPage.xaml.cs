using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class FO0_LandingPage : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Class Variables
        // Initialize + Assign empty List to avoid null value
        public static int ProjectId { get; private set; } = 1; // TODO change Hardcoded value
        public static int SelectedElementId { get; set; } = -1; // Default: no Element Selected

        // Instance Variables - only for "MainPage" Instances
        private Project currentProject = DatabaseAccess.QueryProjectById(ProjectId);

        // Constructor
        public FO0_LandingPage()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)

            // Event Subscription
            DatabaseAccess.ElementsChanged += DB_ElementsChanged;
        }

        // Event handlers - Subscriber
        public void DB_ElementsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Update Class Variable (Project)
            //Project.Elements = DatabaseAccess.QueryElementsByProjectId(1); //TODO: hardcoded

        }        

        // Custom Methods

        // Click on existing Element from WrapPanel
        /*private void elementPanel_Button_Click(object sender, RoutedEventArgs e)
        {
            int elementId = Convert.ToInt32((sender as Button).Content);
            SelectedElement = DatabaseAccess.QueryElementById(elementId);
            MainWindow.SetPage("Setup");
        }*/

        // Right click only on Panel Button(!) opens Context Menu
        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu contextMenu = element_ItemsControl.FindResource("WrapPanel_ContextMenu") as ContextMenu;
            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }
        // Context Menu - Delete
        /*private void delete_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            Button button = contextMenu.PlacementTarget as Button;
            int elementId = Convert.ToInt16(button.Content);
            DatabaseAccess.DeleteElementById(elementId);
        }
        // Context Menu - Edit
        private void edit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            Button button = contextMenu.PlacementTarget as Button;
            int elementId = Convert.ToInt16(button.Content);
            Element editElement = DatabaseAccess.QueryElementById(elementId);
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new NewElementWindow(editElement);

            window.ShowDialog(); // Open as modal (Parent window pauses, waiting for the window to be closed)
        }
        // Context Menu - Lock
        private void lock_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }*/

        private void closeApp_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Main.Close();
        }

        private void selectUsage1_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                currentProject.IsResidentialUsage = true; // Update Class Variable
            DatabaseAccess.UpdateProject(currentProject); // Update in Database
        }

        private void selectUsage0_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                currentProject.IsNonResidentialUsage = true; // Update Class Variable
            DatabaseAccess.UpdateProject(currentProject); // Update in Database
        }

        private void selectAge1_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                currentProject.IsNewConstruction = true; // Update Class Variable
            DatabaseAccess.UpdateProject(currentProject); // Update in Database
        }

        private void selectAge0_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                currentProject.IsExistingConstruction = true; // Update Class Variable
            DatabaseAccess.UpdateProject(currentProject); // Update in Database
        }
    }
}
