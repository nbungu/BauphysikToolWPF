using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BauphysikToolWPF.Models.UI;

namespace BauphysikToolWPF
{
    /*
     * THIS IS THE MAIN WINDOW WHICH CONTAINS ALL PAGES AND CONTENT
     * 
     * Contains the Navigation Box on the left and the Content Pages on the right side
     */

    // Top-level type. Defined outside of class. Part of namespace BauphysikToolWPF. Accessible from whole application
    public enum NavigationPage
    {
        // see in MainWindow.xaml the List of ItemsSource for indices of the ListBoxItems (Pages)
        ProjectPage = 0,
        ElementCatalogue = 10,
        LayerSetup = 11,
        Summary = 12,
        TemperatureCurve = 13,
        GlaserCurve = 14,
        DynamicHeatCalc = 15,
        BuildingEnvelope = 20,
        EnvelopeSummary = 21,
    }
    public partial class MainWindow : Window
    {
        public WindowState RestoredWindowState { get; set; }

        private static ContentControl? _mainWindowContent;
        private static ToastNotification? _toastNotification;

        public MainWindow()
        {
            InitializeComponent();

            _toastNotification = this.Toast;
            _mainWindowContent = this.MainWindowContent;

            SetPage(NavigationPage.ProjectPage);

            if (UpdaterManager.NewVersionAvailable)
            {
                Logger.LogInfo($"Found new Version! Notifying User");
                ShowToast($"New Version Available: {UpdaterManager.LocalUpdaterManagerFile.LatestTag}. Besuchen Sie bauphysik-tool.de für ein kostenloses Update!", ToastType.Info, 6);
                UpdaterManager.LocalUpdaterManagerFile.LastNotification = TimeStamp.GetCurrentUnixTimestamp();
                UpdaterManager.WriteToLocalUpdaterFile(UpdaterManager.LocalUpdaterManagerFile);
            }
        }

        public static void SetPage(NavigationPage page)
        {
            _mainWindowContent.Content = page;
        }

        public static void ShowToast(string message, ToastType toastType, int durationInSeconds = 3)
        {
            if (_toastNotification is null || _toastNotification.Visibility == Visibility.Visible) return;

            _toastNotification.Message = message;
            _toastNotification.ToastType = toastType;
            _toastNotification.Visibility = Visibility.Visible;

            switch (toastType)
            {
                case ToastType.Info:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/info_b.png"));
                    break;
                case ToastType.Success:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/haken.png"));
                    break;
                case ToastType.Warning:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/triangle-warning.png"));
                    break;
                case ToastType.Error:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/error.png"));
                    break;
            }

            // Hide the toast after 3 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(durationInSeconds) };
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
                MINMAXINFO mmi = (MINMAXINFO)(Marshal.PtrToStructure(lParam, typeof(MINMAXINFO)) ?? new MINMAXINFO());

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
