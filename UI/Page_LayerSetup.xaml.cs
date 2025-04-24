using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class Page_LayerSetup : UserControl
    {
        #region private Fields

        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call of this Class.
        private Point _origin;
        private Point _start;
        private bool _isDragging;

        // Class Variables - Belongs to the Class-Type itself and stay the same
        private const double MinimumScale = 0.5;
        private const double MaximumScale = 3.0;

        #endregion

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
        }

        // Save current canvas as image, just before closing Page_LayerSetup Page
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Session.SelectedElement is null) return;

            // Only save if leaving this page
            Session.SelectedElement.UnselectAllLayers();

            if (!IsVisible && Session.SelectedElement.IsValid)
            {
                Session.SelectedElement.DocumentImage = ImageCreator.CaptureUIElementAsImage(ZoomableGrid, includeMargins: true);
                
                ImageCreator.RenderElementPreviewImage(Session.SelectedElement);
                //Session.SelectedElement.Image = ImageCreator.CaptureUIElementAsImage(LayersCanvas, includeMargins: true);
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

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
