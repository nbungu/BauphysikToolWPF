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
        public static List<Element> Elements { get; private set; } = new List<Element>(); // avoid null value
        public static Element SelectedElement { get; set; }

        public FO0_LandingPage()
        {
            Elements = DatabaseAccess.GetElements();
            InitializeComponent();
            DatabaseAccess.ElementsChanged += DB_ElementsChanged; // register with an event (when Elements have been changed)
        }

        // event handlers - subscribers
        public void DB_ElementsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            Elements = DatabaseAccess.GetElements();
            element_ItemsControl.ItemsSource = Elements; // Updates the ItemsSource. Initial object is fetched by XAML via ViewModel
        }

        // custom Methods
        private void createNewElement_Button_Click(object sender, RoutedEventArgs e)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new NewElementWindow();

            //window.Owner = this;
            window.ShowDialog();   // Open as modal (Parent window pauses, waiting for the window to be closed)
            //window.Show();       // Open as modeless
        }

        // Click on existing Element from WrapPanel
        private void elementPanel_Button_Click(object sender, RoutedEventArgs e)
        {
            int elementId = Convert.ToInt32((sender as Button).Content);
            SelectedElement = DatabaseAccess.QueryElementsById(elementId);
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
            Element editElement = DatabaseAccess.QueryElementsById(elementId);
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new NewElementWindow(editElement);

            window.ShowDialog();   // Open as modal (Parent window pauses, waiting for the window to be closed)
        }


        private void closeApp_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Main.Close();
        }
    }
}
