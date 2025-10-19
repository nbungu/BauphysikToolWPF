using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BauphysikToolWPF.Services.UI;

namespace BauphysikToolWPF.UI
{
    // TODO:
    // No "logic" in UI (xaml)
    // Page_Elements.xaml.cs = Controller / Logic
    // Page_Elements_VM = ViewMODEL + UI refresh
    // Page_Elements.xaml = UI
    public partial class Page_Elements : UserControl  // publisher of 'ElementSelectionChanged' event
    {

        private readonly Page_Elements_VM _vm;

        // Constructor
        public Page_Elements()
        {
            InitializeElements();
            
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)

            // View Model
            _vm = new Page_Elements_VM();
            DataContext = _vm;

            // Hook Loaded event: by then the control is fully constructed and in the visual tree
            Loaded += Page_Elements_Loaded;
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
            if (_vm.SelectedElement == null) _vm.SelectElement(_vm.Elements.FirstOrDefault()?.InternalId ?? -1);
            SetButtonFocus();
        }

        private void SetButtonFocus()
        {
            ElementsControl.Focus();

            if (ElementsControl.Items.Count == 0) return;

            // get the first container (ContentPresenter for the first item)
            var container = ElementsControl.ItemContainerGenerator.ContainerFromItem(_vm.SelectedElement) as ContentPresenter;
            if (container is null) return;

            // access the Button defined in the DataTemplate by its x:Name
            var button = container.ContentTemplate.FindName("ElementButton", container) as Button;
            if (button is null) return;
            button.Focus();
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

        private void ElementButton_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Select the item
            int internalId = _vm.SelectedElement?.InternalId ?? -1;
            if (internalId == -1) return;

            // Get mouse position relative to the ListView
            var pos = e.GetPosition((Button)sender);
            var mousePos = new BT.Geometry.Point(pos.X, pos.Y);

            var items = new List<ContextMenuItemDefinition>
            {
                new() { Header = "Eigenschaften", IconSource = Icons.ButtonIcon_Edit_B, Action = () => _vm.EditElement(internalId) },
                new() { Header = "Kopie erstellen", IconSource = Icons.ButtonIcon_Copy_B, Action = () => _vm.CopyElement(internalId) },
                new() { Header = "Als PDF exportieren", IconSource = Icons.ButtonIcon_Export_B, Action = () => _vm.CreateSingleElementPdf(internalId) },
                new() { IsSeparator = true },
                new() { Header = "Löschen", IconSource = Icons.ButtonIcon_Delete_B, Action = () => _vm.DeleteElement(internalId) },
            };
            DynamicContextMenuService.ShowContextMenu((Button)sender, mousePos, items);
        }

        private void ElementButton_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Select the item
            int internalId = (int)((Button)sender).CommandParameter;

            // If no Element was clicked (i.e., empty area), do nothing
            if (internalId == null) return;

            // Item needs to be selected manually because right-click does not select the item by default
            _vm.SelectElement(internalId);
        }

        private void ElementButton_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _vm.OpenElement();
        }

        private void ElementButton_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            
            switch (e.Key)
            {
                case Key.Return:
                    _vm.OpenElement();
                    e.Handled = true;
                    break;
                case Key.Delete:
                    // Select the item
                    int internalId = _vm.SelectedElement?.InternalId ?? -1;
                    if (internalId == -1) return;
                    _vm.DeleteElement(internalId);
                    e.Handled = true;
                    break;
                case Key.Left:
                    _vm.PreviousElement();
                    e.Handled = true;
                    break;
                case Key.Right:
                    _vm.NextElement();
                    e.Handled = true;
                    break;
            }
        }
    }
}
