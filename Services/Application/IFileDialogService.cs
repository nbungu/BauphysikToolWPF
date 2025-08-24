namespace BauphysikToolWPF.Services.Application
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string defaultFileName, string filter);
        string? ShowOpenFileDialog(string filter);
    }
}
