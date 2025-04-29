using System.Collections.Generic;
using BauphysikToolWPF.Services.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BauphysikToolWPF.Models.Domain;

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
            DependencyProperty.Register(nameof(EnvelopeItems), typeof(IEnumerable<EnvelopeItem>), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object EnvelopeItems
        {
            get => GetValue(EnvelopeItemsProperty);
            set => SetValue(EnvelopeItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedEnvelopeItemProperty =
            DependencyProperty.Register(nameof(SelectedEnvelopeItem), typeof(EnvelopeItem), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object SelectedEnvelopeItem
        {
            get => GetValue(SelectedEnvelopeItemProperty);
            set => SetValue(SelectedEnvelopeItemProperty, value);
        }

        public static readonly DependencyProperty IsAllSelectedProperty =
            DependencyProperty.Register(nameof(IsAllSelected), typeof(bool), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object IsAllSelected
        {
            get => GetValue(IsAllSelectedProperty);
            set => SetValue(IsAllSelectedProperty, value);
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void textData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.Any.IsMatch(e.Text);
        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = (DependencyObject)e.OriginalSource;

            while (depObj != null && !(depObj is DataGridCell))
                depObj = VisualTreeHelper.GetParent(depObj);

            if (depObj is DataGridCell cell)
            {
                var dataGrid = (DataGrid)sender;
                if (!cell.IsEditing)
                {
                    if (!cell.IsFocused)
                    {
                        cell.Focus();
                    }

                    var row = DataGridRow.GetRowContainingElement(cell);
                    if (row != null)
                    {
                        dataGrid.SelectedItem = row.Item;
                    }
                    dataGrid.BeginEdit();
                    e.Handled = true;
                }
            }
            //if (depObj is DataGridCell cell)
            //{
            //    var dataGrid = (DataGrid)sender;
            //    var row = DataGridRow.GetRowContainingElement(cell);

            //    if (row != null)
            //    {
            //        // Force select item manually
            //        dataGrid.SelectedItem = row.Item;

            //        // Also push to the custom control's property
            //        SelectedEnvelopeItem = row.Item;
            //    }

            //    // Begin editing
            //    if (!cell.IsEditing)
            //    {
            //        cell.Focus();
            //        dataGrid.BeginEdit();
            //        e.Handled = true;
            //    }
            //}
        }
    }
}
