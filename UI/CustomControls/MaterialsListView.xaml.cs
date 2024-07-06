using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für MaterialsListView.xaml
    /// </summary>
    public partial class MaterialsListView : UserControl
    {
        public MaterialsListView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MaterialsProperty =
            DependencyProperty.Register(nameof(Materials), typeof(object), typeof(MaterialsListView), new PropertyMetadata(null));

        public object Materials
        {
            get => GetValue(MaterialsProperty);
            set => SetValue(MaterialsProperty, value);
        }

        public static readonly DependencyProperty SelectedMaterialProperty =
            DependencyProperty.Register(nameof(SelectedMaterial), typeof(object), typeof(MaterialsListView), new PropertyMetadata(null));

        public object SelectedMaterial
        {
            get => GetValue(SelectedMaterialProperty);
            set => SetValue(SelectedMaterialProperty, value);
        }
    }
}
