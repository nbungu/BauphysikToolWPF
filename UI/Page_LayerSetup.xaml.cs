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
        //private GLControl _glControl;
        //private CrossSectionOpenGLRenderer _glRenderer = new();

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
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                IntPtr hWndParent = new WindowInteropHelper(parentWindow).Handle;
                _glWnd = GLWindow.CreateAndRun(hWndParent, GetRenderAreaBounds());

                parentWindow.Closing += ParentWindow_Closing;
            }
        }

        private void ParentWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _glWnd?.Cleanup();
        }

        //If you dynamically add/remove this UserControl, you may want to unhook the event when unloading
        private void Page_LayerSetup_Unloaded(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Closing -= ParentWindow_Closing;
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
                element.DocumentImage = ImageCreator.CaptureUIElementAsImage(ZoomableGrid, includeMargins: true);
                
                ImageCreator.RenderElementPreviewImage(element);
                //element.Image = ImageCreator.CaptureUIElementAsImage(LayersCanvas, includeMargins: true);
            }
        }

        // Handle Custom User Input - Regex Check
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
        }

        private void ZoomableGrid_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(ZoomableGrid);

            double zoomFactor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;

            // Calculate the new scale
            double newScaleX = GridScaleTransform.ScaleX * zoomFactor;
            double newScaleY = GridScaleTransform.ScaleY * zoomFactor;

            // Ensure the new scale does not go below the minimum scale factor or above the maximum scale factor
            if (newScaleX < MinimumScale || newScaleX > MaximumScale || newScaleY < MinimumScale || newScaleY > MaximumScale)
            {
                return;
            }

            // Calculate the translation to keep the mouse position centered
            double offsetX = mousePosition.X * (newScaleX - GridScaleTransform.ScaleX);
            double offsetY = mousePosition.Y * (newScaleY - GridScaleTransform.ScaleY);

            GridScaleTransform.ScaleX = newScaleX;
            GridScaleTransform.ScaleY = newScaleY;

            GridTranslateTransform.X -= offsetX;
            GridTranslateTransform.Y -= offsetY;
        }

        private void ZoomableGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zoom In on double Click
            if (e.ClickCount == 2)
            {
                Point mousePosition = e.GetPosition(ZoomableGrid);

                double zoomFactor = 1.5;

                // Calculate the new scale
                double newScaleX = GridScaleTransform.ScaleX * zoomFactor;
                double newScaleY = GridScaleTransform.ScaleY * zoomFactor;

                // Ensure the new scale does not go below the minimum scale factor or above the maximum scale factor
                if (newScaleX < MinimumScale || newScaleX > MaximumScale || newScaleY < MinimumScale || newScaleY > MaximumScale)
                {
                    return;
                }

                // Calculate the translation to keep the mouse position centered
                double offsetX = mousePosition.X * (newScaleX - GridScaleTransform.ScaleX);
                double offsetY = mousePosition.Y * (newScaleY - GridScaleTransform.ScaleY);

                GridScaleTransform.ScaleX = newScaleX;
                GridScaleTransform.ScaleY = newScaleY;

                GridTranslateTransform.X -= offsetX;
                GridTranslateTransform.Y -= offsetY;
                return;
            }

            // Only allow dragging if the grid is zoomed in (scale greater than 1)
            if (GridScaleTransform.ScaleX > 1.0 && GridScaleTransform.ScaleY > 1.0)
            {
                _start = e.GetPosition(ZoomableGrid);
                _origin = new Point(GridTranslateTransform.X, GridTranslateTransform.Y);
                ZoomableGrid.CaptureMouse();
                ZoomableGrid.Cursor = Cursors.Hand;
                _isDragging = true;
            }
        }

        private void ZoomableGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point p = e.GetPosition(ZoomableGrid);
                //if (p.X < 0 || p.X > ZoomableGrid.ActualWidth || p.Y < 0 || p.Y > ZoomableGrid.ActualHeight)
                //{
                //    // Stop dragging if the mouse is outside the bounds of the ZoomableGrid
                //    CustomReleaseMouseCapture();
                //    return;
                //}

                Vector v = p - _start; // Correct the direction of the movement

                // Adjust the translation based on the scale factor
                GridTranslateTransform.X = _origin.X + v.X;// / GridScaleTransform.ScaleX;
                GridTranslateTransform.Y = _origin.Y + v.Y;// / GridScaleTransform.ScaleY;
            }
        }

        private void ZoomableGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CustomReleaseMouseCapture();
        }

        private void CustomReleaseMouseCapture()
        {
            if (ZoomableGrid.IsMouseCaptured)
            {
                ZoomableGrid.ReleaseMouseCapture();
                ZoomableGrid.Cursor = Cursors.Arrow;
                _isDragging = false;
            }
        }

        private void ZoomableGrid_OnTouchDown(object sender, TouchEventArgs e)
        {
            // Only allow dragging if the grid is zoomed in (scale greater than 1)
            if (GridScaleTransform.ScaleX > 1.0 && GridScaleTransform.ScaleY > 1.0)
            {
                _start = e.GetTouchPoint(ZoomableGrid).Position;
                _origin = new Point(GridTranslateTransform.X, GridTranslateTransform.Y);
                ZoomableGrid.CaptureTouch(e.TouchDevice);
                _isDragging = true;
            }
        }

        private void ZoomableGrid_OnTouchMove(object sender, TouchEventArgs e)
        {
            if (_isDragging)
            {
                Point p = e.GetTouchPoint(ZoomableGrid).Position;
                //if (p.X < 0 || p.X > ZoomableGrid.ActualWidth || p.Y < 0 || p.Y > ZoomableGrid.ActualHeight)
                //{
                //    // Stop dragging if the touch is outside the bounds of the ZoomableGrid
                //    CustomReleaseTouchCapture(e.TouchDevice);
                //    return;
                //}

                Vector v = p - _start; // Correct the direction of the movement

                // Adjust the translation based on the scale factor
                GridTranslateTransform.X = _origin.X + v.X;// / GridScaleTransform.ScaleX;
                GridTranslateTransform.Y = _origin.Y + v.Y;// / GridScaleTransform.ScaleY;
            }
        }

        private void ZoomableGrid_OnTouchUp(object sender, TouchEventArgs e)
        {
            CustomReleaseTouchCapture(e.TouchDevice);
        }

        private void CustomReleaseTouchCapture(TouchDevice touchDevice)
        {
            if (ZoomableGrid.AreAnyTouchesCaptured)
            {
                ZoomableGrid.ReleaseTouchCapture(touchDevice);
                _isDragging = false;
            }
        }

        private void ZoomableGrid_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Reset transformations on right click
            GridScaleTransform.ScaleX = 1.0;
            GridScaleTransform.ScaleY = 1.0;
            GridTranslateTransform.X = 0.0;
            GridTranslateTransform.Y = 0.0;
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
