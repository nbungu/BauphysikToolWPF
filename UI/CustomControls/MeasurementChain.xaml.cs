using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für MeasurementChain.xaml
    /// </summary>
    public partial class MeasurementChain : UserControl
    {
        public MeasurementChain()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DrawingGeometryProperty =
            DependencyProperty.Register(nameof(DrawingGeometry), typeof(object), typeof(MeasurementChain), new PropertyMetadata(null));

        public object DrawingGeometry
        {
            get => GetValue(DrawingGeometryProperty);
            set => SetValue(DrawingGeometryProperty, value);
        }

    }
}
