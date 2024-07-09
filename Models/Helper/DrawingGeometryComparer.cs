using System.Collections.Generic;
using BauphysikToolWPF.UI.Drawing;

namespace BauphysikToolWPF.Models.Helper
{
    public enum DrawingGeometrySortingType
    {
        ZIndexAscending,
        ZIndexDescending
    }

    public class DrawingGeometryComparer : IComparer<IDrawingGeometry>
    {

        // Instance Variables
        public DrawingGeometrySortingType SortingType { get; set; }

        public DrawingGeometryComparer(DrawingGeometrySortingType sortingType = DrawingGeometrySortingType.ZIndexAscending)
        {
            SortingType = sortingType;
        }

        // Interface Method
        public int Compare(IDrawingGeometry? x, IDrawingGeometry? y)
        {
            if (x is null || y is null)
                return 0;
            switch (SortingType)
            {
                case DrawingGeometrySortingType.ZIndexAscending:
                    return x.ZIndex.CompareTo(y.ZIndex);
                case DrawingGeometrySortingType.ZIndexDescending:
                    return y.ZIndex.CompareTo(x.ZIndex);
                default:
                    return x.ZIndex.CompareTo(y.ZIndex);
            }
        }
    }
}
