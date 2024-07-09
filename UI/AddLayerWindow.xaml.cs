using BauphysikToolWPF.Services;
using System.Windows;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class AddLayerWindow : Window
    {
        public AddLayerWindow()
        {
            InitializeComponent();
            // Call these Methods only once when Constructor is invoked (Categories stay constant)
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }
    }
}
