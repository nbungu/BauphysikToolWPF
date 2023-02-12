using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class FO0_LandingPage : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Class Variables
        // Initialize + Assign empty List to avoid null value
        public static Project Project { get; private set; } = DatabaseAccess.QueryProjectById(1); //Hardcoded. TODO change
        public static Element SelectedElement { get; set; } = new Element();

        // Constructor
        public FO0_LandingPage()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)

            //TODO: XAML Binding on IsChecked doest work somehow. Define here instead
            SetProjectBuildingSettings();

            // Event Subscription
            DatabaseAccess.ElementsChanged += DB_ElementsChanged;
        }

        // Event handlers - Subscriber
        public void DB_ElementsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Update Class Variable (Project)
            Project.Elements = DatabaseAccess.QueryElementsByProjectId(1); //TODO: hardcoded

            // Update UI
            element_ItemsControl.ItemsSource = Project.Elements; // Initial ItemsSource is fetched by XAML via ViewModel
        }        

        // Custom Methods
        private void SetProjectBuildingSettings()
        {
            selectUsage1_Button.IsChecked = Project.IsResidentialUsage;
            selectUsage0_Button.IsChecked = Project.IsNonResidentialUsage;
            selectAge1_Button.IsChecked = Project.IsNewConstruction;
            selectAge0_Button.IsChecked = Project.IsExistingConstruction;
        }

        private void createNewElement_Button_Click(object sender, RoutedEventArgs e)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new NewElementWindow();
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();
        }

        // Click on existing Element from WrapPanel
        private void elementPanel_Button_Click(object sender, RoutedEventArgs e)
        {
            int elementId = Convert.ToInt32((sender as Button).Content);
            SelectedElement = DatabaseAccess.QueryElementById(elementId);
            MainWindow.SetPage("Setup");
        }

        // Right click on Panel Button opens Context Menu
        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu contextMenu = element_ItemsControl.FindResource("WrapPanel_ContextMenu") as ContextMenu;
            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }
        // Context Menu - Delete
        private void delete_MenuItem_Click(object sender, RoutedEventArgs e)
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
        }

        private void closeApp_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Main.Close();
        }

        private void selectUsage1_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsResidentialUsage = true; // Update Class Variable
            DatabaseAccess.UpdateProject(Project); // Update in Database
        }

        private void selectUsage0_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsNonResidentialUsage = true; // Update Class Variable
            DatabaseAccess.UpdateProject(Project); // Update in Database
        }

        private void selectAge1_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsNewConstruction = true; // Update Class Variable
            DatabaseAccess.UpdateProject(Project); // Update in Database
        }

        private void selectAge0_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsExistingConstruction = true; // Update Class Variable
            DatabaseAccess.UpdateProject(Project); // Update in Database
        }
    }
}
