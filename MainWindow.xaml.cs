using BauphysikToolWPF.UI;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    // Top-level type. Defined outside of class. Part of namespace BauphysikToolWPF
    // accessible from whole application
    public enum NavigationContent 
    {
        // see in MainWindow.xaml the List of ItemsSource for indices of the ListBoxItems (Pages)
        LandingPage,
        SetupLayer,
        SetupEnv,
        TemperatureCurve,
        GlaserCurve
    }
    public partial class MainWindow : Window
    {
        public static ListBox navigationMenuListBox;
        public static ContentControl mainWindow_Content;

        // Saves MainWindow Instance here
        public static Window Main;

        public MainWindow()
        {
            InitializeComponent();
            navigationMenuListBox = this.NavigationMenuListBox;
            mainWindow_Content = this.MainWindow_Content;
            Main = this;
        }
        
        public static void SetPage(NavigationContent page)
        {
            // MainWindow.xaml changes the ContentPage based on the SelectedItem string
            // The string values of the SelectedItem are defined at 'NavigationMenuItems'
            switch (page)
            {
                case NavigationContent.LandingPage:
                    mainWindow_Content.Tag = "LandingPage"; // TODO
                    break;
                case NavigationContent.SetupLayer:
                    navigationMenuListBox.SelectedItem = "SetupLayer";
                    break;
                case NavigationContent.SetupEnv:
                    navigationMenuListBox.SelectedItem = "SetupEnv";
                    break;
                case NavigationContent.TemperatureCurve:
                    navigationMenuListBox.SelectedItem = "Temperature";
                    break;
                case NavigationContent.GlaserCurve:
                    navigationMenuListBox.SelectedItem = "Moisture";
                    break;
                default:
                    navigationMenuListBox.SelectedItem = "LandingPage";
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetPage(NavigationContent.LandingPage);
        }
    }
}
