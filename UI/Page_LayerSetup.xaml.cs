using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class Page_LayerSetup : UserControl
    {
        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call of this Class.
        private Point _origin;
        private Point _start;
        private bool _isDragging;

        // Class Variables - Belongs to the Class-Type itself and stay the same
        private const double MinimumScale = 0.5;
        private const double MaximumScale = 3.0;

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            if (UserSaved.SelectedElement != null)
            {
                UserSaved.SelectedElement.SortLayers();
                UserSaved.SelectedElement.AssignEffectiveLayers();
                UserSaved.SelectedElement.AssignInternalIdsToLayers();
            }

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
        }

        // Save current canvas as image, just before closing Page_LayerSetup Page
        // 'Unloaded' event was called after FO0 Initialize();
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only save if leaving this page
            if (IsVisible) return;
            UserSaved.SelectedElement.Layers.ForEach(l => l.IsSelected = false);
            UserSaved.SelectedElement.Image = (UserSaved.SelectedElement.Layers.Count != 0) ? SaveCanvas.SaveAsBLOB(LayersCanvas, true) : Array.Empty<byte>();
            UserSaved.SelectedElement.FullImage = (UserSaved.SelectedElement.Layers.Count != 0) ? SaveCanvas.SaveGridAsBLOB(ZoomableGrid) : Array.Empty<byte>();
        }

        // Handle Custom User Input - Regex Check
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // If typed input over a selected Text, delete the old value in the TB
                if (tb.SelectedText != "")
                    tb.Text = "";

                //Handle the input
                string userInput = e.Text;
                Regex regex = new Regex("[^0-9,-]+"); //regex that matches disallowed text
                e.Handled = regex.IsMatch(userInput);

                // only allow one decimal point
                if (userInput == "," && tb.Text.IndexOf(',') > -1)
                {
                    e.Handled = true;
                }
                // only allow one minus char
                if (userInput == "-" && tb.Text.IndexOf('-') > -1)
                {
                    e.Handled = true;
                }
            }
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
