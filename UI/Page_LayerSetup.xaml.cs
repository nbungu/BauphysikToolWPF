using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using BauphysikToolWPF.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BauphysikToolWPF.UI
{
    public partial class Page_LayerSetup : UserControl
    {
        #region private Fields

        private OglController _oglController;
        private Element _element; // Selected Element from Session
        private readonly Page_LayerSetup_VM _viewModel; // Selected Element from Session

        #endregion

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            InitalizeLayers();

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            InitalizeOglView();

            // View Model
            _viewModel = new Page_LayerSetup_VM(_oglController);
            this.DataContext = _viewModel;

            // Event Handlers
            this.IsVisibleChanged += UserControl_IsVisibleChanged; // Save current canvas as image, just before closing Page_LayerSetup Page
            this.KeyDown += Page_LayerSetup_KeyDown; // Handle KeyDown events for this page
        }

        private void InitalizeLayers()
        {
            if (Session.SelectedElement is null) return;
            _element = Session.SelectedElement;
            _element.SortLayers();
            _element.AssignEffectiveLayers();
            _element.AssignInternalIdsToLayers();
        }

        private void InitalizeOglView()
        {
            _oglController = new OglController(OpenTkControl, new ElementSceneBuilder(_element, DrawingType.CrossSection));
            _oglController.IsTextSizeZoomable = false;
            _oglController.Redraw(); // Initial render to display the scene


            Session.SelectedElementChanged += _oglController.Redraw;
            Session.SelectedLayerChanged += _oglController.Redraw;
            Session.SelectedLayerIndexChanged += _oglController.Redraw;
        }

        // Save current canvas as image, just before closing Page_LayerSetup Page
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _element.UnselectAllLayers();
            
            if (!IsVisible)
            {
                Session.SelectedElement.RenderOffscreenImage(target: RenderTarget.Screen, withDecorations: false);

                Session.SelectedElementChanged -= _oglController.Redraw;
                Session.SelectedLayerChanged -= _oglController.Redraw;
                Session.SelectedLayerIndexChanged -= _oglController.Redraw;

                _oglController.Dispose();
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
                _viewModel.DeleteLayer();
            }
            else if (e.Key == Key.Enter)
            {
                // Edits the selected Layer
                _viewModel.EditLayer();
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
