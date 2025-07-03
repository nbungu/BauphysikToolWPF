using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using OpenTK.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BauphysikToolWPF.UI
{
    // TRY: https://github.com/opentk/GLWpfControl

    public partial class Page_LayerSetup : UserControl
    {
        #region private Fields

        private readonly ElementScene? _elementScene = new ElementScene(); // Instance of the OpenGL Test Scene, if needed

        #endregion

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    
            this.Unloaded += Page_LayerSetup_Unloaded; // Unload event handler for this page

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3,
                RenderContinuously = false,
            };
            OpenTkControl.Start(settings);
            _elementScene.UseElement(Session.SelectedElement);
            _elementScene.Initialize();
        }

        private void Page_LayerSetup_Unloaded(object sender, RoutedEventArgs e)
        { 
            _elementScene?.Dispose();
            OpenTkControl.Render -= OpenTkControl_OnRender;
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            _elementScene.UpdateProjection(GetRenderAreaBounds());
            _elementScene.Render();
        }

        #region Resizing the Main Window and the Child Window Along With It

        private void OpenTkControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _elementScene.UpdateProjection(GetRenderAreaBounds());
        }

        private Rect GetRenderAreaBounds()
        {
            Point location = OpenTkControl.TransformToAncestor(this).Transform(new Point(0, 0));
            return new Rect
            {
                X = location.X,
                Y = location.Y,
                Width = OpenTkControl.ActualWidth,
                Height = OpenTkControl.ActualHeight
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
