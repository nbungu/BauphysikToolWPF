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

        private readonly OglController _oglController;
        private readonly Element _element; // Selected Element from Session

        #endregion

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            if (Session.SelectedElement is null) return;

            _element = Session.SelectedElement;
            _element.SortLayers();
            _element.AssignEffectiveLayers();
            _element.AssignInternalIdsToLayers();

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            _oglController = new OglController(new ElementSceneBuilder(_element, DrawingType.CrossSection));
            _oglController.ConnectToView(OpenTkControl);  // hook into GL control

            // View Model
            this.DataContext = new Page_LayerSetup_VM(_oglController);
            this.IsVisibleChanged += UserControl_IsVisibleChanged; // Save current canvas as image, just before closing Page_LayerSetup Page
        }

        // Save current canvas as image, just before closing Page_LayerSetup Page
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only save if leaving this page
            _element.UnselectAllLayers();

            if (!IsVisible && _element.IsValid)
            {
                _oglController.Dispose();
                // TODO: 
                //element.DocumentImage = ImageCreator.CaptureUIElementAsImage(ZoomableGrid, includeMargins: true);

                //ImageCreator.RenderElementPreviewImage(element);
                //element.Image = ImageCreator.CaptureUIElementAsImage(LayersCanvas, includeMargins: true);
            }
        }

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
