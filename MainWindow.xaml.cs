using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

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
        private static ListBox? _navigationMenuListBox;

        private static Border? _projectBoxHeader;

        public MainWindow()
        {
            UserSaved.SelectedProject = DatabaseAccess.QueryProjectById(1);

            InitializeComponent();
            _navigationMenuListBox = this.NavigationMenuListBox;
            _projectBoxHeader = this.ProjectBoxHeader;
        }

        public static void SetPage(NavigationContent page)
        {
            if (_navigationMenuListBox is null || _projectBoxHeader is null) return;
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
                    _navigationMenuListBox.SelectedIndex = -1;
                    _projectBoxHeader.Tag = "ProjectPage";
                    break;
                case NavigationContent.LandingPage:
                    _navigationMenuListBox.SelectedIndex = -1;
                    _projectBoxHeader.Tag = "LandingPage";
                    break;
                case NavigationContent.SetupLayer:
                    _navigationMenuListBox.SelectedItem = "SetupLayer";
                    break;
                case NavigationContent.SetupEnv:
                    _navigationMenuListBox.SelectedItem = "SetupEnv";
                    break;
                case NavigationContent.TemperatureCurve:
                    _navigationMenuListBox.SelectedItem = "Temperature";
                    break;
                case NavigationContent.GlaserCurve:
                    _navigationMenuListBox.SelectedItem = "Moisture";
                    break;
                case NavigationContent.DynamicHeatCalc:
                    _navigationMenuListBox.SelectedItem = "Dynamic";
                    break;
                default:
                    _navigationMenuListBox.SelectedItem = "LandingPage";
                    break;
            }
        }

        private void DockPanelOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void MinimizeCommand(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeCommand(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void CloseCommand(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region ResizeWindows

        bool ResizeInProcess = false;
        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            if (senderRect != null)
            {
                ResizeInProcess = true;
                senderRect.CaptureMouse();
            }
        }

        private void Resize_End(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            if (senderRect != null)
            {
                ResizeInProcess = false; ;
                senderRect.ReleaseMouseCapture();
            }
        }

        private void Resizing_Form(object sender, MouseEventArgs e)
        {
            if (ResizeInProcess)
            {
                Rectangle senderRect = sender as Rectangle;
                Window mainWindow = senderRect.Tag as Window;
                if (senderRect != null)
                {
                    double width = e.GetPosition(mainWindow).X;
                    double height = e.GetPosition(mainWindow).Y;
                    senderRect.CaptureMouse();
                    if (senderRect.Name.ToLower().Contains("right"))
                    {
                        width += 5;
                        if (width > 0)
                            mainWindow.Width = width;
                    }
                    if (senderRect.Name.ToLower().Contains("left"))
                    {
                        width -= 5;
                        mainWindow.Left += width;
                        width = mainWindow.Width - width;
                        if (width > 0)
                        {
                            mainWindow.Width = width;
                        }
                    }
                    if (senderRect.Name.ToLower().Contains("bottom"))
                    {
                        height += 5;
                        if (height > 0)
                            mainWindow.Height = height;
                    }
                    if (senderRect.Name.ToLower().Contains("top"))
                    {
                        height -= 5;
                        mainWindow.Top += height;
                        height = mainWindow.Height - height;
                        if (height > 0)
                        {
                            mainWindow.Height = height;
                        }
                    }
                }
            }
        }
        #endregion

    }
}
