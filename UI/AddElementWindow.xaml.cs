using BauphysikToolWPF.Services.UI;
using System.Windows;
using System.Windows.Input;
using BauphysikToolWPF.Services.Application;

namespace BauphysikToolWPF.UI
{
    public partial class AddElementWindow : Window
    {
        public static int TargetElementInternalId;
        public AddElementWindow(int targetElementInternalId = -1)
        {
            TargetElementInternalId = targetElementInternalId;
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
