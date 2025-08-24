using System.Collections.Generic;
using System.Windows;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.UI;
using BauphysikToolWPF.UI.CustomControls;

namespace BauphysikToolWPF.Services.Application
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

        public MessageBoxResult ShowExitSaveConfirmationDialog()
        {
            var dialog = new SaveExitConfirmationDialog
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

        public void ShowPropertyBagDialog(IEnumerable<IPropertyItem> propertyItems, string propertyTitle, string windowTitle)
        {
            var dialog = new PropertyWindow(propertyItems, propertyTitle, windowTitle)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            dialog.ShowDialog();
        }
    }
}
