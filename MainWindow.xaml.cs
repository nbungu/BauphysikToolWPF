using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using System;
using System.ComponentModel;
using System.IO;
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

    public partial class MainWindow : Window
    {
        public WindowState RestoredWindowState { get; set; }

        private static ContentControl? _mainWindowContent;
        private static ToastNotification? _toastNotification;
        private readonly IDialogService _dialogService;
        private readonly IFileDialogService _fileDialogService;

        public MainWindow()
        {
            InitializeComponent();

            Session.NewProjectAdded += UpdateNewProjectAdded;

            _toastNotification = this.Toast;
            _mainWindowContent = this.MainWindowContent;
            _dialogService = new DialogService();
            _fileDialogService = new FileDialogService();

            SetPage(NavigationPage.ProjectData);

            if (UpdaterManager.NewVersionAvailable)
            {
                Logger.LogInfo($"Found new Version! Notifying User");
                ShowToast($"Neue Version verfügbar: {UpdaterManager.LocalUpdaterManagerFile.LatestTag}. Besuchen Sie bauphysik-tool.de für ein kostenloses Update!", ToastType.Info, 6);
                UpdaterManager.LocalUpdaterManagerFile.LastNotification = TimeStamp.GetCurrentUnixTimestamp();
                UpdaterManager.WriteToLocalUpdaterFile(UpdaterManager.LocalUpdaterManagerFile);
            }
        }

        public static void SetPage(NavigationPage targetPage, NavigationPage? originPage = null)
        {
            if (_mainWindowContent == null) return;
            _mainWindowContent.Content = targetPage;
            Session.OnPageChanged(targetPage, originPage);
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
                    _toastNotification.ToastIcon.Source = Application.Current.Resources["ButtonIcon_Info_B"] as BitmapImage;
                    break;
                case ToastType.Success:
                    _toastNotification.ToastIcon.Source = Application.Current.Resources["ButtonIcon_OK_B"] as BitmapImage;
                    break;
                case ToastType.Warning:
                    _toastNotification.ToastIcon.Source = Application.Current.Resources["ButtonIcon_Warning_B"] as BitmapImage;
                    break;
                case ToastType.Error:
                    _toastNotification.ToastIcon.Source = Application.Current.Resources["ButtonIcon_Error_B"] as BitmapImage;
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
        
        private void UpdateNewProjectAdded()
        {
            _dialogService.ShowLoadingDialog("Lädt, bitte warten...", minDurationMs: 400);

            Session.SelectedElementId = -1;

            // Update InternalIds and render new images
            Session.SelectedProject.AssignAsParentToElements();
            Session.SelectedProject.AssignInternalIdsToElements(true);
            Session.SelectedProject.AssignInternalIdsToEnvelopeItems(true);
            Session.SelectedProject.RenderAllElementImages(withDecorations: false);

            _dialogService.CloseLoadingDialog();
        }

        private void MinimizeCommand(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeCommand(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void CloseCommand(object sender, RoutedEventArgs e) => this.Close();

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
            if (Session.SelectedProject != null && Session.SelectedProject.IsModified)
            {
                MessageBoxResult result = _dialogService.ShowExitSaveConfirmationDialog();

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (!File.Exists(Session.ProjectFilePath))
                        {
                            string saveFileName = Session.SelectedProject.Name == "" ? "unbekannt" : Session.SelectedProject.Name;
                            string? filePath = _fileDialogService.ShowSaveFileDialog($"{saveFileName}.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
                            if (filePath != null)
                            {
                                ProjectSerializer.SaveProjectToFile(Session.SelectedProject, filePath);
                                RecentProjectsManager.AddRecentProject(filePath);
                            }
                            else
                            {
                                e.Cancel = true; // user canceled SaveFileDialog
                                return;
                            }
                        }
                        else
                        {
                            ProjectSerializer.SaveProjectToFile(Session.SelectedProject, Session.ProjectFilePath);
                        }
                        break;
                    case MessageBoxResult.No:
                        // proceed without saving
                        break;
                    case MessageBoxResult.Cancel:
                        // Cancel window close
                        e.Cancel = true;
                        return;
                }
            }
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
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(Interop.HookProc);
        }

        #endregion
    }
}
