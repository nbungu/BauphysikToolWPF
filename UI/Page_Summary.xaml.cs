using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.UI.OpenGL;
using BauphysikToolWPF.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.Application;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Summary : UserControl
    {
        // Class Variables - Belongs to the Class-Type itself and stay the same
        //

        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call.
        private readonly OglController _oglController;
        private readonly Element _element; // Selected Element from Session

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_Summary()
        {
            if (Session.SelectedElement is null) return;

            _element = Session.SelectedElement;

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            _oglController = new OglController(OpenTkControlVertical, new ElementSceneBuilder(_element, DrawingType.VerticalCut));
            _oglController.Redraw(); // Initial render to display the scene
            _oglController.IsSceneInteractive = false; // Disable interaction with the scene
            _oglController.IsTextSizeZoomable = true; // Disable editing of the scene

            // View Model
            this.DataContext = new Page_Summary_VM();
            this.IsVisibleChanged += Page_Summary_IsVisibleChanged;
        }

        private void Page_Summary_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) _oglController.Dispose();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
