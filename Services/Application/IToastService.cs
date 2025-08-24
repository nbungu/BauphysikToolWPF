namespace BauphysikToolWPF.Services.Application
{
    public interface IToastService
    {
        void ShowInfoToast(string msg);
        void ShowWarningToast(string msg);
        void ShowErrorToast(string msg);
    }
}
