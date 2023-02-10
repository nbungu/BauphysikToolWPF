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
        // Initialize with empty List to avoid null value
        public static Project Project { get; private set; } = new Project();
        public static List<Element> Elements { get; private set; } = new List<Element>();
        public static Element SelectedElement { get; set; } = new Element();

        public FO0_LandingPage()
        {
            // To be able to access the related Children of these Classes, fetch parent of the child directly from DB. 
            Project = DatabaseAccess.QueryProjectById(1); //Hardcoded. TODO change
            Elements = DatabaseAccess.QueryElementsByProjectId(Project.ProjectId);
            InitializeComponent();
            DatabaseAccess.ElementsChanged += DB_ElementsChanged; // register with an event (when Elements have been changed)

            //TODO: XAML Binding on IsChecked doest work somehow. Define here instead
            selectUsage1_Button.IsChecked = Project.IsResidentialUsage;
            selectUsage0_Button.IsChecked = Project.IsNonResidentialUsage;
            selectAge1_Button.IsChecked = Project.IsNewConstruction;
            selectAge0_Button.IsChecked = Project.IsExistingConstruction;
        }

        // event handlers - subscribers
        public void DB_ElementsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            Elements = DatabaseAccess.QueryElementsByProjectId(Project.ProjectId);
            element_ItemsControl.ItemsSource = Elements; // Updates the ItemsSource. Initial object is fetched by XAML via ViewModel
        }

        // custom Methods
        private void createNewElement_Button_Click(object sender, RoutedEventArgs e)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new NewElementWindow(Project);

            //window.Owner = this;
            window.ShowDialog();   // Open as modal (Parent window pauses, waiting for the window to be closed)
            //window.Show();       // Open as modeless
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
            var window = new NewElementWindow(Project, editElement);

            window.ShowDialog();   // Open as modal (Parent window pauses, waiting for the window to be closed)
        }

        private void closeApp_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Main.Close();
        }

        private void selectUsage1_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsResidentialUsage = true;

            DatabaseAccess.UpdateProject(Project);
        }

        private void selectUsage0_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsNonResidentialUsage = true;

            DatabaseAccess.UpdateProject(Project);
        }

        private void selectAge1_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsNewConstruction = true;

            DatabaseAccess.UpdateProject(Project);
        }

        private void selectAge0_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked == true)
                Project.IsExistingConstruction = true;

            DatabaseAccess.UpdateProject(Project);
        }
    }
}
