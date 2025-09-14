using System.Collections.Generic;
using System.Windows;
using BauphysikToolWPF.Models.UI;

namespace BauphysikToolWPF.Services.Application
{
    public interface IDialogService
    {
        MessageBoxResult ShowSaveConfirmationDialog();
        MessageBoxResult ShowExitSaveConfirmationDialog();
        MessageBoxResult ShowDeleteConfirmationDialog();
        void ShowAddNewElementDialog();
        void ShowEditElementDialog(int targetElementInternalId);
        void ShowAddNewLayerDialog();
        void ShowEditLayerDialog(int targetLayerInternalId);
        void ShowAddNewSubconstructionDialog(int targetLayerInternalId);
        void ShowEditSubconstructionDialog(int targetLayerInternalId);
        void ShowPropertyBagDialog(IEnumerable<IPropertyItem> propertyItems, string propertyTitle, string windowTitle);
        void ShowLoadingDialog(string message, int minDurationMs);
        void CloseLoadingDialog();
    }
}
