using System.Collections.Generic;

namespace BauphysikToolWPF.Models.Helper
{
    public enum LayerSortingType
    {
        DateAscending,
        DateDescending,
        NameAscending,
        NameDescending,
        LayerPositionAscending,
        LayerPositionDescending
    }
    public class LayerComparer : IComparer<Layer>
    {
        // Static Class Variable
        public static List<string> SortingTypes { get; private set; } = new List<string>() { "Änderungsdatum (älteste zuerst)", "Änderungsdatum (neueste zuerst)", "Name (aufsteigend)", "Name (absteigend)", "Position", "Position umgekehrt" };

        // Instance Variables
        public LayerSortingType SortingType { get; set; }

        public LayerComparer(LayerSortingType sortingType = LayerSortingType.LayerPositionAscending)
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
                case LayerSortingType.DateAscending:
                    return x.UpdatedAt.CompareTo(y.UpdatedAt);
                case LayerSortingType.DateDescending:
                    return y.UpdatedAt.CompareTo(x.UpdatedAt);
                case LayerSortingType.NameAscending:
                    return x.Material.Name.CompareTo(y.Material.Name);
                case LayerSortingType.NameDescending:
                    return y.Material.Name.CompareTo(x.Material.Name);
                case LayerSortingType.LayerPositionAscending:
                    return x.LayerPosition.CompareTo(y.LayerPosition);
                case LayerSortingType.LayerPositionDescending:
                    return y.LayerPosition.CompareTo(x.LayerPosition);
                default:
                    return x.LayerPosition.CompareTo(y.LayerPosition);
            }
        }
    }
}
