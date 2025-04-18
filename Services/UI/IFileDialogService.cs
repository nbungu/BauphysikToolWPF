namespace BauphysikToolWPF.Services.UI
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string defaultFileName, string filter);
        string? ShowOpenFileDialog(string filter);
    }
}
