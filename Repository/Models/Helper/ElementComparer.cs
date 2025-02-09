using System.Collections.Generic;

namespace BauphysikToolWPF.Repository.Models.Helper
{
    public enum ElementSortingType
    {
        DateAscending,
        DateDescending,
        NameAscending,
        NameDescending
    }
    public enum ElementGroupingType
    {
        None,
        Type,
        Orientation,
        Tag
    }
    public class ElementComparer : IComparer<Element>
    {
        // Static Class Variable
        public static List<string> SortingTypes { get; private set; } = new List<string>() { "Änderungsdatum (älteste zuerst)", "Änderungsdatum (neueste zuerst)", "Name (aufsteigend)", "Name (absteigend)" };
        public static List<string> GroupingTypes { get; private set; } = new List<string>() { "Ohne", "Typ", "Ausrichtung", "Tags" };

        // Instance Variables
        public ElementSortingType SortingType { get; set; }
        public ElementGroupingType GroupingType { get; set; }

        public ElementComparer(ElementSortingType sortingType = ElementSortingType.DateAscending)
        {
            SortingType = sortingType;
        }

        // Interface Method
        public int Compare(Element? x, Element? y)
        {
            if (x is null || y is null)
                return 0;
            switch (SortingType)
            {
                case ElementSortingType.DateAscending:
                    return x.UpdatedAt.CompareTo(y.UpdatedAt);
                case ElementSortingType.DateDescending:
                    return y.UpdatedAt.CompareTo(x.UpdatedAt);
                case ElementSortingType.NameAscending:
                    return x.Name.CompareTo(y.Name);
                case ElementSortingType.NameDescending:
                    return y.Name.CompareTo(x.Name);
                default:
                    return x.UpdatedAt.CompareTo(y.UpdatedAt);
            }
        }
    }
}
