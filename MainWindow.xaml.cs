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

        // Saves MainWindow Instance here
        public static Window Main;

        public MainWindow()
        {
            InitializeComponent();
            navigationMenuListBox = this.NavigationMenuListBox;
            Main = this;
        }
        
        public static void SetPage(NavigationContent page)
        {
            /*
             * MainWindow.xaml changes the ContentPage based on the 'SelectedItem' string when toggled from 'NavigationListBox'
             * The string values of the SelectedItem are defined at 'NavigationMenuItems'
             * 
             * MainWindow.xaml changes the ContentPage based on the 'Tag' string when NOT toggled from 'NavigationListBox'
             * Set 'SelectedItem' to null before!
             */
            switch (page)
            {
                case NavigationContent.LandingPage:
                    navigationMenuListBox.SelectedIndex = -1;
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
