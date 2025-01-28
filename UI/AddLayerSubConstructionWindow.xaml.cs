using BauphysikToolWPF.Services;
using System.Windows;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class AddLayerSubConstructionWindow : Window
    {
        public static bool EditExistingSubConstr;

        public AddLayerSubConstructionWindow(bool editExisting = true)
        {
            EditExistingSubConstr = editExisting;

            InitializeComponent();
            // Call these Methods only once when Constructor is invoked (Categories stay constant)
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void AddLayerSubConstructionWindowControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close(); // Close the window
            }
        }
    }
}
