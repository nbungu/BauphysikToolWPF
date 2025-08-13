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
        public static double SizeOf1Cm; // starting value

        private Element _element;

        public Element Element
        {
            get => _element;
            set
            {
                _element = value;
                UpdateCanvasSize();
            }
        }
        /// <summary>
        /// Canvas Size in Pixels.
        /// </summary>
        public Size CanvasSize { get; set; }

        private DrawingType _drawingType = DrawingType.CrossSection;
        public DrawingType DrawingType
        {
            get => _drawingType;
            set
            {
                _drawingType = value;
                UpdateCanvasSize();
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

        public CrossSectionBuilder(Element? element, Size canvasSize, DrawingType drawingType, AlignmentVariant variant = AlignmentVariant.EvenSpacingCentered)
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

        private void UpdateCanvasSize()
        {
            if (DrawingType == DrawingType.CrossSection)
                CanvasSize = new Size(960, 480);
            else if (DrawingType == DrawingType.VerticalCut)
                CanvasSize = new Size(480, 960);
            else CanvasSize = new Size(480, 480);
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
            else SizeOf1Cm = 1.0;
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
                layerDrawings.Add(l.CopyGeometry());

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
                                    CanvasSize = new Size(totalSubconstructionsWidth, CanvasSize.Height);
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
                                var subConstrGeometry = l.SubConstruction.CopyGeometry(); // Rename to copy?
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
                                    CanvasSize = new Size(CanvasSize.Width, totalSubconstructionsHeight);
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
                                var subConstrGeometry = l.SubConstruction.CopyGeometry(); // Rename to copy?
                                subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(ptStart.X, y));
                                layerDrawings.Add(subConstrGeometry);
                            }
                        }
                    }
                    else
                    {
                        var subConstrGeometry = l.SubConstruction.CopyGeometry(); // Rename to copy?
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
            foreach (var layer in Element.Layers)
            {
                // Metadata
                layer.ShapeId = new ShapeId(ShapeType.Layer, layer.InternalId);
                layer.Tag = $"Layer_{layer.LayerNumber}";
                // UI
                SetLayerRectangle(layer);
                SetLayerVisuals(layer);

                if (layer.SubConstruction != null)
                {
                    // Metadata
                    layer.SubConstruction.ShapeId = new ShapeId(ShapeType.SubConstructionLayer, layer.InternalId);
                    layer.SubConstruction.Tag = $"Layer_{layer.LayerNumber}b";
                    // UI
                    SetSubConstructionRectangle(layer);
                    SetSubConstructionVisuals(layer);
                }
            }
        }

        private void SetLayerRectangle(Layer layer)
        {
            switch (DrawingType)
            {
                case DrawingType.CrossSection:
                    layer.Rectangle = new Rectangle(
                        new Point(0, 0),
                        CanvasSize.Width,
                        layer.Thickness * SizeOf1Cm
                    );
                    break;

                case DrawingType.VerticalCut:
                    layer.Rectangle = new Rectangle(
                        new Point(0, 0),
                        layer.Thickness * SizeOf1Cm,
                        CanvasSize.Height
                    );
                    break;
            }
        }
        private void SetSubConstructionRectangle(Layer layer)
        {
            var sub = layer.SubConstruction;
            bool isCutFace = (int)DrawingType == (int)sub.Direction;

            switch (DrawingType)
            {
                case DrawingType.CrossSection:
                    sub.Rectangle = isCutFace
                        ? new Rectangle(new Point(0, 0), sub.Width * SizeOf1Cm, sub.Thickness * SizeOf1Cm)
                        : new Rectangle(new Point(0, 0), layer.Rectangle.Width, sub.Thickness * SizeOf1Cm);
                    break;

                case DrawingType.VerticalCut:
                    sub.Rectangle = isCutFace
                        ? new Rectangle(new Point(0, 0), sub.Thickness * SizeOf1Cm, sub.Width * SizeOf1Cm)
                        : new Rectangle(new Point(0, 0), sub.Thickness * SizeOf1Cm, layer.Rectangle.Height);
                    break;
            }
        }
        private void SetSubConstructionVisuals(Layer layer)
        {
            var sub = layer.SubConstruction;
            bool isCutFace = (int)DrawingType == (int)sub.Direction;

            if (isCutFace)
            {
                sub.Opacity = sub.IsEffective ? 1 : 0.4;
                sub.BorderPen = Pens.GetSolidPen(Brushes.Black, 0.5);
            }
            else
            {
                sub.Opacity = sub.IsEffective ? 0.4 : 0.2;
                sub.BorderPen = Pens.GetDashedPen(Brushes.Black, 1.2);
            }
            sub.BackgroundColor = new SolidColorBrush(sub.Material.Color);
            sub.TextureBrush = Textures.GetBrush(sub.Material.MaterialCategory, sub.Rectangle, 1.0);
            sub.BorderPen.Brush = sub.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            if (sub.Material.MaterialCategory == Enums.MaterialCategory.Insulation) sub.HatchFitMode = HatchFitMode.StretchToFill;
        }

        private void SetLayerVisuals(Layer layer)
        {
            layer.BackgroundColor = new SolidColorBrush(layer.Material.Color);
            layer.TextureBrush = Textures.GetBrush(layer.Material.MaterialCategory, layer.Rectangle, 1.0);
            layer.BorderPen = Pens.GetSolidPen(layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black, layer.IsSelected ? 2 : 0.4);
            layer.Opacity = layer.IsEffective ? 1 : 0.3;
            if (layer.Material.MaterialCategory == Enums.MaterialCategory.Insulation) layer.HatchFitMode = HatchFitMode.StretchToFill;
        }
    }
}
