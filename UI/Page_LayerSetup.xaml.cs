using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using BauphysikToolWPF.UI.ViewModels;
using BT.Logging;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BauphysikToolWPF.UI.CustomControls;
using System;
using System.Linq;

namespace BauphysikToolWPF.UI
{
    // TODO:
    // No "logic" in UI (xaml)
    // Page_LayerSetup.xaml.cs = Controller / Logic
    // Page_LayerSetup_VM = ViewMODEL + UI refresh
    // Page_LayerSetup.xaml = UI

    public partial class Page_LayerSetup : UserControl
    {
        #region private Fields

        private readonly Page_LayerSetup_VM _vm;

        #endregion

        public OglController OglController { get; private set; }

        public Page_LayerSetup()
        {
            InitalizeLayers();

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            InitalizeOglView();

            // View Model
            _vm = new Page_LayerSetup_VM(this);
            DataContext = _vm;
            
            // Event Handlers
            IsVisibleChanged += UserControl_IsVisibleChanged; // Save current canvas as image, just before closing Page_LayerSetup Page
            KeyDown += Page_LayerSetup_KeyDown; // Handle KeyDown events for this page
        }

        private void InitalizeLayers()
        {
            if (Session.SelectedElement is null)
            {
                MainWindow.SetPage(NavigationPage.ElementCatalogue, NavigationPage.LayerSetup);
                return;
            }
            var element = Session.SelectedElement;
            element.SortLayers();
            element.AssignEffectiveLayers();
            element.AssignInternalIdsToLayers();
        }

        private void InitalizeOglView()
        {
            Logger.LogInfo("[OGL] Starting OglController for LayerSetup-Page");

            OglController = new OglController(OpenTkControl, new ElementSceneBuilder(Session.SelectedElement, DrawingType.CrossSection));
            OglController.IsTextSizeZoomable = false;
            OglController.Redraw(); // Initial render to display the scene

            OglController.ShapeClicked += OnShapeClicked;
            OglController.ShapeDoubleClicked += OnShapeDoubleClicked;
            OglController.ShapeRightClicked += OnShapeRightClicked;

            Session.SelectedElementChanged += OglController.Redraw;
            Session.SelectedLayerChanged += OglController.Redraw;
            Session.SelectedLayerIndexChanged += OglController.Redraw;

            Logger.LogInfo("[OGL] Successfully started OglController for LayerSetup-Page");
        }

        private void LayerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Find the visual element that was actually clicked
            var originalSource = e.OriginalSource as DependencyObject;

            // Try to find the parent ListViewItem of that clicked element
            var item = FindAncestor<ListViewItem>(originalSource);

            // If no ListViewItem was clicked (i.e., empty area), do nothing
            if (item == null)
                return;

            var clickedItem = (Layer)item.DataContext;

            // Select the item
            ((LayersListView)sender).SelectedLayer = clickedItem;

            _vm.LayerDoubleClick();
        }

        private void OnShapeClicked(ShapeId shape)
        {
            var targetLayer = _vm.LayerList.FirstOrDefault(l => l?.InternalId == shape.Index, null);
            if (targetLayer != null)
            {
                var index = _vm.LayerList.IndexOf(targetLayer);
                _vm.SelectedLayerIndex = index;
            }
            Console.WriteLine($"Shape clicked: {shape}");
        }

        private List<ContextMenuItemDefinition> GetLayerContextMenuItems()
        {
            return new List<ContextMenuItemDefinition>
            {
                new() { Header = "Bearbeiten", IconSource = "pack://application:,,,/Resources/Icons/edit-2.png", Action = _vm.EditLayer },
                new() { Header = "Schicht", IconSource = "pack://application:,,,/Resources/Icons/plus.png", Action = _vm.AddLayerBelow },
                new() { Header = "Duplizieren", IconSource = "pack://application:,,,/Resources/Icons/copy.png", Action = _vm.DuplicateLayer },
                new() { Header = "Löschen", IconSource = "pack://application:,,,/Resources/Icons/delete-2.png", Action = _vm.DeleteLayer },
                new() { IsSeparator = true },
                new() { Header = "Balkenlage hinzufügen", IconSource = "pack://application:,,,/Resources/Icons/plus_B.png", Action = () => _vm.AddSubConstructionLayer(_vm.SelectedLayer?.InternalId ?? -1), IsVisible = !_vm.SelectedLayer?.HasSubConstruction},
                new() { Header = "Balkenlage bearbeiten", IconSource = "pack://application:,,,/Resources/Icons/edit-2.png", Action = () => _vm.EditSubConstructionLayer(_vm.SelectedLayer?.InternalId ?? -1), IsVisible = _vm.SelectedLayer?.HasSubConstruction },
                new() { Header = "Balkenlage löschen", IconSource = "pack://application:,,,/Resources/Icons/delete_B.png", Action = () => _vm.DeleteSubConstructionLayer(_vm.SelectedLayer?.InternalId ?? -1), IsVisible = _vm.SelectedLayer?.HasSubConstruction },
            };
        }

