using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für LandingPage.xaml
    /// </summary>
    public partial class FO0_LandingPage : UserControl
    {
        public static List<Element> Elements { get; private set; } = new List<Element>(); // avoid null value
        public static Element SelectedElement { get; set; }
        public FO0_LandingPage()
        {
            Elements = DatabaseAccess.GetElements();
            InitializeComponent();
            DatabaseAccess.ElementsChanged += DB_ElementsChanged;   // register with an event (when Elements have been changed)
        }

        // event handlers
        public void DB_ElementsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            Elements = DatabaseAccess.GetElements();
            element_ItemsControl.ItemsSource = Elements;
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

        private void openElement_Button_Click(object sender, RoutedEventArgs e)
        {
            int elementId = Convert.ToInt32((sender as Button).Content);
            SelectedElement = DatabaseAccess.QueryElementsById(elementId);
            MainWindow.SetPage("Setup");
        }

        private void delete_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            Button button = contextMenu.PlacementTarget as Button;
            int elementId = Convert.ToInt16(button.Content);
            DatabaseAccess.DeleteElementById(elementId);
        }

        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu contextMenu = element_ItemsControl.FindResource("ButtonContextMenu") as ContextMenu;
            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }

        private void rename_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
