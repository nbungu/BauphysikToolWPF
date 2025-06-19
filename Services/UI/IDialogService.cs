using System.Windows;

namespace BauphysikToolWPF.Services.UI
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
    }
}
