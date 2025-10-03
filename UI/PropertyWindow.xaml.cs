using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für PropertyWindow.xaml
    /// </summary>
    public partial class PropertyWindow : Window
    {
        public PropertyWindow(IEnumerable<IPropertyItem> propertyItems, string propertyTitle, string windowTitle)
        {
            // Init VM
            InitializeComponent();

            // Explicitly instantiate and initialize ViewModel!
            // This is necessary to set the ctor parameters correctly
            var vm = new PropertyWindow_VM
            {
                PropertyBag = new ObservableCollection<IPropertyItem>(propertyItems),
                PropertyBagTitle = propertyTitle,
                Title = windowTitle
            };

            DataContext = vm;
        }

        public PropertyWindow() => InitializeComponent();

        // Handle Custom User Input - Regex Check
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        /// <summary>
        /// Handles the PreviewMouseWheel event for a ScrollViewer. 
        /// Prevents the parent ScrollViewer from scrolling when a ComboBox dropdown is open.
        /// If the ComboBox's dropdown is open, the mouse wheel event is consumed to allow 
        /// scrolling inside the ComboBox dropdown instead of scrolling the parent container.
        /// </summary>
        /// <param name="sender">The ScrollViewer that triggered the event.</param>
        /// <param name="e">The MouseWheelEventArgs containing event data.</param>
        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled) return;

            DependencyObject current = e.OriginalSource as DependencyObject;

            while (current != null)
            {
                // Check for ComboBox in visual/logical tree
                if (current is FrameworkElement fe && fe.TemplatedParent is ComboBox comboBox)
                {
                    if (comboBox.IsDropDownOpen) return; // ComboBox should handle scrolling
                }

                current = current is Visual or Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
            }

            // Scroll parent ScrollViewer
            if (sender is ScrollViewer scv)
            {
                scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void PropertyWindowControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close(); // Close the window
            }
        }
    }
}
