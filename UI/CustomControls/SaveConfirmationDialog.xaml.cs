using System.Windows;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für SaveConfirmationDialog.xaml
    /// </summary>
    public partial class SaveConfirmationDialog : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public SaveConfirmationDialog()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            DialogResult = true;
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            DialogResult = false;
        }
    }
}
