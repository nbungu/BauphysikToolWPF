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
        LandingPage = 1,
        SetupLayer = 3,
        SetupEnv = 4,
        TemperatureCurve = 6,
        GlaserCurve = 7,
        Default = LandingPage
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
            navigationMenuListBox.SelectedIndex = (int)page; // MainWindow.xaml Binding changes the ContentPage based on the SelectedItem string
        }
    }
}
