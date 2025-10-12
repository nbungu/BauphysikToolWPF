using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.UI;
using BauphysikToolWPF.UI.CustomControls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using static System.Windows.Application;

namespace BauphysikToolWPF.Services.Application
{
    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowSaveConfirmationDialog()
        {
            var dialog = new SaveConfirmationDialog
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
            return dialog.Result;
        }

        public MessageBoxResult ShowExitSaveConfirmationDialog()
        {
            var dialog = new SaveExitConfirmationDialog
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
            return dialog.Result;
        }

        public MessageBoxResult ShowDeleteConfirmationDialog()
        {
            var dialog = new DeleteConfirmationDialog()
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
            return dialog.Result;
        }
        public void ShowAddNewElementDialog(int atIndex = -1)
        {
            var dialog = new AddElementWindow
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }
        public void ShowEditElementDialog(int targetElementInternalId)
        {
            var dialog = new AddElementWindow(targetElementInternalId)
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        public void ShowAddNewLayerDialog(int atIndex = -1)
        {
            var dialog = new AddLayerWindow
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            // TODO: Rework!
            AddLayerWindow.AddAtIndex = atIndex;
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        public void ShowEditLayerDialog(int targetLayerInternalId)
        {
            var dialog = new AddLayerWindow(targetLayerInternalId)
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }
        
        public void ShowAddNewSubconstructionDialog(int targetLayerInternalId)
        {
            var dialog = new AddLayerSubConstructionWindow(targetLayerInternalId)
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        public void ShowEditSubconstructionDialog(int targetLayerInternalId) => ShowAddNewSubconstructionDialog(targetLayerInternalId);

        public void ShowPropertyBagDialog(IEnumerable<IPropertyItem> propertyItems, string propertyTitle, string windowTitle)
        {
            var dialog = new PropertyWindow(propertyItems, propertyTitle, windowTitle)
            {
                Owner = Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        private LoadingDialog? _loadingDialog;
        private Stopwatch? _stopwatch;
        private int _minDurationMs;

        public void ShowLoadingDialog(string message = "Loading, please wait...", int minDurationMs = 400)
        {
            if (_loadingDialog != null) return;

            _minDurationMs = minDurationMs;
            _stopwatch = Stopwatch.StartNew();

            Current.Dispatcher.Invoke(() =>
            {
                _loadingDialog = new LoadingDialog
                {
                    Owner = Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Set initial message text (assumes you added a public method/property in LoadingDialog)
                _loadingDialog.UpdateMessage(message);

                _loadingDialog.Show();
            });
        }

        public void CloseLoadingDialog()
        {
            if (_loadingDialog == null || _stopwatch == null)
                return;

            int elapsedMs = (int)_stopwatch.ElapsedMilliseconds;
            int remainingMs = _minDurationMs - elapsedMs;

            if (remainingMs > 0)
            {
                // Sleep off the UI thread to avoid blocking UI - do it synchronously if you want a simpler solution
                Thread.Sleep(remainingMs);
            }

            Current.Dispatcher.Invoke(() =>
            {
                _loadingDialog?.Close();
                _loadingDialog = null;
            });

            _stopwatch = null;
        }
    }
}
