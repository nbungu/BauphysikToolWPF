using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF
{
    /*
     * THIS IS THE MAIN WINDOW WHICH CONTAINS ALL PAGES AND CONTENT
     * 
     * Contains the Navigation Box on the left and the Content Pages on the right side
     */

    // Top-level type. Defined outside of class. Part of namespace BauphysikToolWPF. Accessible from whole application
    public enum NavigationContent 
    {
        // see in MainWindow.xaml the List of ItemsSource for indices of the ListBoxItems (Pages)
        ProjectPage,
        LandingPage,
        SetupLayer,
        SetupEnv,
        TemperatureCurve,
        GlaserCurve,
        DynamicHeatCalc
    }
    public partial class MainWindow : Window
    {
        public static ListBox? navigationMenuListBox;

        public static Border? projectBoxHeader;

        public MainWindow()
        {
            InitializeComponent();
            navigationMenuListBox = this.NavigationMenuListBox;
            projectBoxHeader = this.ProjectBoxHeader;
        }
        
        public static void SetPage(NavigationContent page)
        {
            if (navigationMenuListBox is null || projectBoxHeader is null)
                return;
            /*
             * MainWindow.xaml changes the ContentPage based on the 'SelectedItem' string when toggled from 'NavigationListBox'
             * The string values of the SelectedItem are defined at 'NavigationMenuItems'
             * 
             * Alternatively: MainWindow.xaml changes the ContentPage based on the 'Tag' string when NOT toggled from 'NavigationListBox'
             * Set 'SelectedItem' or 'SelectedIndex' to null / -1 before!
             */

            switch (page)
            {
                case NavigationContent.ProjectPage:
                    navigationMenuListBox.SelectedIndex = -1;
                    projectBoxHeader.Tag = "ProjectPage";
                    break;
                case NavigationContent.LandingPage:
                    navigationMenuListBox.SelectedIndex = -1;
                    projectBoxHeader.Tag = "LandingPage";
                    break;
                    //
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
                case NavigationContent.DynamicHeatCalc:
                    navigationMenuListBox.SelectedItem = "Dynamic";
                    break;
                default:
                    navigationMenuListBox.SelectedItem = "LandingPage";
                    break;
            }
        }
    }
}
