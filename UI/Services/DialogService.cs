using System.Windows;

namespace BauphysikToolWPF.UI.Services
{
    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowSaveConfirmationDialog()
        {
            string message = "Do you want to save the current project before creating a new one?";
            string caption = "Save Project";
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            return MessageBox.Show(message, caption, buttons, icon);
        }
    }
}
