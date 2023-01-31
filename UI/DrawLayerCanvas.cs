using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
{
    public class DrawLayerCanvas
    {
        // custom parameter constructor, with optional parameter "showPositionLabel"
        public DrawLayerCanvas(List<Layer> layers, Canvas canvas, bool showPositionLabel = true)
        {
            canvas.Children.Clear();

            if (layers == null || layers.Count == 0)
                return;

            // check if canvas was already created in frontend
            canvas = canvas ?? throw new ArgumentNullException(nameof(canvas) + " is not initialized or not found");

            double right = canvas.Width;
            double elementWidth = 0;
            foreach (Layer layer in layers)
            {
                elementWidth += layer.LayerThickness;
            }
            // Drawing from right to left: first layer in list is inside (= right side)
            foreach (Layer layer in layers)
            {
                double layerWidthScale = layer.LayerThickness / elementWidth; // from  0 ... 1
                double layerWidth = canvas.Width * layerWidthScale;
                double left = right - layerWidth; // start drawing from right canvas side (beginning with INSIDE Layer, which is first list element) -> We want Inside layer position on right/inner side. 

                // Draw layer rectangle
                Rectangle baseRect = new Rectangle()
                {
                    Width = layerWidth,
                    Height = canvas.Height,
                    Stroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black,
                    StrokeThickness = layer.IsSelected ? 2 : 0.2,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode)),
                };
                canvas.Children.Add(baseRect);
                Canvas.SetTop(baseRect, 0);
                Canvas.SetLeft(baseRect, left);

                // Draw hatch pattern rectangle
                Rectangle hatchPatternRect = new Rectangle()
                {
                    Width = layerWidth - 1,
                    Height = canvas.Height,
                    Fill = HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, layerWidth, canvas.Height),
                    Opacity = 0.6
                };
                canvas.Children.Add(hatchPatternRect);
                Canvas.SetTop(hatchPatternRect, 0);
                Canvas.SetLeft(hatchPatternRect, left + 0.5);

                if (showPositionLabel == true)
                {
                    // Add Label with layer position
                    Label label = new Label()
                    {
                        Content = layer.LayerPosition,
                        FontSize = 14
                    };
                    canvas.Children.Add(label);
                    Canvas.SetTop(label, 0);
                    Canvas.SetLeft(label, left);
                }
                right -= layerWidth; // Add new layer at left edge of previous layer
            }
        }
        // Save current canvas as image, just before closing FO1_Setup Page
        public static void SaveAsImg(Canvas canvas)
        {
            // Set the Target
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 48d, 48d, PixelFormats.Default); // Default DPI: 96d
            bitmap.Render(canvas);

            // Set Width, Height and Croppings: Create smaller image
            var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(0, 0, (int)canvas.RenderSize.Width/2, (int)canvas.RenderSize.Width/2));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
            string path = "C:/Users/Admin/source/repos/nbungu/BauphysikToolWPF/Resources/ElementImages/";
            string imgName = "Element_"+FO0_LandingPage.SelectedElement.ElementId+".png";

            using (var fs = System.IO.File.OpenWrite(path+imgName))
            {
                pngEncoder.Save(fs);
            }
        }
    }
}
