using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für LayersCanvas.xaml
    /// </summary>
    public partial class LayersCanvas : UserControl
    {
        public LayersCanvas()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DrawingGeometriesProperty =
            DependencyProperty.Register(nameof(DrawingGeometries), typeof(object), typeof(LayersCanvas), new PropertyMetadata(null));

        public object DrawingGeometries
        {
            get => GetValue(DrawingGeometriesProperty);
            set => SetValue(DrawingGeometriesProperty, value);
        }
    }
}
