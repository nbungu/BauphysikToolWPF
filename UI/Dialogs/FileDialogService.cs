using Microsoft.Win32;

namespace BauphysikToolWPF.UI.Dialogs
{
    public class FileDialogService : IFileDialogService
    {
        public string? ShowSaveFileDialog(string defaultFileName, string filter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = filter
            };

            bool? result = saveFileDialog.ShowDialog();
            return result == true ? saveFileDialog.FileName : null;
        }
        public string? ShowOpenFileDialog(string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter
            };

            bool? result = openFileDialog.ShowDialog();
            return result == true ? openFileDialog.FileName : null;
        }
    }
}
