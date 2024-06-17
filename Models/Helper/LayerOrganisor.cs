using System.Collections.Generic;
using BauphysikToolWPF.Repository;

namespace BauphysikToolWPF.Models.Helper
{
    public enum LayerSortingType
    {
        None,
        Name,
        LayerPosition,
        Default = LayerPosition
    }
    public class LayerOrganisor : IComparer<Layer>
    {
        // Static Class Variable
        public static List<string> SortingTypes { get; private set; } = new List<string>() { "Schicht ID", "Materialname", "Position" };

        // Instance Variables
        public LayerSortingType SortingType { get; set; }

        public LayerOrganisor(LayerSortingType sortingType = LayerSortingType.Default)
        {
            SortingType = sortingType;
        }

        // Interface Method
        public int Compare(Layer? x, Layer? y)
        {
            if (x is null || y is null)
                return 0;
            switch (SortingType)
            {
                case LayerSortingType.Name:
                    return x.Material.Name.CompareTo(y.Material.Name);
                case LayerSortingType.LayerPosition:
                    return x.LayerPosition.CompareTo(y.LayerPosition);
                default:
                    return x.LayerPosition.CompareTo(y.LayerPosition);
            }
        }
        // Sets the 'LayerPosition' of a Layer List from 1 to N, without missing values inbetween
        // Layers have to be SORTED (LayerPos)
        public static void FillGaps(List<Layer> layers)
        {
            if (layers.Count > 0)
            {
                // Update the Id property of the remaining objects
                for (int i = 0; i < layers.Count; i++)
                {
                    layers[i].LayerPosition = i;
                    DatabaseAccess.UpdateLayer(layers[i], triggerUpdateEvent: false); // triggerUpdateEvent: false -> avoid notification loop
                }
            }
        }
        // Set every following Layer after AirLayer to IsEffective = false
        // Layers have to be SORTED (LayerPos) + No Gaps (FillGaps)
        public static void AssignEffectiveLayers(List<Layer> layers)
        {
            if (layers.Count > 0)
            {
                bool foundAirLayer = false;
                foreach (Layer layer in layers)
                {
                    if (layer.Material.Category == MaterialCategory.Air)
                        foundAirLayer = true;
                    layer.IsEffective = !foundAirLayer;
                    //DatabaseAccess.UpdateLayer(layer, triggerUpdateEvent: false); // triggerUpdateEvent: false -> avoid notification loop
                }
            }
        }
    }
}
