using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BT.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Hook Loaded event: by then the control is fully constructed and in the visual tree
            this.Loaded += Page_Elements_Loaded;

            Logger.LogInfo("Success");
        }

        private void InitializeElements()
        {
            if (Session.SelectedProject is null)
            {
                MainWindow.SetPage(NavigationPage.ProjectData, NavigationPage.ElementCatalogue);
                return;
            }
            Session.SelectedProject.Init();
        }

        /// <summary>
        /// Handles the UserControl's <see cref="FrameworkElement.Loaded"/> event.
        /// Ensures that when the page is displayed, keyboard focus is set to the first
        /// element's Button inside the <see cref="ElementsControl"/>, enabling immediate
        /// keyboard navigation (arrow keys, Enter, Delete, etc.).
        /// </summary>
        private void Page_Elements_Loaded(object sender, RoutedEventArgs e)
        {
            ElementsControl.Focus();

            if (ElementsControl.Items.Count == 0) return;

            // get the first container (ContentPresenter for the first item)
            var container = ElementsControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            if (container is null) return;

            // access the Button defined in the DataTemplate by its x:Name
            var button = container.ContentTemplate.FindName("ElementButton", container) as Button;
            button?.Focus();
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
