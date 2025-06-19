namespace BauphysikToolWPF.Services.UI
{
    public interface IToastService
    {
        void ShowInfoToast(string msg);
        void ShowWarningToast(string msg);
        void ShowErrorToast(string msg);
    }
}
