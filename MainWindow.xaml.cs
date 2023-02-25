using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ListBox navigationMenuListBox;

        // Saves MainWindow Instance here
        public static Window Main;

        public MainWindow()
        {
            InitializeComponent();
            navigationMenuListBox = this.NavigationMenuListBox;
            Main = this;
        }
        
        // The Pages a user can inteact with
        public enum NavigationContent
        {
            LandingPage,
            SetupLayer,
            SetupEnv,
            TemperatureCurve,
            GlaserCurve,
            Default = LandingPage
        }
        public static void SetPage(string page)
        {
            
            navigationMenuListBox.SelectedItem = page; // MainWindow.xaml Binding changes the ContentPage based on the SelectedItem string
        }
    }
}
