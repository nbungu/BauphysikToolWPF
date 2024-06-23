using BauphysikToolWPF.UI.Helper;
using Geometry;
using System.Collections.Generic;

namespace BauphysikToolWPF.Models.Helper
{
    public static class LayerExtensions
    {
        public static void AssignInternalIdsToLayers(this List<Layer> layers)
        {
            if (layers.Count != 0)
            {
                int index = 0; // Start at 0
                layers.ForEach(e => e.InternalId = index++);
            }
        }

        // Sets the 'LayerPosition' of a Layer List from 1 to N, without missing values inbetween
        // Layers have to be SORTED (LayerPos)
        public static void SortLayers(this List<Layer> layers)
        {
            if (layers.Count != 0)
            {
                layers.Sort((a, b) => a.LayerPosition.CompareTo(b.LayerPosition));
                // Fix postioning
                int index = 0; // Start at 0
                layers.ForEach(e => e.LayerPosition = index++);
            }
        }

        public static void AssignEffectiveLayers(this List<Layer> layers)
        {
            if (layers.Count != 0)
            {
                bool foundAirLayer = false;
                foreach (var layer in layers)
                {
                    if (layer.Material.Category == MaterialCategory.Air)
                        foundAirLayer = true;
                    layer.IsEffective = !foundAirLayer;
                }
            }
        }

        public static void UpdateLayerGeometries(this List<Layer> layers)
        {
            if (layers.Count != 0)
            {
                layers.ForEach(l => l.UpdateGeometry(l));
            }
        }


        #region Drawing Stuff

        public static void ScaleAndStackLayers(this List<Layer> layers, double canvasWidth = 320, double canvasHeight = 400)
        {
            layers.UpdateLayerGeometries();
            layers.ScaleToFitCanvas(canvasWidth, canvasHeight);
            layers.StackLayers();
        }

        public static void StackLayers(this List<Layer> layers)
        {
            if (layers.Count == 0) return;

            Point ptStart = new Point(0, 0);
            foreach (var layer in layers)
            {
                layer.Rectangle.MoveTo(ptStart);
                ptStart = layer.Rectangle.TopRight;
            }
        }

        public static void ScaleToFitCanvas(this List<Layer> layers, double canvasWidth = 320, double canvasHeight = 400)
        {
            if (layers.Count == 0) return;

            double elementWidth = 0;
            layers.ForEach(l => elementWidth += l.LayerThickness);

            foreach (var layer in layers)
            {
                var relativeLayerWidth = layer.Rectangle.Width / elementWidth;
                var newWidth = canvasWidth * relativeLayerWidth;
                layer.Rectangle = new Rectangle(new Point(), newWidth, canvasHeight);
                layer.DrawingBrush = HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, newWidth, canvasHeight);
            }
        }

        #endregion
    }
}
