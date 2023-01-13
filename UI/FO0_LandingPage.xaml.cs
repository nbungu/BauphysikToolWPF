using BauphysikToolWPF.SQLiteRepo;
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

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für LandingPage.xaml
    /// </summary>
    public partial class FO0_LandingPage : UserControl
    {
        public FO0_LandingPage()
        {
            //TODO: xaml binding with FO0_ViewModel
            InitializeComponent();
            DB_ElementsChanged();
            DatabaseAccess.ElementsChanged += DB_ElementsChanged;   // register with an event (when Layers have been changed)
        }

        // event handlers
        public void DB_ElementsChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
           //TODO: xaml binding with FO0_ViewModel

           foreach(Element e in DatabaseAccess.GetElements())
           {
                Border border = new Border()
                {
                     CornerRadius = new CornerRadius(8),
                     BorderThickness = new Thickness(0),
                     Margin = new Thickness(8),
                     Background = new SolidColorBrush(Colors.LightSlateGray),
                };
                Label label = new Label();
                label.Content = e.ElementId+"_"+e.Name;
                label.Foreground = new SolidColorBrush(Colors.White);
                label.VerticalAlignment = VerticalAlignment.Center;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                border.Child = label;
                recentElements_WrapPanel.Children.Add(border);
           }          
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetPage("Setup");
            FO1_Setup.ElementId = 1;
        }
    }
}
