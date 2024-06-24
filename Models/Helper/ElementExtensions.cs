using BauphysikToolWPF.UI.Helper;
using Geometry;

namespace BauphysikToolWPF.Models.Helper
{
    public static class ElementExtensions
    {
        public static void AssignInternalIdsToLayers(this Element element)
        {
            if (element.Layers.Count != 0)
            {
                int index = 0; // Start at 0
                element.Layers.ForEach(e => e.InternalId = index++);
            }
        }

        // Sets the 'LayerPosition' of a Layer List from 1 to N, without missing values inbetween
        // Layers have to be SORTED (LayerPos)
        public static void SortLayers(this Element element)
        {
            if (element.Layers.Count != 0)
            {
                element.Layers.Sort((a, b) => a.LayerPosition.CompareTo(b.LayerPosition));
                // Fix postioning
                int index = 0; // Start at 0
                element.Layers.ForEach(e => e.LayerPosition = index++);
            }
        }

        public static void AssignEffectiveLayers(this Element element)
        {
            if (element.Layers.Count != 0)
            {
                bool foundAirLayer = false;
                foreach (Layer layer in element.Layers)
                {
                    if (layer.Material.Category == MaterialCategory.Air)
                        foundAirLayer = true;
                    layer.IsEffective = !foundAirLayer;
                }
            }
        }

        #region Drawing Stuff

        public static void ScaleAndStackLayers(this Element element, double canvasWidth = 320, double canvasHeight = 400)
        {
            element.UpdateLayerGeometries();
            element.ScaleToFitCanvas(canvasWidth, canvasHeight);
            element.StackLayers();
        }

        private static void UpdateLayerGeometries(this Element element)
        {
            if (element.Layers.Count != 0)
            {
                foreach (var l in element.Layers)
                {
                    l.UpdateGeometry();
                    if (l.HasSubConstruction) l.SubConstruction.UpdateGeometry();
                }
                
            }
        }

        private static void StackLayers(this Element element)
        {
            if (element.Layers.Count == 0) return;

            Point ptStart = new Point(0, 0);
            foreach (var layer in element.Layers)
            {
                layer.Rectangle = layer.Rectangle.MoveTo(ptStart);
                ptStart = layer.Rectangle.TopRight;
            }
        }

        private static void ScaleToFitCanvas(this Element element, double canvasWidth = 320, double canvasHeight = 400)
        {
            if (element.Layers.Count == 0) return;

            foreach (var layer in element.Layers)
            {
                var relativeLayerWidth = layer.Rectangle.Width / element.ElementWidth;
                var newWidth = canvasWidth * relativeLayerWidth;
                layer.Rectangle = new Rectangle(new Point(), newWidth, canvasHeight);
                layer.DrawingBrush = HatchPattern.GetHatchPattern(layer.Material.Category, 0.5, newWidth, canvasHeight);
            }
        }

        #endregion
    }
}
