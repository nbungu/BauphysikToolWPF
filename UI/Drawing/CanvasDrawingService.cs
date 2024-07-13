using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using Geometry;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.Drawing
{
    public enum AlignmentVariant
    {
        EvenSpacingCentered,
        OddElementCentered,
        Left
    }
    public class CanvasDrawingService
    {
        private Rectangle _defaultCrossSectionRectangle = new Rectangle(new Point(0, 0), 880, 400);
        private Rectangle _defaultVerticalCutRectangle = new Rectangle(new Point(0, 0), 400, 880);
        private bool _isOverflowing = false;

        // Static, because globally valid for all Intstances
        public static double SizeOf1Cm = 16.0; // starting value
        public Element Element { get; set; }
        public Rectangle CanvasSize { get; set; }
        public List<IDrawingGeometry> DrawingGeometries { get; set; } //=> GetCrossSectionDrawing(Element, CanvasSize);

        public CanvasDrawingService(Element element, Rectangle canvasSize)
        {
            Element = element;
            CanvasSize = canvasSize;
            DrawingGeometries = GetCrossSectionDrawing(Element, CanvasSize);
        }

        public void UpdateDrawings()
        {
            DrawingGeometries = GetCrossSectionDrawing(Element, CanvasSize);
        }

        // Querschnitt, Canvas in PX
        private List<IDrawingGeometry> GetCrossSectionDrawing(Element element, Rectangle canvas, AlignmentVariant variant = AlignmentVariant.EvenSpacingCentered)
        {
            var layerDrawings = new List<IDrawingGeometry>();
            SizeOf1Cm = canvas.Height / element.Thickness;

            if (element.IsValid)
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
                    l.Rectangle = new Rectangle(new Point(), canvas.Width, l.Rectangle.Height * SizeOf1Cm);
                    l.DrawingBrush = HatchPattern.GetHatchPattern(l.Material.Category, 0.5, l.Rectangle);

                    // SubConstruction
                    if (l.HasSubConstruction)
                    {
                        l.SubConstruction.Rectangle = l.SubConstruction.Rectangle.Scale(SizeOf1Cm); // new Rectangle(new Point(), l.SubConstruction.Rectangle.Width * sizeOf1Cm, l.SubConstruction.Rectangle.Height * sizeOf1Cm);
                        l.SubConstruction.DrawingBrush = HatchPattern.GetHatchPattern(l.SubConstruction.Material.Category, 0.5, l.SubConstruction.Rectangle);
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
                        double spacing = l.SubConstruction.Spacing * SizeOf1Cm;

                        int numSubconstructions = (int)Math.Floor((canvas.Width + spacing) / (subConstrWidth + spacing));
                        double startX = 0;

                        if (variant == AlignmentVariant.EvenSpacingCentered)
                        {
                            // Adjust to ensure an even number of subconstruction groups
                            if (numSubconstructions % 2 != 0) numSubconstructions--;

                            // Ensure at least two subconstructions
                            numSubconstructions = Math.Max(numSubconstructions, 2);

                            double totalSubconstructionsWidth = numSubconstructions * subConstrWidth + (numSubconstructions - 1) * spacing;
                            _isOverflowing = totalSubconstructionsWidth > canvas.Width;
                            
                            if (_isOverflowing)
                            {
                                CanvasSize = new Rectangle(CanvasSize.Left, CanvasSize.Top, totalSubconstructionsWidth, CanvasSize.Height);
                                return GetCrossSectionDrawing(Element, CanvasSize, variant);
                            }
                            // If it fits within the canvas width, center normally
                            startX = (canvas.Width - totalSubconstructionsWidth) / 2;
                            
                        }
                        else if (variant == AlignmentVariant.OddElementCentered)
                        {
                            // Adjust to ensure at least one subconstruction in the middle
                            if (numSubconstructions % 2 == 0) numSubconstructions--;

                            double totalSubconstructionsWidth = numSubconstructions * subConstrWidth + (numSubconstructions - 1) * spacing;
                            startX = (canvas.Width - totalSubconstructionsWidth) / 2;
                        }

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

                // Sort Ascending by ZIndex
                layerDrawings.Sort(new DrawingGeometryComparer(sortingType: DrawingGeometrySortingType.ZIndexAscending));
            }
            return layerDrawings;
        }
    }
}
