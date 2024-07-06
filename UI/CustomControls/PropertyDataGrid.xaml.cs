using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für PropertyDataGrid.xaml
    /// </summary>
    public partial class PropertyDataGrid : UserControl
    {
        public PropertyDataGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register(nameof(Properties), typeof(object), typeof(PropertyDataGrid), new PropertyMetadata(null));

        public object Properties
        {
            get => GetValue(PropertiesProperty);
            set => SetValue(PropertiesProperty, value);
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.IsReadOnly) return;

                var propertyItem = textBox.DataContext as IPropertyItem ?? null;
                if (textBox.Text != "" && propertyItem != null)
                {
                    // Change Value in property item, which reflects to the corresponding SelectedLayer Property
                    var type = propertyItem.Value.GetType();

                    if (type == typeof(double)) propertyItem.Value = double.Parse(textBox.Text);
                    else if (type == typeof(int)) propertyItem.Value = int.Parse(textBox.Text);
                    else propertyItem.Value = textBox.Text.ToString(CultureInfo.CurrentCulture);
                }
            }
        }
    }
}
