using BauphysikToolWPF.UI.Drawing;
using Geometry;
using System.Collections.Generic;

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
                }
            }
        }

        #region Drawing Stuff

        public static List<DrawingGeometry> GetLayerDrawings(this Element element, double canvasWidth = 320, double canvasHeight = 400)
        {
            var layerDrawings = new List<DrawingGeometry>();
            if (element.Layers.Count != 0)
            {
                // Updating Geometry
                foreach (var l in element.Layers)
                {
                    l.UpdateGeometry();
                    l.Tag = l.LayerPosition;
                    if (l.HasSubConstruction)
                    {
                        l.SubConstruction.UpdateGeometry();
                        l.Tag = l.LayerPosition;
                    }
                }

                // Scaling to fit Canvas (cm to px conversion)
                foreach (var l in element.Layers)
                {
                    var relativeLayerWidth = l.Rectangle.Width / element.ElementWidth;
                    var newWidth = canvasWidth * relativeLayerWidth;
                    l.Rectangle = new Rectangle(new Point(), newWidth, canvasHeight);
                    l.DrawingBrush = HatchPattern.GetHatchPattern(l.Material.Category, 0.5, newWidth, canvasHeight);

                    // SubConstruction
                    if (l.HasSubConstruction)
                    {
                        var relativeLayerSubConstrWidth = l.SubConstruction.Rectangle.Width / element.ElementWidth;
                        var newWidthSubConstr = canvasWidth * relativeLayerSubConstrWidth;

                        var scFac = newWidthSubConstr / l.SubConstruction.Rectangle.Width;
                        var newHeightSubConstr = l.SubConstruction.Rectangle.Height * scFac;

                        l.SubConstruction.Rectangle = new Rectangle(new Point(), newWidthSubConstr, newHeightSubConstr);
                        l.SubConstruction.DrawingBrush = HatchPattern.GetHatchPattern(l.SubConstruction.Material.Category, 0.5, newWidthSubConstr, newHeightSubConstr);
                    }
                }

                // Stacking
                Point ptStart = new Point(0, 0);
                foreach (var l in element.Layers)
                {
                    // Layer
                    l.Rectangle = l.Rectangle.MoveTo(ptStart);
                    // SubConstruction
                    if (l.HasSubConstruction) l.SubConstruction.Rectangle = l.SubConstruction.Rectangle.MoveTo(ptStart);
                    // Update Origin
                    ptStart = l.Rectangle.TopRight;
                    // Add to List
                    layerDrawings.Add((DrawingGeometry)l.Convert());
                    if (l.HasSubConstruction) layerDrawings.Add((DrawingGeometry)l.SubConstruction.Convert());
                }
                layerDrawings.Sort(new DrawingGeometryComparer(DrawingGeometrySortingType.ZIndexAscending));
            }
            return layerDrawings;
        }

        #endregion
    }
}
