using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
{
    public class DrawLayerCanvas
    {
        private StationaryTempCurve TempCurve { get; set; }
        public List<Layer> Layers { get; set; } 
        public Canvas Canvas { get; set; }  

        public DrawLayerCanvas(List<Layer> layers, Canvas canvas)
        {
            this.TempCurve = new StationaryTempCurve(layers, RSurfaces.selectedRsi.First().Value, RSurfaces.selectedRse.First().Value, Temperatures.selectedTi.First().Value, Temperatures.selectedTe.First().Value);
            this.Layers = layers;
            this.Canvas = canvas;
            DrawRectanglesFromLayers();
        }

        public void DrawRectanglesFromLayers()
        {
            Canvas.Children.Clear();
            
            if (Layers == null || Layers.Count == 0)
                return;

            double canvasHeight = Canvas.Height;
            double canvasWidth = Canvas.Width;
            double bottom = 0;
            double right = canvasWidth;
            double fullWidth = 0;

            //TODO refactor: variablen sollen nicht bei jedem foreach neu initialisiert und zugeweisen werden müssen

            //Get width of all layers combined to get fullWidth
            foreach (Layer layer in Layers)
            {
                fullWidth += layer.LayerThickness;
            }
            foreach (Layer layer in Layers)
            {
                double layerWidthScale = layer.LayerThickness / fullWidth; // from  0 ... 1
                double layerWidth = canvasWidth * layerWidthScale;
                double left = right - layerWidth; // start drawing from right canvas side (beginning with INSIDE Layer, which is first list element) -> We want Inside layer position on right/inner side. 

                // Set properties of the layer rectangle
                Rectangle rect = new Rectangle()
                {
                    Width = layerWidth,
                    Height = canvasHeight,
                    Stroke = layer.IsSelected ? Brushes.Red : Brushes.Black,
                    StrokeThickness = layer.IsSelected ? 1 : 0.4,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode))
                };

                // Draw layer rectangle
                Canvas.Children.Add(rect);
                Canvas.SetTop(rect, bottom);
                Canvas.SetLeft(rect, left);

                right -= layerWidth; // Add new layer at left edge of previous layer
            }
        }
    }
}
