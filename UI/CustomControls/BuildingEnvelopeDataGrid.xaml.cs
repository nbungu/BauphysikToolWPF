using BauphysikToolWPF.Services.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für BuildingEnvelopeDataGrid.xaml
    /// </summary>
    public partial class BuildingEnvelopeDataGrid : UserControl
    {
        public BuildingEnvelopeDataGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty EnvelopeItemsProperty =
            DependencyProperty.Register(nameof(EnvelopeItems), typeof(object), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object EnvelopeItems
        {
            get => GetValue(EnvelopeItemsProperty);
            set => SetValue(EnvelopeItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedEnvelopeItemProperty =
            DependencyProperty.Register(nameof(SelectedEnvelopeItem), typeof(object), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object SelectedEnvelopeItem
        {
            get => GetValue(SelectedEnvelopeItemProperty);
            set => SetValue(SelectedEnvelopeItemProperty, value);
        }

        //public static readonly DependencyProperty ElementCollectionProperty =
        //    DependencyProperty.Register(nameof(ElementCollection), typeof(object), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        //public object ElementCollection
        //{
        //    get => GetValue(ElementCollectionProperty);
        //    set => SetValue(ElementCollectionProperty, value);
        //}

        private void DataGridCell_Selected(object sender, RoutedEventArgs e)
        {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);
            }
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void textData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.Any.IsMatch(e.Text);
        }
    }
}
