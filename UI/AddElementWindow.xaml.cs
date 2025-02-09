using BauphysikToolWPF.Services;
using System.Windows;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class AddElementWindow : Window
    {
        public static bool EditExistingElement;
        public AddElementWindow(bool editExsiting = false)
        {
            EditExistingElement = editExsiting;
            InitializeComponent();
        }
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void AddElementWindowControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close(); // Close the window
            }
        }
    }
}
