using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo.Helper
{
    public enum LayerSortingType
    {
        None,
        Name,
        LayerPosition,
        Default = LayerPosition
    }
    public class LayerSorting : IComparer<Layer>
    {
        // Static Class Variable
        public static List<string> TypeList { get; private set; } = new List<string>() { "Schicht ID", "Materialname", "Position" };

        // Instance Variables
        public LayerSortingType SortingType { get; set; }
        public string Name { get; set; }
        public string PropertyName { get; set; }
        public int Index { get; set; }

        public LayerSorting(LayerSortingType sortingType = LayerSortingType.Default)
        {
            SortingType = sortingType;
            //AssignProperties(SortingType);
        }
        public void AssignProperties(LayerSortingType sortingType)
        {
            switch (sortingType)
            {
                case LayerSortingType.None:
                    Name = TypeList[(int)sortingType];
                    PropertyName = "LayerId";
                    Index = (int)sortingType;
                    return;
                case LayerSortingType.Name:
                    Name = TypeList[(int)sortingType];
                    PropertyName = "Material.Name";
                    Index = (int)sortingType;
                    return;
                case LayerSortingType.LayerPosition:
                    Name = TypeList[(int)sortingType];
                    PropertyName = "LayerPosition";
                    Index = (int)sortingType;
                    return;
                default:
                    Name = TypeList[(int)LayerSortingType.LayerPosition];
                    PropertyName = "LayerPosition";
                    Index = (int)LayerSortingType.LayerPosition;
                    return;
            }
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
                    DatabaseAccess.UpdateLayer(layer, triggerUpdateEvent: false); // triggerUpdateEvent: false -> avoid notification loop
                }
            }
        }
    }    
}
