using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BauphysikToolWPF.UI
{
    public partial class Page_LayerSetup : UserControl
    {
        #region private Fields

        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call of this Class.
        private Point _origin;
        private Point _start;
        private bool _isDragging;
        private GLWindow? _glWnd;
        // Flag to track whether GLWindow has been initialized
        private bool _glWindowInitialized = false;

        // Class Variables - Belongs to the Class-Type itself and stay the same
        private const double MinimumScale = 0.5;
        private const double MaximumScale = 3.0;

        #endregion

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
            this.Loaded += Page_LayerSetup_Loaded;
            this.Unloaded += Page_LayerSetup_Unloaded;
        }

        #region Creating and Closing the Child Window for OpenGL rendering
        
        private void Page_LayerSetup_Loaded(object sender, RoutedEventArgs e)
        {
            if (_glWindowInitialized)
                return;

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                IntPtr hWndParent = new WindowInteropHelper(parentWindow).Handle;

                // Create and run GLWindow only once
                _glWnd = GLWindow.CreateAndRun(hWndParent, GetRenderAreaBounds());
                _glWindowInitialized = true;

                // Cleanup on main window close
                parentWindow.Closing += ParentWindow_Closing;
            }
        }

        private void ParentWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            CleanupGLWindow();
        }

        //If you dynamically add/remove this UserControl, you may want to unhook the event when unloading
        private void Page_LayerSetup_Unloaded(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Closing -= ParentWindow_Closing;
            }

            CleanupGLWindow(); // Optional: eager cleanup if switching away
        }
        private void CleanupGLWindow()
        {
            if (_glWnd != null)
            {
                _glWnd.Cleanup();
                _glWnd = null;
                _glWindowInitialized = false;
            }
        }

        #endregion

        #region Resizing the Main Window and the Child Window Along With It

        private void RenderArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _glWnd?.SetBoundingBox(GetRenderAreaBounds());
        }

        private Rect GetRenderAreaBounds()
        {
            Point location = RenderArea.TransformToAncestor(this).Transform(new Point(0, 0));
            return new Rect
            {
                X = location.X,
                Y = location.Y,
                Width = RenderArea.ActualWidth,
                Height = RenderArea.ActualHeight
            };
        }

        #endregion

        // Save current canvas as image, just before closing Page_LayerSetup Page
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Session.SelectedElement is null) return;
            var element = Session.SelectedElement;

            // Only save if leaving this page
            element.UnselectAllLayers();

            if (!IsVisible && element.IsValid)
            {
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
