using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain.Helper
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
        public static Element SortLayers(this Element element)
        {
            if (element.Layers.Count != 0)
            {
                element.Layers.Sort((a, b) => a.LayerPosition.CompareTo(b.LayerPosition));
                // Fix postioning
                int index = 0; // Start at 0
                element.Layers.ForEach(e => e.LayerPosition = index++);
            }
            return element;
        }

        public static void AssignEffectiveLayers(this Element element)
        {
            if (element.Layers.Count != 0)
            {
                bool foundAirLayer = false;
                foreach (Layer layer in element.Layers)
                {
                    if (layer.Material.MaterialCategory == MaterialCategory.Air) foundAirLayer = true;

                    layer.IsEffective = !foundAirLayer;
                    if (layer.SubConstruction != null) layer.SubConstruction.IsEffective = layer.IsEffective;
                }
            }
        }

        public static void DuplicateLayer(this Element element, Layer layer)
        {
            var copy = layer.Copy();
            copy.LayerPosition = element.Layers.Count;
            copy.InternalId = element.Layers.Count;
            element.AddLayer(copy);
        }

        public static void DuplicateLayerById(this Element element, int layerId)
        {
            if (element.Layers.Count == 0) return;
            var copy = element.Layers.First(l => l.InternalId == layerId).Copy();
            copy.LayerPosition = element.Layers.Count;
            copy.InternalId = element.Layers.Count;
            element.AddLayer(copy);
        }

        public static void MoveLayerPositionToInside(this Element element, int layerId)
        {
            if (element.Layers.Count == 0) return;
            var targetLayer = element.Layers.First(l => l.InternalId == layerId);
            // When Layer is already at the top of the List (first in the List)
            if (targetLayer.LayerPosition == 0) return;
            // Change Positions
            Layer? neighbour = element.Layers.Find(l => l.LayerPosition == targetLayer.LayerPosition - 1);
            if (neighbour == null) return;
            neighbour.LayerPosition += 1;
            targetLayer.LayerPosition -= 1;

            element.SortLayers();
            element.AssignEffectiveLayers();
        }

        public static void MoveLayerPositionToOutside(this Element element, int layerId)
        {
            if (element.Layers.Count == 0) return;
            var targetLayer = element.Layers.First(l => l.InternalId == layerId);
            // When Layer is already at the bottom of the List (last in the List)
            if (targetLayer.LayerPosition == element.Layers.Count - 1) return;
            // Change Positions
            Layer? neighbour = element.Layers.Find(l => l.LayerPosition == targetLayer.LayerPosition + 1);
            if (neighbour == null) return;
            neighbour.LayerPosition -= 1;
            targetLayer.LayerPosition += 1;

            element.SortLayers();
            element.AssignEffectiveLayers();
        }

        public static void RemoveLayerById(this Element element, int layerId)
        {
            if (element.Layers.Count == 0) return;
            var targetLayer = element.Layers.First(l => l.InternalId == layerId);
            element.Layers.Remove(targetLayer);
            //
            element.SortLayers();
            element.AssignInternalIdsToLayers();
            element.AssignEffectiveLayers();
        }

        public static void AddLayer(this Element element, Layer newLayer)
        {
            element.Layers.Add(newLayer);
            element.SortLayers();
            element.AssignInternalIdsToLayers();
            element.AssignEffectiveLayers();
        }

        public static void UnselectAllLayers(this Element element)
        {
            element.Layers.ForEach(l => l.IsSelected = false);
        }
    }
}
