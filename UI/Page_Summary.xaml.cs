using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Summary : UserControl
    {
        // Class Variables - Belongs to the Class-Type itself and stay the same


        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call.
        private Point _origin;
        private Point _start;

        private const double MinimumScale = 0.5;
        private const double MaximumScale = 3.0;

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_Summary()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
        }

        #region Cross Section Cut

        private void ZoomableGrid1_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(ZoomableGrid1);

            double zoomFactor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;

            // Calculate the new scale
            double newScaleX = Grid1ScaleTransform.ScaleX * zoomFactor;
            double newScaleY = Grid1ScaleTransform.ScaleY * zoomFactor;

            // Ensure the new scale does not go below the minimum scale factor or above the maximum scale factor
            if (newScaleX < MinimumScale || newScaleX > MaximumScale || newScaleY < MinimumScale || newScaleY > MaximumScale)
            {
                return;
            }

            // Calculate the translation to keep the mouse position centered
            double offsetX = mousePosition.X * (newScaleX - Grid1ScaleTransform.ScaleX);
            double offsetY = mousePosition.Y * (newScaleY - Grid1ScaleTransform.ScaleY);

            Grid1ScaleTransform.ScaleX = newScaleX;
            Grid1ScaleTransform.ScaleY = newScaleY;

            Grid1TranslateTransform.X -= offsetX;
            Grid1TranslateTransform.Y -= offsetY;
        }

        private void ZoomableGrid1_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ZoomableGrid1.IsMouseCaptured)
            {
                Point p = e.GetPosition(ZoomableGrid1);
                Vector v = p - _start; // Correct the direction of the movement

                // Adjust the translation based on the scale factor
                Grid1TranslateTransform.X = _origin.X + v.X;
                Grid1TranslateTransform.Y = _origin.Y + v.Y;
            }
        }

        private void ZoomableGrid1_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Reset transformations on right click
            Grid1ScaleTransform.ScaleX = 1.0;
            Grid1ScaleTransform.ScaleY = 1.0;
            Grid1TranslateTransform.X = 0.0;
            Grid1TranslateTransform.Y = 0.0;
        }

        private void ZoomableGrid1_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zoom In on double Click
            if (e.ClickCount == 2)
            {
                Point mousePosition = e.GetPosition(ZoomableGrid1);

                double zoomFactor = 1.5;

                // Calculate the new scale
                double newScaleX = Grid1ScaleTransform.ScaleX * zoomFactor;
                double newScaleY = Grid1ScaleTransform.ScaleY * zoomFactor;

                // Ensure the new scale does not go below the minimum scale factor or above the maximum scale factor
                if (newScaleX < MinimumScale || newScaleX > MaximumScale || newScaleY < MinimumScale || newScaleY > MaximumScale)
                {
                    return;
                }

                // Calculate the translation to keep the mouse position centered
                double offsetX = mousePosition.X * (newScaleX - Grid1ScaleTransform.ScaleX);
                double offsetY = mousePosition.Y * (newScaleY - Grid1ScaleTransform.ScaleY);

                Grid1ScaleTransform.ScaleX = newScaleX;
                Grid1ScaleTransform.ScaleY = newScaleY;

                Grid1TranslateTransform.X -= offsetX;
                Grid1TranslateTransform.Y -= offsetY;
                return;
            }

            // Only allow dragging if the grid is zoomed in
            if (Grid1ScaleTransform.ScaleX > 1.0 || Grid1ScaleTransform.ScaleY > 1.0)
            {
                _start = e.GetPosition(ZoomableGrid1);
                _origin = new Point(Grid1TranslateTransform.X, Grid1TranslateTransform.Y);
                ZoomableGrid1.CaptureMouse();
                ZoomableGrid1.Cursor = Cursors.Hand;
            }
        }

        private void ZoomableGrid1_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ZoomableGrid1.IsMouseCaptured)
            {
                ZoomableGrid1.ReleaseMouseCapture();
                ZoomableGrid1.Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Vertical Cut

        private void ZoomableGrid2_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(ZoomableGrid2);

            double zoomFactor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;

            // Calculate the new scale
            double newScaleX = Grid2ScaleTransform.ScaleX * zoomFactor;
            double newScaleY = Grid2ScaleTransform.ScaleY * zoomFactor;

            // Ensure the new scale does not go below the minimum scale factor or above the maximum scale factor
            if (newScaleX < MinimumScale || newScaleX > MaximumScale || newScaleY < MinimumScale || newScaleY > MaximumScale)
            {
                return;
            }

            // Calculate the translation to keep the mouse position centered
            double offsetX = mousePosition.X * (newScaleX - Grid2ScaleTransform.ScaleX);
            double offsetY = mousePosition.Y * (newScaleY - Grid2ScaleTransform.ScaleY);

            Grid2ScaleTransform.ScaleX = newScaleX;
            Grid2ScaleTransform.ScaleY = newScaleY;

            Grid2TranslateTransform.X -= offsetX;
            Grid2TranslateTransform.Y -= offsetY;
        }

        private void ZoomableGrid2_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ZoomableGrid2.IsMouseCaptured)
            {
                Point p = e.GetPosition(ZoomableGrid2);
                Vector v = p - _start; // Correct the direction of the movement

                // Adjust the translation based on the scale factor
                Grid2TranslateTransform.X = _origin.X + v.X;
                Grid2TranslateTransform.Y = _origin.Y + v.Y;
            }
        }

        private void ZoomableGrid2_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Reset transformations on right click
            Grid2ScaleTransform.ScaleX = 1.0;
            Grid2ScaleTransform.ScaleY = 1.0;
            Grid2TranslateTransform.X = 0.0;
            Grid2TranslateTransform.Y = 0.0;
        }

        private void ZoomableGrid2_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zoom In on double Click
            if (e.ClickCount == 2)
            {
                Point mousePosition = e.GetPosition(ZoomableGrid2);

                double zoomFactor = 1.5;

                // Calculate the new scale
                double newScaleX = Grid2ScaleTransform.ScaleX * zoomFactor;
                double newScaleY = Grid2ScaleTransform.ScaleY * zoomFactor;

                // Ensure the new scale does not go below the minimum scale factor or above the maximum scale factor
                if (newScaleX < MinimumScale || newScaleX > MaximumScale || newScaleY < MinimumScale || newScaleY > MaximumScale)
                {
                    return;
                }

                // Calculate the translation to keep the mouse position centered
                double offsetX = mousePosition.X * (newScaleX - Grid2ScaleTransform.ScaleX);
                double offsetY = mousePosition.Y * (newScaleY - Grid2ScaleTransform.ScaleY);

                Grid2ScaleTransform.ScaleX = newScaleX;
                Grid2ScaleTransform.ScaleY = newScaleY;

                Grid2TranslateTransform.X -= offsetX;
                Grid2TranslateTransform.Y -= offsetY;
                return;
            }

            // Only allow dragging if the grid is zoomed in
            if (Grid2ScaleTransform.ScaleX > 1.0 || Grid2ScaleTransform.ScaleY > 1.0)
            {
                _start = e.GetPosition(ZoomableGrid2);
                _origin = new Point(Grid2TranslateTransform.X, Grid2TranslateTransform.Y);
                ZoomableGrid2.CaptureMouse();
                ZoomableGrid2.Cursor = Cursors.Hand;
            }
        }

        private void ZoomableGrid2_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ZoomableGrid2.IsMouseCaptured)
            {
                ZoomableGrid2.ReleaseMouseCapture();
                ZoomableGrid2.Cursor = Cursors.Arrow;
            }
        }

        #endregion
    }
}
