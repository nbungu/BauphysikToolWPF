using System.Collections.Generic;

namespace BauphysikToolWPF.Models.Helper
{
    public enum ElementSortingType
    {
        Date,
        Name
    }
    public enum ElementGroupingType
    {
        None,
        Type,
        Orientation,
        Color,
        Tag
    }
    public class ElementOrganisor : IComparer<Element>
    {
        // Static Class Variable
        public static List<string> SortingTypes { get; private set; } = new List<string>() { "Änderungsdatum", "Name" };
        public static List<string> GroupingTypes { get; private set; } = new List<string>() { "Ohne", "Typ", "Ausrichtung", "Farbe", "Tags" };

        // Instance Variables
        public ElementSortingType SortingType { get; set; }
        public ElementGroupingType GroupingType { get; set; }

        public ElementOrganisor(ElementSortingType sortingType = ElementSortingType.Date)
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
                case ElementSortingType.Date:
                    return x.ElementId.CompareTo(y.ElementId);
                case ElementSortingType.Name:
                    return x.Name.CompareTo(y.Name);
                default:
                    return x.ElementId.CompareTo(y.ElementId);
            }
        }
    }
}