        private void OnShapeDoubleClicked(ShapeId shape)
        {
            var layerShapeTarget = _vm.LayerList.FirstOrDefault(l => l?.InternalId == shape.Index, null);

            if (layerShapeTarget != null)
            {
                var index = _vm.LayerList.IndexOf(layerShapeTarget);
                _vm.SelectedLayerIndex = index;

                if (shape.Type == ShapeType.SubConstructionLayer) _vm.EditSubConstructionLayer(_vm.SelectedLayer?.InternalId ?? -1);
                else if (shape.Type == ShapeType.DimensionalChain) _vm.LayerDoubleClick(); // TODO: Put focus on thickness TextBox;
                else _vm.LayerDoubleClick();
            }
            Console.WriteLine($"Shape double clicked: {shape}");
        }
        private void OnShapeRightClicked(ShapeId shapeId, BT.Geometry.Point mousePos)
        {
            // First: Select OnShapeClicked to set SelectedLayer
            OnShapeClicked(shapeId); // triggers SelectedLayerIndexChanged which is hooked to a Redraw event

            // Second: provide context Menu
            DynamicContextMenuService.ShowContextMenu(OglController.View, mousePos, GetLayerContextMenuItems());
        }

        private void LayerListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Find the visual element that was actually clicked
            var originalSource = e.OriginalSource as DependencyObject;

            // Try to find the parent ListViewItem of that clicked element
            var item = FindAncestor<ListViewItem>(originalSource);

            // If no ListViewItem was clicked: unselect
            if (item == null)
                ((LayersListView)sender).SelectedLayerIndex = -1;
        }

        private void LayerListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Find the visual element that was actually clicked
            var originalSource = e.OriginalSource as DependencyObject;

            // Try to find the parent ListViewItem of that clicked element
            var item = FindAncestor<ListViewItem>(originalSource);

            // If no ListViewItem was clicked (i.e., empty area), do nothing
            if (item == null)
                return;

            // Items gets automatically selected when right-clicked on Button down
            //var clickedItem = (Layer)item.DataContext;
            //((LayersListView)sender).SelectedLayer = clickedItem;

            // Get mouse position relative to the ListView
            var pos = e.GetPosition((UIElement)sender);
            var mousePos = new BT.Geometry.Point(pos.X, pos.Y);

            DynamicContextMenuService.ShowContextMenu(this, mousePos, GetLayerContextMenuItems());
        }

        private static T? FindAncestor<T>(DependencyObject? current)
            where T : DependencyObject
        {
            while (current != null && current is not T)
                current = VisualTreeHelper.GetParent(current);

            return current as T;
        }

        // Save current canvas as image, just before closing Page_LayerSetup
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Session.SelectedElement.UnselectAllLayers();
            
            if (!IsVisible)
            {
                Session.SelectedElement.RenderOffscreenImage(target: RenderTarget.Screen, withDecorations: false);

                Session.SelectedElementChanged -= OglController.Redraw;
                Session.SelectedLayerChanged -= OglController.Redraw;
                Session.SelectedLayerIndexChanged -= OglController.Redraw;

                OglController.Dispose();
            }
        }

        // Handle Custom User Input - Regex Check
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }
        private void numericData_PreviewTextInput_Positive(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulturePositive.IsMatch(e.Text);
        }

        private void Page_LayerSetup_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Escape key to close the dropdown of ComboBox
            if (e.Key == Key.Delete)
            {
                // Removes selected Layer
                _vm.DeleteLayer();
            }
            else if (e.Key == Key.Enter)
            {
                // Edits the selected Layer
                _vm.EditLayer();
            }
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
            if (e.Handled)
                return;

            DependencyObject current = e.OriginalSource as DependencyObject;

            while (current != null)
            {
                // Check for ComboBox in visual/logical tree
                if (current is FrameworkElement fe && fe.TemplatedParent is ComboBox comboBox)
                {
                    if (comboBox.IsDropDownOpen)
                        return; // ComboBox should handle scrolling
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

        private void LayerListScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

    }
}
