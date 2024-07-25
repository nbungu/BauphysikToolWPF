using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.CustomControls;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BauphysikToolWPF.UI
{
    public partial class AddLayerWindow : Window
    {
        private static ToastNotification? _toastNotification;

        public AddLayerWindow()
        {
            InitializeComponent();
            _toastNotification = this.Toast;
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        // Example usage
        public static void ShowToast(string message, ToastType toastType)
        {
            if (_toastNotification is null || _toastNotification.Visibility == Visibility.Visible) return;

            _toastNotification.Message = message;
            _toastNotification.ToastType = toastType;
            _toastNotification.Visibility = Visibility.Visible;

            switch (toastType)
            {
                case ToastType.Info:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/info.png"));
                    break;
                case ToastType.Success:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/success.png"));
                    break;
                case ToastType.Warning:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/warning.png"));
                    break;
                case ToastType.Error:
                    _toastNotification.ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/Flat/error.png"));
                    break;
            }

            // Hide the toast after 3 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                _toastNotification.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }
}
