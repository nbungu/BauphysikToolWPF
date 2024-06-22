using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class AddLayerSubConstructionWindow : Window
    {
        public AddLayerSubConstructionWindow()
        {
            InitializeComponent();
            // Call these Methods only once when Constructor is invoked (Categories stay constant)
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
