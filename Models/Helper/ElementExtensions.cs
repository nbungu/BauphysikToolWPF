using System.Linq;

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
                    if (layer.HasSubConstruction) layer.SubConstruction.IsEffective = layer.IsEffective;
                }
            }
        }

        public static void MoveLayerPositionToInside(this Element element, int targetLayerId)
        {
            if (element is null || element.Layers.Count == 0) return;
            var targetLayer = element.Layers.First(l => l.InternalId == targetLayerId);
            // When Layer is already at the top of the List (first in the List)
            if (targetLayer.LayerPosition == 0) return;
            // Change Positions
            Layer neighbour = element.Layers.Find(l => l.LayerPosition == targetLayer.LayerPosition - 1);
            neighbour.LayerPosition += 1;
            targetLayer.LayerPosition -= 1;
        }

        public static void MoveLayerPositionToOutside(this Element element, int targetLayerId)
        {
            if (element is null || element.Layers.Count == 0) return;
            var targetLayer = element.Layers.First(l => l.InternalId == targetLayerId);
            // When Layer is already at the bottom of the List (last in the List)
            if (targetLayer.LayerPosition == element.Layers.Count - 1) return;
            // Change Positions
            Layer neighbour = element.Layers.Find(l => l.LayerPosition == targetLayer.LayerPosition + 1);
            neighbour.LayerPosition -= 1;
            targetLayer.LayerPosition += 1;
        }
    }
}
