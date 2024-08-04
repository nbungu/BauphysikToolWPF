using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        LayerSetup,
        Summary,
        TemperatureCurve,
        GlaserCurve,
        DynamicHeatCalc
    }
    public partial class MainWindow : Window
    {
        public WindowState RestoredWindowState { get; set; }

        private static ListBox? _navigationMenuListBox;
        private static Border? _projectBoxHeader;
        private static ToastNotification? _toastNotification;

        public MainWindow()
        {
            InitializeComponent();
            _navigationMenuListBox = this.NavigationMenuListBox;
            _projectBoxHeader = this.ProjectBoxHeader;
            _toastNotification = this.Toast;

            if (Updater.CompareSemanticVersions(Updater.LocalUpdaterFile.Current, Updater.LocalUpdaterFile.Latest) < 0)
            {
                Logger.LogInfo($"Found new Version! Notifying User");
                ShowToast($"New Version Available: {Updater.LocalUpdaterFile.LatestTag}", ToastType.Info);
                Updater.LocalUpdaterFile.LastNotification = TimeStamp.GetCurrentUnixTimestamp();
                Updater.WriteToLocalUpdaterFile(Updater.LocalUpdaterFile);
            }
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
                case NavigationContent.LayerSetup:
                    _navigationMenuListBox.SelectedItem = "LayerSetup";
                    break;
                case NavigationContent.Summary:
                    _navigationMenuListBox.SelectedItem = "Summary";
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

        public static void ShowToast(string message, ToastType toastType)
        {
            if (_toastNotification is null || _toastNotification.Visibility == Visibility.Visible) return;

            _toastNotification.Message = message;
            _toastNotification.ToastType = toastType;
            _toastNotification.Visibility = Visibility.Visible;

            switch (toastType)
            {
                case ToastType.Info:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/info.png"));
                    break;
                case ToastType.Success:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/success.png"));
                    break;
                case ToastType.Warning:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/warning.png"));
                    break;
                case ToastType.Error:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/error.png"));
                    break;
            }

            // Hide the toast after 3 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                _toastNotification.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
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

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.Normal)
            {
                this.RestoredWindowState = this.WindowState;
            }

            this.RefreshMaximizeRestoreButton();
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.Normal)
            {
                this.RestoredWindowState = this.WindowState;
            }

            this.RefreshMaximizeRestoreButton();
        }
        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            // TODO: 
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
                RestoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                MaximizeButton.Visibility = Visibility.Visible;
                RestoreButton.Visibility = Visibility.Collapsed;
            }
        }

        #region Fix for Maximized placement (7px cut off)

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        #endregion

    }
}
