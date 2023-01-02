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
    public static class DrawLayerCanvas
    {
        private static StationaryTempCurve tempCurve; // default value = null
        public static bool ShowTempCurve { get; set; } = true; // default value = true
        public static void DrawRectanglesFromLayers(List<Layer> layers, Canvas canvas)
        {
            canvas.Children.Clear();
            
            if (layers == null || layers.Count == 0)
                return;

            tempCurve = new StationaryTempCurve(layers, SurfaceResistance.selectedRsiValue, SurfaceResistance.selectedRseValue, ReferenceTemp.selectedTiValue, ReferenceTemp.selectedTeValue);

            double canvasHeight = canvas.Height;
            double canvasWidth = canvas.Width;
            double bottom = 0;
            double right = canvasWidth;
            double fullWidth = 0;

            //TODO refactor: variablen sollen nicht bei jedem foreach neu initialisiert und zugeweisen werden müssen

            //Get width of all layers combined to get fullWidth
            foreach (Layer layer in layers)
            {
                fullWidth += layer.LayerThickness;
            }
            foreach (Layer layer in layers)
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
                canvas.Children.Add(rect);
                Canvas.SetTop(rect, bottom);
                Canvas.SetLeft(rect, left);

                if (ShowTempCurve == true)
                {
                    int layerPosition = layer.LayerPosition - 1; // convert to 0 based index

                    double shiftRange = (tempCurve.Te < 0) ? Math.Abs(tempCurve.Te) : 0;        // tempHeightScale can get negative values due to negative °C values -> Shift layerTemps up by Te or Ti if negative
                    double layerTempY1 = tempCurve.LayerTemps[layerPosition] + shiftRange;      // right layer side temp value
                    double layerTempY2 = tempCurve.LayerTemps[layerPosition + 1] + shiftRange;  // left layer side temp value
                    double deltaT = tempCurve.Ti - tempCurve.Te;

                    double tempHeightScaleY1 = layerTempY1 / deltaT;
                    double tempHeightScaleY2 = layerTempY2 / deltaT;

                    // Set properties of the temp curve line
                    Line tempCurveLine = new Line()
                    {
                        Stroke = Brushes.Red,
                        X1 = left + (layerWidth),
                        Y1 = canvasHeight - (canvasHeight * tempHeightScaleY1),
                        X2 = left,
                        Y2 = canvasHeight - (canvasHeight * tempHeightScaleY2),
                        StrokeThickness = 1
                    };

                    // Draw temp curve line
                    canvas.Children.Add(tempCurveLine);
                }

                right -= layerWidth; // Add new layer at left edge of previous layer
            }
        }
    }
}
