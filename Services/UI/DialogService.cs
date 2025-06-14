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

            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
