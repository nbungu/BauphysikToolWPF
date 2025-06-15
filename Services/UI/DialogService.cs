using BauphysikToolWPF.UI;
using BauphysikToolWPF.UI.CustomControls;
using System.Windows;

namespace BauphysikToolWPF.Services.UI
{
    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowSaveConfirmationDialog()
        {
            var dialog = new SaveConfirmationDialog
            {
                Owner = System.Windows.Application.Current.MainWindow,
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
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
            return dialog.Result;
        }
        public void ShowAddNewElementDialog()
        {
            var dialog = new AddElementWindow
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }
        public void ShowEditElementDialog(int targetElementInternalId)
        {
            var dialog = new AddElementWindow(targetElementInternalId)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        public void ShowAddNewLayerDialog()
        {
            var dialog = new AddLayerWindow
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        public void ShowEditLayerDialog(int targetLayerInternalId)
        {
            var dialog = new AddLayerWindow(targetLayerInternalId)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }



        public void ShowAddNewSubconstructionDialog(int targetLayerInternalId)
        {
            var dialog = new AddLayerSubConstructionWindow(targetLayerInternalId)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }

        public void ShowEditSubconstructionDialog(int targetLayerInternalId) => ShowAddNewSubconstructionDialog(targetLayerInternalId);

    }
}
