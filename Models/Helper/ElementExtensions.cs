using Geometry;
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

        public static Rectangle CalculationAreaBounds(this Element element)
        {
            // Return 100 x 100 cm Rectangle by default
            if (!element.IsValid || !element.Layers.Any(l => l.HasSubConstruction)) return new Rectangle(new Point(0, 0), 100, 100);

            var subConstructions = element.Layers.Select(l => l.SubConstruction);

            var verticalLayerSubConstructions =
                subConstructions.Where(l => l.SubConstructionDirection == SubConstructionDirection.Vertical).ToList();
            var horizontalLayerSubConstructions =
                subConstructions.Where(l => l.SubConstructionDirection == SubConstructionDirection.Horizontal).ToList();


            var boundsInXDirection = verticalLayerSubConstructions.Count > 0 ? verticalLayerSubConstructions.Max(l => l.Spacing + l.Width) : 100;
            var boundsInZDirection = horizontalLayerSubConstructions.Count > 0 ? horizontalLayerSubConstructions.Max(l => l.Spacing + l.Width) : 100;
            return new Rectangle(new Point(0, 0), boundsInXDirection, boundsInZDirection);
        }

    }
}
