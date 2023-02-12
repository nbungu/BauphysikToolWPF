using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
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
        public DrawLayerCanvas(Canvas canvas, List<Layer> layers, bool showPositionLabel = true)
        {
            canvas.Children.Clear();

            if (layers == null || layers.Count == 0)
                return;

            // check if canvas was already created in frontend
            canvas = canvas ?? throw new ArgumentNullException(nameof(canvas) + " is not initialized or not found");

            double x = 0;
            double elementWidth = 0;
            foreach (Layer layer in layers)
            {
                elementWidth += layer.LayerThickness;
            }

            // Drawing from left to right: first layer in list is inside
            foreach (Layer layer in layers)
            {
                double layerWidthScale = layer.LayerThickness / elementWidth; // from  0 ... 1
                double layerWidth = canvas.Width * layerWidthScale;

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
                Canvas.SetLeft(baseRect, x);

                // Draw hatch pattern rectangle
                Rectangle hatchPatternRect = new Rectangle()
                {
                    Width = layerWidth, // -1 to leave small gap between hatching and layer border
                    Height = canvas.Height,
                    Fill = HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, layerWidth, canvas.Height),
                    Opacity = 0.6
                };
                canvas.Children.Add(hatchPatternRect);
                Canvas.SetTop(hatchPatternRect, 0);
                Canvas.SetLeft(hatchPatternRect, x + 0.5);

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
                    Canvas.SetLeft(label, x);
                }
                x += layerWidth; // Draw next Layer on right side of previous
            }
        }

        public static void SaveAsPNG(Canvas target, string path = "C:/Users/arnes/source/repos/BauphysikToolWPF/Resources/ElementImages/")
        {
            //string path2 = Environment.CurrentDirectory; //"C:\\Users\\arnes\\source\\repos\\BauphysikToolWPF\\bin\\Debug\\net6.0-windows"
            
            // Set the Bitmap size and target to save
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)target.RenderSize.Width, (int)target.RenderSize.Height, 48d, 48d, PixelFormats.Default); // Default DPI: 96d
            bitmap.Render(target);

            // Set Width, Height and Croppings: Create always img of same size, regardless of current canvas dimensions
            var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(0, 0, (int)target.RenderSize.Width/2, (int)target.RenderSize.Width/2));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            string imgName = "Element_"+FO0_LandingPage.SelectedElement.ElementId+".png";

            // use using to call Dispose() after use of unmanaged resources. GC cannot manage this
            using (var fileStream = File.OpenWrite(path+imgName))
            {
                encoder.Save(fileStream);
            }
        }

        public static byte[] SaveAsBLOB(Canvas target)
        {
            // Convert the BitmapImage to a byte array
            byte[] imageBytes;

            // Set the Bitmap size and target to save
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)target.RenderSize.Width, (int)target.RenderSize.Height, 48d, 48d, PixelFormats.Default); // Default DPI: 96d
            bitmap.Render(target);

            // Set Width, Height and Croppings: Create always img of same size, regardless of current canvas dimensions
            var croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(0, 0, (int)target.RenderSize.Width / 2, (int)target.RenderSize.Width / 2));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            // use using to call Dispose() after use of unmanaged resources. GC cannot manage this
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                imageBytes = stream.ToArray();
            }
            return imageBytes;
        }
    }
}
