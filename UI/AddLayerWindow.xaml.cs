using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BauphysikToolWPF.UI
{
    public partial class AddLayerWindow : Window
    {
        private static ToastNotification? _toastNotification;
        
        public static int TargetLayerInternalId { get; private set; }
        public static int AddAtIndex { get; set; }

        // New Layer
        public AddLayerWindow()
        {
            TargetLayerInternalId = -1;
            AddAtIndex = -1;

            InitializeComponent();
            _toastNotification = this.Toast;
        }

        // Edit Layer
        public AddLayerWindow(int targetLayerInternalId)
        {
            TargetLayerInternalId = targetLayerInternalId;
            AddAtIndex = -1;

            InitializeComponent();
            _toastNotification = this.Toast;
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        // TODO: unify with MainWindow Toast
        public static void ShowToast(string message, ToastType toastType)
        {
            if (_toastNotification is null || _toastNotification.Visibility == Visibility.Visible) return;

            _toastNotification.Message = message;
            _toastNotification.ToastType = toastType;
            _toastNotification.Visibility = Visibility.Visible;

            // Hide the toast after 3 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                _toastNotification.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        private void AddLayerWindowControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close(); // Close the window
            }
        }
    }
}
