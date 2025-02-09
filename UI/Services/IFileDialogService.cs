namespace BauphysikToolWPF.UI.Services
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string defaultFileName, string filter);
        string? ShowOpenFileDialog(string filter);
    }
}
