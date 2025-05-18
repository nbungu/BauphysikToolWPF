using System.Collections.Generic;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.Domain.Helper
{
    public class ElementComparer : IComparer<Element>
    {
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
