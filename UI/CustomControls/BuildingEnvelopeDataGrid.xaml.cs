using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        public static readonly DependencyProperty ElementCollectionProperty =
            DependencyProperty.Register(nameof(ElementCollection), typeof(object), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object ElementCollection
        {
            get => GetValue(ElementCollectionProperty);
            set => SetValue(ElementCollectionProperty, value);
        }

        private void BuildingEnvelopeDataGridElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe)
            {
                //DataGridCell cell = fe.Parent as DataGridCell;
                DataGridCell cell = FindParent<DataGridCell>(fe);
                if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
                {
                    // TODO:
                    // Special handling for ComboBox to open dropdown immediately
                    if (cell.Content is ComboBox comboBox)
                    {
                        comboBox.Focus();
                        comboBox.IsDropDownOpen = true;
                    }

                    // Set focus to the cell
                    cell.Focus();

                    // Force DataGrid into edit mode
                    DataGrid dataGrid = FindParent<DataGrid>(cell);
                    if (dataGrid != null)
                    {
       
                        dataGrid.BeginEdit();
                        e.Handled = true; // Prevent default selection behavior


                    }


                }
            }
        }
        // Helper function to find the parent DataGrid
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typedParent)
                    return typedParent;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
