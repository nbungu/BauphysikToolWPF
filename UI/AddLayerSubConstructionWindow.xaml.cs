using BauphysikToolWPF.Services.UI;
using System.Windows;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class AddLayerSubConstructionWindow : Window
    {
        public static bool EditExistingSubConstr;
        public static int TargetLayerInternalId;

        public AddLayerSubConstructionWindow(int targetLayerInternalId = -1, bool editExisting = true)
        {
            EditExistingSubConstr = editExisting;
            TargetLayerInternalId = targetLayerInternalId;

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
