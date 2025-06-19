using BauphysikToolWPF.Models.Domain;
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
            DependencyProperty.Register(nameof(Layers), typeof(object), typeof(LayersListView), new FrameworkPropertyMetadata(null));

        public object Layers
        {
            get => GetValue(LayersProperty);
            set => SetValue(LayersProperty, value);
        }

        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register(nameof(SelectedLayer), typeof(Layer), typeof(LayersListView), new FrameworkPropertyMetadata(new Layer(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedLayer
        {
            get => GetValue(SelectedLayerProperty);
            set => SetValue(SelectedLayerProperty, value);
        }

        public static readonly DependencyProperty SelectedLayerIndexProperty =
            DependencyProperty.Register(nameof(SelectedLayerIndex), typeof(int), typeof(LayersListView), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedLayerIndex
        {
            get => GetValue(SelectedLayerIndexProperty);
            set => SetValue(SelectedLayerIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedLayerInternalIdProperty =
            DependencyProperty.Register(nameof(SelectedLayerInternalId), typeof(int), typeof(LayersListView), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedLayerInternalId
        {
            get => GetValue(SelectedLayerInternalIdProperty);
            set => SetValue(SelectedLayerInternalIdProperty, value);
        }
    }
}
