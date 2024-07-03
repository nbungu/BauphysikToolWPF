using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
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

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var propertyItem = (IPropertyItem)textBox.DataContext;
                if (textBox.Text != "")
                {
                    // Change Value in property item, which reflects to the corresponding SelectedLayer Property
                    propertyItem.Value = double.Parse(textBox.Text);
                    // Update XAML
                    UserSaved.OnSelectedLayerChanged();
                }
            }
        }
    }
}
