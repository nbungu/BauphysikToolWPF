using System.Collections.Generic;

namespace BauphysikToolWPF.Repository.Models.Helper
{
    public enum MaterialSortingType
    {
        CategoryAscending,
        CategoryDescending,
        NameAscending,
        NameDescending
    }

    public class MaterialComparer : IComparer<Material>
    {

        // Instance Variables
        public MaterialSortingType SortingType { get; set; }

        public MaterialComparer(MaterialSortingType sortingType = MaterialSortingType.CategoryAscending)
        {
            SortingType = sortingType;
        }

        // Interface Method
        public int Compare(Material? x, Material? y)
        {
            if (x is null || y is null)
                return 0;
            switch (SortingType)
            {
                case MaterialSortingType.CategoryAscending:
                    return x.Category.CompareTo(y.Category);
                case MaterialSortingType.CategoryDescending:
                    return y.Category.CompareTo(x.Category);
                case MaterialSortingType.NameAscending:
                    return x.Name.CompareTo(y.Name);
                case MaterialSortingType.NameDescending:
                    return y.Name.CompareTo(x.Name);
                default:
                    return x.Category.CompareTo(y.Category);
            }
        }
    }
}
