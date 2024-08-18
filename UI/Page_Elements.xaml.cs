using System;
using System.Linq;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Elements : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Constructor
        public Page_Elements()
        {
            RenderNewImages();
            
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }

        private void RenderNewImages()
        {
            var elementsWithoutImage = UserSaved.SelectedProject.Elements.Where(e => e.Image == Array.Empty<byte>());
            foreach (var element in elementsWithoutImage)
            {
                // Create a CanvasDrawingService for the selected element
                var canvasSize = new BT.Geometry.Rectangle(new BT.Geometry.Point(0, 0), 880, 400);
                var drawingService = new CanvasDrawingService(element, canvasSize);

                // Capture images using the GeometryRenderer
                var imageBytes = CaptureImage.CaptureOffscreenVisualAsImage(drawingService);

                // Update the selected element with the captured images
                element.Image = imageBytes;
                element.FullImage = imageBytes; // Or another image if needed
            }
        }
    }
}
