using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using BauphysikToolWPF.Services.Application;

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

        public static readonly DependencyProperty IsNonResidentialProperty =
            DependencyProperty.Register(nameof(IsNonResidential), typeof(bool), typeof(BuildingEnvelopeDataGrid), new PropertyMetadata(null));

        public object IsNonResidential
        {
            get => GetValue(IsNonResidentialProperty);
            set => SetValue(IsNonResidentialProperty, value);
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void textData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.Any.IsMatch(e.Text);
        }

        /// <summary>
        /// Handles the PreviewMouseWheel event for a ScrollViewer. 
        /// Prevents the parent ScrollViewer from scrolling when a ComboBox dropdown is open.
        /// If the ComboBox's dropdown is open, the mouse wheel event is consumed to allow 
        /// scrolling inside the ComboBox dropdown instead of scrolling the parent container.
        /// </summary>
        /// <param name="sender">The ScrollViewer that triggered the event.</param>
        /// <param name="e">The MouseWheelEventArgs containing event data.</param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled)
                return;

            DependencyObject current = e.OriginalSource as DependencyObject;

            while (current != null)
            {
                // Find the ComboBox that owns the popup (logical or visual tree walk)
                if (current is FrameworkElement fe && fe.TemplatedParent is ComboBox comboBox)
                {
                    if (comboBox.IsDropDownOpen)
                        return; // Let the ComboBox handle the scroll
                }

                current = VisualTreeHelper.GetParent(current);
            }

            // Scroll parent ScrollViewer
            if (sender is ScrollViewer scv)
            {
                scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = (DependencyObject)e.OriginalSource;

            // Traverse the visual tree to find the DataGridCell
            while (depObj != null && !(depObj is DataGridCell))
                depObj = VisualTreeHelper.GetParent(depObj);

            if (depObj is DataGridCell cell)
            {
                var dataGrid = (DataGrid)sender;

                if (!cell.IsEditing)
                {
                    // Focus the cell
                    if (!cell.IsFocused)
                    {
                        cell.Focus();
                    }

                    // Get the row that contains the cell
                    var row = DataGridRow.GetRowContainingElement(cell);
                    if (row != null)
                    {
                        // Set the row item as selected
                        dataGrid.SelectedItem = row.Item;
                    }

                    // Begin editing immediately
                    dataGrid.BeginEdit();

                    // Wait for the cell to finish the edit mode transition before accessing the editor
                    dataGrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        // Get the editor (ComboBox) once the cell is in edit mode
                        var editor = cell.Content as ComboBox;
                        if (editor != null && editor.IsEnabled)
                        {
                            // Open the dropdown of the ComboBox
                            editor.IsDropDownOpen = true;
                        }
                    }));

                    e.Handled = true;  // Prevent the event from bubbling further
                }
            }
        }

    }
}
