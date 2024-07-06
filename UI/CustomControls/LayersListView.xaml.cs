using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für LayersListView.xaml
    /// </summary>
    public partial class LayersListView : UserControl
    {
        public LayersListView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LayersProperty =
            DependencyProperty.Register(nameof(Layers), typeof(object), typeof(LayersListView), new PropertyMetadata(null));

        public object Layers
        {
            get => GetValue(LayersProperty);
            set => SetValue(LayersProperty, value);
        }

        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register(nameof(SelectedLayer), typeof(object), typeof(LayersListView), new PropertyMetadata(null));

        public object SelectedLayer
        {
            get => GetValue(SelectedLayerProperty);
            set => SetValue(SelectedLayerProperty, value);
        }
    }
}
