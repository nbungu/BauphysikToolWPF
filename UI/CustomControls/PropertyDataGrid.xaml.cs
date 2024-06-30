using System.Windows;
using System.Windows.Controls;

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
    }
}
