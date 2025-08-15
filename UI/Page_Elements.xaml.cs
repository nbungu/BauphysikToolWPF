using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Elements : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Constructor
        public Page_Elements()
        {
            InitializeElements();
            
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }

        private void InitializeElements()
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.AssignInternalIdsToElements();
            Session.SelectedProject.AssignAsParentToElements();
            Session.SelectedProject.SortElements(ElementSortingType.DateDescending);
            Session.SelectedProject.RenderMissingElementImages(withDecorations: false);
        }

        /// <summary>
        /// mouse wheel to be handled by a parent scrollable container instead of the inner ScrollViewer of the PropertyDataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ElementPropertiesDataGrid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Pass the event to the parent instead of the ScrollViewer
            e.Handled = true;

            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };

            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
