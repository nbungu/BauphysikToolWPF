using BauphysikToolWPF.UI.CustomControls;

namespace BauphysikToolWPF.Services.Application
{
    public class ToastService : IToastService
    {
        public void ShowInfoToast(string msg)
        {
            MainWindow.ShowToast(msg, ToastType.Info);
        }

        public void ShowWarningToast(string msg)
        {
            MainWindow.ShowToast(msg, ToastType.Warning);
        }

        public void ShowErrorToast(string msg)
        {
            MainWindow.ShowToast(msg, ToastType.Error);
        }

        public void ShowSuccessToast(string msg)
        {
            MainWindow.ShowToast(msg, ToastType.Success);
        }
    }
}
