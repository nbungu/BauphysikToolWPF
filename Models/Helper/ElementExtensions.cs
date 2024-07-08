using BauphysikToolWPF.UI.Drawing;
using Geometry;
using System;
using System.Collections.Generic;
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
                }
            }
        }

        #region Drawing Stuff

        public static List<IDrawingGeometry> GetLayerDrawings(this Element element, double canvasWidth = 880, double canvasHeight = 400)
        {
            var layerDrawings = new List<IDrawingGeometry>();
            DrawingGeometry.SizeOf1Cm = canvasHeight / element.Thickness_cm;

            if (element.Layers.Count != 0)
            {
                // Updating Geometry
                foreach (var l in element.Layers)
                {
                    l.UpdateGeometry();
                    l.Tag = l.InternalId;
                    if (l.HasSubConstruction)
                    {
                        l.SubConstruction.UpdateGeometry();
                        l.SubConstruction.Tag = l.InternalId;
                    }
                }

                // Scaling to fit Canvas (cm to px conversion)
                foreach (var l in element.Layers)
                {
                    l.Rectangle = new Rectangle(new Point(), canvasWidth, l.Rectangle.Height * DrawingGeometry.SizeOf1Cm);
                    l.DrawingBrush = HatchPattern.GetHatchPattern(l.Material.Category, 0.5, l.Rectangle.Width, l.Rectangle.Height);

                    // SubConstruction
                    if (l.HasSubConstruction)
                    {
                        l.SubConstruction.Rectangle = l.SubConstruction.Rectangle.Scale(DrawingGeometry.SizeOf1Cm); // new Rectangle(new Point(), l.SubConstruction.Rectangle.Width * sizeOf1Cm, l.SubConstruction.Rectangle.Height * sizeOf1Cm);
                        l.SubConstruction.DrawingBrush = HatchPattern.GetHatchPattern(l.SubConstruction.Material.Category, 0.5, l.SubConstruction.Rectangle.Width, l.SubConstruction.Rectangle.Height);
                    }
                }

                // Stacking
                Point ptStart = new Point(0, 0);
                foreach (var l in element.Layers)
                {
                    // Main Layer Geometry
                    l.Rectangle = l.Rectangle.MoveTo(ptStart);
                    layerDrawings.Add(l.Convert());

                    // SubConstruction
                    if (l.HasSubConstruction)
                    {
                        double subConstrWidth = l.SubConstruction.Rectangle.Width;
                        double subConstrHeight = l.SubConstruction.Rectangle.Height;
                        double spacing = l.SubConstruction.Spacing * DrawingGeometry.SizeOf1Cm;

                        int numSubconstructions = (int)Math.Floor((canvasWidth + spacing) / (subConstrWidth + spacing));
                        double totalSubconstructionsWidth = numSubconstructions * subConstrWidth + (numSubconstructions - 1) * spacing;
                        double startX = (canvasWidth - totalSubconstructionsWidth) / 2;

                        for (int i = 0; i < numSubconstructions; i++)
                        {
                            double x = startX + i * (subConstrWidth + spacing);
                            var subConstrGeometry = l.SubConstruction.Convert();
                            subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(x, ptStart.Y));
                            layerDrawings.Add(subConstrGeometry);
                        }
                    }
                    // Update Origin
                    ptStart = l.Rectangle.BottomLeft;
                }
                layerDrawings.Sort(new DrawingGeometryComparer(DrawingGeometrySortingType.ZIndexAscending));
            }
            return layerDrawings;
        }

        #endregion
    }
}
