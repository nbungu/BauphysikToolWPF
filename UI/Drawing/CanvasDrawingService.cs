using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.Drawing
{
    public enum AlignmentVariant
    {
        EvenSpacingCentered,
        OddElementCentered,
        Left
    }
    public enum DrawingType
    {
        CrossSection,
        VerticalCut
    }
    public class CanvasDrawingService
    {
        //private Rectangle _defaultCrossSectionRectangle = new Rectangle(new Point(0, 0), 880, 400);
        //private Rectangle _defaultVerticalCutRectangle = new Rectangle(new Point(0, 0), 400, 880);
        private bool _isOverflowing = false;

        // Static, because globally valid for all Intstances
        public static double SizeOf1Cm = 16.0; // starting value

        public Element Element { get; set; }
        public Rectangle CanvasSize { get; set; }
        public DrawingType DrawingType { get; set; }
        public AlignmentVariant Alignment { get; set; }
        public List<IDrawingGeometry> DrawingGeometries { get; private set; } //=> GetCrossSectionDrawing(Element, CanvasSize);

        public CanvasDrawingService(Element element, Rectangle canvasSize, DrawingType drawingType = DrawingType.CrossSection, AlignmentVariant variant = AlignmentVariant.EvenSpacingCentered)
        {
            Element = element;
            CanvasSize = canvasSize;
            DrawingType = drawingType;
            Alignment = variant;
            DrawingGeometries = GetDrawing();
        }

        public void UpdateDrawings()
        {
            DrawingGeometries = GetDrawing();
        }
        
        // Vertikalschnitt, Canvas in PX
        // Querschnitt, Canvas in PX
        private List<IDrawingGeometry> GetDrawing()
        {
            if (DrawingType == DrawingType.CrossSection) SizeOf1Cm = CanvasSize.Height / Element.Thickness;
            else if (DrawingType == DrawingType.VerticalCut) SizeOf1Cm = CanvasSize.Width / Element.Thickness;
            else SizeOf1Cm = 16.67;

            if (!Element.IsValid) return new List<IDrawingGeometry>();

            // Reset Geometries for selected Drawing Type
            UpdateGeometries();

            // Stacking
            var layerDrawings = Stacking();

            // Sort Ascending by ZIndex
            layerDrawings.Sort(new DrawingGeometryComparer(sortingType: DrawingGeometrySortingType.ZIndexAscending));
            return layerDrawings;
            
        }
        
        private List<IDrawingGeometry> Stacking()
        {
            var layerDrawings = new List<IDrawingGeometry>();
            // Stacking
            Point ptStart = new Point(0, 0);
            foreach (var l in Element.Layers)
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

                    // Only Stack if it shows Faces of Cutted Crossections
                    if ((int)DrawingType == (int)l.SubConstruction.SubConstructionDirection)
                    {
                        // Stack Horizontally
                        if (DrawingType == DrawingType.CrossSection)
                        {
                            int numSubconstructions = (int)Math.Floor((CanvasSize.Width + spacing) / (subConstrWidth + spacing));
                            double startX = 0;

                            if (Alignment == AlignmentVariant.EvenSpacingCentered)
                            {
                                // Adjust to ensure an even number of subconstruction groups
                                if (numSubconstructions % 2 != 0) numSubconstructions--;

                                // Ensure at least two subconstructions
                                numSubconstructions = Math.Max(numSubconstructions, 2);

                                double totalSubconstructionsWidth = numSubconstructions * subConstrWidth + (numSubconstructions - 1) * spacing;
                                _isOverflowing = totalSubconstructionsWidth > CanvasSize.Width;

                                if (_isOverflowing)
                                {
                                    CanvasSize = new Rectangle(CanvasSize.Left, CanvasSize.Top, totalSubconstructionsWidth, CanvasSize.Height);
                                    return GetDrawing();
                                }
                                // If it fits within the CanvasSize width, center normally
                                startX = (CanvasSize.Width - totalSubconstructionsWidth) / 2;

                            }
                            else if (Alignment == AlignmentVariant.OddElementCentered)
                            {
                                // Adjust to ensure at least one subconstruction in the middle
                                if (numSubconstructions % 2 == 0) numSubconstructions--;

                                double totalSubconstructionsWidth = numSubconstructions * subConstrWidth + (numSubconstructions - 1) * spacing;
                                startX = (CanvasSize.Width - totalSubconstructionsWidth) / 2;
                            }

                            for (int i = 0; i < numSubconstructions; i++)
                            {
                                double x = startX + i * (subConstrWidth + spacing);
                                var subConstrGeometry = l.SubConstruction.Convert();
                                subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(x, ptStart.Y));
                                layerDrawings.Add(subConstrGeometry);
                            }
                        }
                        // Stack vertically
                        else if (DrawingType == DrawingType.VerticalCut)
                        {
                            int numSubconstructions = (int)Math.Floor((CanvasSize.Height + spacing) / (subConstrHeight + spacing));
                            double startY = 0;

                            if (Alignment == AlignmentVariant.EvenSpacingCentered)
                            {
                                // Adjust to ensure an even number of subconstruction groups
                                if (numSubconstructions % 2 != 0) numSubconstructions--;

                                // Ensure at least two subconstructions
                                numSubconstructions = Math.Max(numSubconstructions, 2);

                                double totalSubconstructionsHeight = numSubconstructions * subConstrHeight + (numSubconstructions - 1) * spacing;
                                _isOverflowing = totalSubconstructionsHeight > CanvasSize.Height;

                                if (_isOverflowing)
                                {
                                    CanvasSize = new Rectangle(CanvasSize.Left, CanvasSize.Top, CanvasSize.Width, totalSubconstructionsHeight);
                                    return GetDrawing();
                                }
                                // If it fits within the CanvasSize width, center normally
                                startY = (CanvasSize.Height - totalSubconstructionsHeight) / 2;

                            }
                            else if (Alignment == AlignmentVariant.OddElementCentered)
                            {
                                // Adjust to ensure at least one subconstruction in the middle
                                if (numSubconstructions % 2 == 0) numSubconstructions--;

                                double totalSubconstructionsHeight = numSubconstructions * subConstrHeight + (numSubconstructions - 1) * spacing;
                                startY = (CanvasSize.Height - totalSubconstructionsHeight) / 2;
                            }

                            for (int i = 0; i < numSubconstructions; i++)
                            {
                                double y = startY + i * (subConstrHeight + spacing);
                                var subConstrGeometry = l.SubConstruction.Convert();
                                subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(ptStart.X, y));
                                layerDrawings.Add(subConstrGeometry);
                            }
                        }
                    }
                    else
                    {
                        var subConstrGeometry = l.SubConstruction.Convert();
                        subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(ptStart);
                        layerDrawings.Add(subConstrGeometry);
                    }
                }
                // Update Origin
                if (DrawingType == DrawingType.CrossSection) ptStart = l.Rectangle.BottomLeft;
                else if (DrawingType == DrawingType.VerticalCut) ptStart = l.Rectangle.TopRight;
            }
            return layerDrawings;
        }

        private void UpdateGeometries()
        {
            // Updating + Scaling Geometries
            // Scaling to fit Canvas (cm to px conversion)
            foreach (var l in Element.Layers)
            {
                if (DrawingType == DrawingType.CrossSection)
                {
                    l.Rectangle = new Rectangle(new Point(0, 0), CanvasSize.Width, l.Thickness * SizeOf1Cm);
                }
                else if (DrawingType == DrawingType.VerticalCut)
                {
                    l.Rectangle = new Rectangle(new Point(0, 0), l.Thickness * SizeOf1Cm, CanvasSize.Height);
                }
                l.BackgroundColor = new SolidColorBrush(l.Material.Color);
                l.DrawingBrush = HatchPattern.GetHatchPattern(l.Material.Category, 0.5, l.Rectangle);
                l.RectangleBorderColor = l.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
                l.RectangleBorderThickness = l.IsSelected ? 2 : 0.2;
                l.Opacity = l.IsEffective ? 1 : 0.3;
                l.Tag = l.InternalId;

                if (l.HasSubConstruction)
                {
                    // Shows Faces of Cutted Crossections
                    if ((int)DrawingType == (int)l.SubConstruction.SubConstructionDirection)
                    {
                        l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.SubConstruction.Width * SizeOf1Cm, l.SubConstruction.Thickness * SizeOf1Cm);
                        l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 1 : 0.4;
                    }
                    // SubConstruction runs parallel to Main Layer
                    else
                    {
                        l.SubConstruction.Rectangle = l.Rectangle;
                        l.SubConstruction.Opacity = 0.2;
                    }
                    l.SubConstruction.BackgroundColor = new SolidColorBrush(l.SubConstruction.Material.Color);
                    l.SubConstruction.DrawingBrush = HatchPattern.GetHatchPattern(l.SubConstruction.Material.Category, 0.5, l.SubConstruction.Rectangle);
                    l.SubConstruction.RectangleBorderColor = l.SubConstruction.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
                    l.SubConstruction.RectangleBorderThickness = l.SubConstruction.IsSelected ? 1 : 0.2;
                    l.SubConstruction.Tag = l.InternalId;
                }
            }
        }
    }
}
