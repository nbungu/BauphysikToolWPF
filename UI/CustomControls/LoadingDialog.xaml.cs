using System.Windows;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog : Window
    {
        public LoadingDialog()
        {
            InitializeComponent();
        }

        public void UpdateMessage(string message)
        {
            // Dispatcher.Invoke not needed if called on UI thread already
            LoadingMessageTextBlock.Text = message;
        }
    }
}
