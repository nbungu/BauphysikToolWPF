using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using BT.Geometry;
using BT.Logging;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Enums = BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Services.UI
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
    public class CrossSectionBuilder
    {
        private bool _isOverflowing;

        // Static, because globally valid for all Instances
        public static double SizeOf1Cm = 16.0; // starting value

        public Element Element { get; set; }
        public Rectangle CanvasSize { get; set; }
        private DrawingType _drawingType = DrawingType.CrossSection;

        public DrawingType DrawingType
        {
            get => _drawingType;
            set
            {
                if (value == DrawingType.CrossSection)
                    CanvasSize = new Rectangle(new Point(0, 0), 880, 400);
                else if (value == DrawingType.VerticalCut)
                    CanvasSize = new Rectangle(new Point(0, 0), 400, 880);
                else CanvasSize = new Rectangle(new Point(0, 0), 880, 880);
                _drawingType = value;
            }
        }
        public AlignmentVariant Alignment { get; set; }
        public List<IDrawingGeometry> DrawingGeometries { get; private set; }
        public bool LastDrawingSuccessful { get; private set; }

        public CrossSectionBuilder()
        {
            Element = new Element();
            DrawingType = DrawingType.CrossSection;
            Alignment = AlignmentVariant.EvenSpacingCentered;
            DrawingGeometries = new List<IDrawingGeometry>();
        }

        public CrossSectionBuilder(Element element, DrawingType type)
        {
            Element = element;
            DrawingType = type;
            Alignment = AlignmentVariant.EvenSpacingCentered;
            DrawingGeometries = GetDrawing();
        }

        public CrossSectionBuilder(Element? element, Rectangle canvasSize, DrawingType drawingType, AlignmentVariant variant = AlignmentVariant.EvenSpacingCentered)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element), "Element cannot be null.");
            DrawingType = drawingType;
            CanvasSize = canvasSize;
            Alignment = variant;
            DrawingGeometries = GetDrawing();
        }

        public void RebuildCrossSection()
        {
            DrawingGeometries = GetDrawing();
            Logger.LogInfo($"Updated cross section drawing of element: {Element}.");
        }

        // Vertikalschnitt, Canvas in PX
        // Querschnitt, Canvas in PX
        private List<IDrawingGeometry> GetDrawing()
        {
            if (!Element.IsValid) return new List<IDrawingGeometry>();

            SetUpPixelResolution();

            UpdateGeometries();

            // Stacking
            var layerDrawings = ReturnStackedGeometries();

            // Sort Ascending by ZIndex
            layerDrawings.Sort(new DrawingGeometryComparer(sortingType: DrawingGeometrySortingType.ZIndexAscending));

            LastDrawingSuccessful = layerDrawings.Count > 0;

            return layerDrawings;
        }

        private void SetUpPixelResolution()
        {
            if (DrawingType == DrawingType.CrossSection) SizeOf1Cm = CanvasSize.Height / Element.Thickness;
            else if (DrawingType == DrawingType.VerticalCut) SizeOf1Cm = CanvasSize.Width / Element.Thickness;
            else SizeOf1Cm = 16.67;
        }
        
        private List<IDrawingGeometry> ReturnStackedGeometries()
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
                if (l.SubConstruction != null)
                {
                    double subConstrWidth = l.SubConstruction.Rectangle.Width;
                    double subConstrHeight = l.SubConstruction.Rectangle.Height;
                    double spacing = l.SubConstruction.Spacing * SizeOf1Cm;

                    // Only Stack if it shows Faces of Cutted Crossections
                    if ((int)DrawingType == (int)l.SubConstruction.Direction)
                    {
                        // Stack Horizontally
                        if (DrawingType == DrawingType.CrossSection)
                        {
                            // returns the largest integer less than or equal to the specified value
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
                                    
                                    // Add padding to the width when overflowing. Creates an extra space of half the width of the SubConstr on each side.
                                    totalSubconstructionsWidth += l.SubConstruction.Width * SizeOf1Cm;
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
                                var subConstrGeometry = l.SubConstruction.Convert(); // Rename to copy?
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
                                    // Add padding to the width when overflowing. Creates an extra space of half the width of the SubConstr on each side.
                                    totalSubconstructionsHeight += l.SubConstruction.Width * SizeOf1Cm;
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
                                var subConstrGeometry = l.SubConstruction.Convert(); // Rename to copy?
                                subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(ptStart.X, y));
                                layerDrawings.Add(subConstrGeometry);
                            }
                        }
                    }
                    else
                    {
                        var subConstrGeometry = l.SubConstruction.Convert(); // Rename to copy?
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
            foreach (var l in Element.Layers)
            {
                // Main Layer Geometry
                if (DrawingType == DrawingType.CrossSection)
                {
                    l.Rectangle = new Rectangle(new Point(0, 0), CanvasSize.Width, l.Thickness * SizeOf1Cm);
                    //l.HatchFitMode = ElementScene.HatchFitMode.OriginalPixelSize;
                }

                else if (DrawingType == DrawingType.VerticalCut)
                {
                    l.Rectangle = new Rectangle(new Point(0, 0), l.Thickness * SizeOf1Cm, CanvasSize.Height);
                    //l.HatchFitMode = ElementScene.HatchFitMode.OriginalPixelSize;
                }

                UpdateLayerGeometry(l);

                if (l.SubConstruction != null)
                {
                    if (DrawingType == DrawingType.CrossSection)
                    {
                        // if it shows Faces of Cutted Vertical/Crosssections
                        if ((int)DrawingType == (int)l.SubConstruction.Direction)
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.SubConstruction.Width * SizeOf1Cm, l.SubConstruction.Thickness * SizeOf1Cm);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 1 : 0.4;
                            l.SubConstruction.BorderPen = Pens.GetSolidPen(Brushes.Black, 0.5);
                        }
                        // if sub construction runs parallel to main layer
                        else
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.Rectangle.Width, l.SubConstruction.Thickness * SizeOf1Cm);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 0.4 : 0.2;
                            l.SubConstruction.BorderPen = Pens.GetDashedPen(Brushes.Black, 1.2);
                        }
                    }
                    else if (DrawingType == DrawingType.VerticalCut)
                    {
                        // if it shows Faces of Cutted Vertical/Crosssections
                        if ((int)DrawingType == (int)l.SubConstruction.Direction)
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.SubConstruction.Thickness * SizeOf1Cm, l.SubConstruction.Width * SizeOf1Cm);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 1 : 0.4;
                            l.SubConstruction.BorderPen = Pens.GetSolidPen(Brushes.Black, 0.5);
                        }
                        // if sub construction runs parallel to main layer
                        else
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.SubConstruction.Thickness * SizeOf1Cm, l.Rectangle.Height);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 0.4 : 0.2;
                            l.SubConstruction.BorderPen = Pens.GetDashedPen(Brushes.Black, 1.2);
                        }
                    }
                    UpdateSubConstructionGeometry(l);
                }
            }
        }

        private void UpdateLayerGeometry(Layer layer)
        {
            layer.ShapeId = new ShapeId(ShapeType.Layer, layer.InternalId);

            layer.BackgroundColor = new SolidColorBrush(layer.Material.Color);
            layer.TextureBrush = Textures.GetBrush(layer.Material.MaterialCategory, layer.Rectangle, 1.0);
            layer.BorderPen = Pens.GetSolidPen(layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black, layer.IsSelected ? 2 : 0.4);
            layer.Opacity = layer.IsEffective ? 1 : 0.3;
            layer.Tag = $"Layer_{layer.LayerNumber}";
            if (layer.Material.MaterialCategory == Enums.MaterialCategory.Insulation) layer.HatchFitMode = HatchFitMode.StretchToFill;
        }

        private void UpdateSubConstructionGeometry(Layer layer)
        {
            if (layer.SubConstruction is null) return;
            var subConstruction = layer.SubConstruction;
            subConstruction.ShapeId = new ShapeId(ShapeType.SubConstructionLayer, layer.InternalId);
            subConstruction.BackgroundColor = new SolidColorBrush(subConstruction.Material.Color);
            subConstruction.TextureBrush = Textures.GetBrush(subConstruction.Material.MaterialCategory, subConstruction.Rectangle, 1.0);
            subConstruction.BorderPen.Brush = subConstruction.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            subConstruction.Tag = $"Layer_{layer.LayerNumber}b";
            //if (subConstruction.IsSelected) subConstruction.Opacity *= 0.7; // Make selected layers slightly more transparent
            if (subConstruction.Material.MaterialCategory == Enums.MaterialCategory.Insulation) subConstruction.HatchFitMode = HatchFitMode.StretchToFill;
        }
    }
}
