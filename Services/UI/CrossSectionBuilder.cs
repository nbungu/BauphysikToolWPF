using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
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
        //private Rectangle _defaultCrossSectionRectangle = new Rectangle(new Point(0, 0), 880, 400);
        //private Rectangle _defaultVerticalCutRectangle = new Rectangle(new Point(0, 0), 400, 880);
        private bool _isOverflowing;

        // Static, because globally valid for all Instances
        public static double SizeOf1Cm = 16.0; // starting value

        public Element Element { get; set; }
        public Rectangle CanvasSize { get; set; }
        public DrawingType DrawingType { get; set; }
        public AlignmentVariant Alignment { get; set; }
        public List<IDrawingGeometry> DrawingGeometries { get; private set; }
        public bool LastDrawingSuccessful { get; private set; }
        public bool DrawWithLayerLabels { get; set; } = true; // Default to true, can be set to false if labels are not needed

        public CrossSectionBuilder()
        {
            Element = new Element();
            CanvasSize = new Rectangle(new Point(0, 0), 880, 400); // Default size for CrossSection
            DrawingType = DrawingType.CrossSection;
            Alignment = AlignmentVariant.EvenSpacingCentered;
            DrawingGeometries = new List<IDrawingGeometry>();
        }

        public CrossSectionBuilder(Element? element, Rectangle canvasSize, DrawingType drawingType, AlignmentVariant variant = AlignmentVariant.EvenSpacingCentered)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element), "Element cannot be null.");
            CanvasSize = canvasSize;
            DrawingType = drawingType;
            Alignment = variant;
            DrawingGeometries = GetDrawing();
        }

        public void RebuildCrossSection()
        {
            DrawingGeometries = GetDrawing();
            Logger.LogInfo($"Updated cross section drawing of element: {Element}.");
        }

        public void UpdateSingleDrawing(Layer layer)
        {
            if (LastDrawingSuccessful)
            {
                var tag = $"{layer.LayerNumber}";
                var selectedGeometry = DrawingGeometries.FindIndex(g => (string)g.Tag == tag);
                if (selectedGeometry != -1)
                {
                    DrawingGeometries[selectedGeometry] = UpdateLayerGeometry(layer).Convert();
                }
            }
            else RebuildCrossSection();
        }

        // Vertikalschnitt, Canvas in PX
        // Querschnitt, Canvas in PX
        private List<IDrawingGeometry> GetDrawing()
        {
            if (Element is null || !Element.IsValid) return new List<IDrawingGeometry>();

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
                if (DrawWithLayerLabels) layerDrawings.Add(GetLabelForLayer(l, (l.LayerNumber).ToString()));

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
                                var subConstrGeometry = l.SubConstruction.Convert();
                                subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(x, ptStart.Y));
                                layerDrawings.Add(subConstrGeometry);
                                var labelOffset = new Vector(0, 0);
                                if (l.SubConstruction.Direction == Enums.ConstructionDirection.Horizontal) labelOffset = new Vector(24, 0);
                                if (DrawWithLayerLabels) layerDrawings.Add(GetLabelForSubContruction(subConstrGeometry, $"{l.LayerNumber}b", labelOffset));
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
                                var subConstrGeometry = l.SubConstruction.Convert();
                                subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(new Point(ptStart.X, y));
                                layerDrawings.Add(subConstrGeometry);
                                var labelOffset = new Vector(0, 0);
                                if (l.SubConstruction.Direction == Enums.ConstructionDirection.Vertical) labelOffset = new Vector(24, 0);
                                if (DrawWithLayerLabels) layerDrawings.Add(GetLabelForSubContruction(subConstrGeometry, $"{l.LayerNumber}b", labelOffset));
                            }
                        }
                    }
                    else
                    {
                        var subConstrGeometry = l.SubConstruction.Convert();
                        subConstrGeometry.Rectangle = subConstrGeometry.Rectangle.MoveTo(ptStart);
                        layerDrawings.Add(subConstrGeometry);
                        var labelOffset = new Vector(0, 0);
                        if (l.SubConstruction.Direction == Enums.ConstructionDirection.Horizontal) labelOffset = new Vector(24, 0);
                        if (l.SubConstruction.Direction == Enums.ConstructionDirection.Vertical) labelOffset = new Vector(0, 24);
                        if (DrawWithLayerLabels) layerDrawings.Add(GetLabelForSubContruction(subConstrGeometry, $"{l.LayerNumber}b", labelOffset));
                    }
                }
                // Update Origin
                if (DrawingType == DrawingType.CrossSection) ptStart = l.Rectangle.BottomLeft;
                else if (DrawingType == DrawingType.VerticalCut) ptStart = l.Rectangle.TopRight;
            }
            return layerDrawings;
        }

        private IDrawingGeometry GetLabelForLayer(IDrawingGeometry layerGeometry, string labelText)
        {
            var labelGeometry = new DrawingGeometry()
            {
                Rectangle = new Rectangle(
                    new Point(layerGeometry.Rectangle.Left + layerGeometry.Rectangle.Width / 2 - 10, layerGeometry.Rectangle.Top + layerGeometry.Rectangle.Height / 2 - 10),
                    20, // Kreis-Durchmesser 20px
                    20
                ),
                BackgroundColor = Brushes.Transparent, // Hintergrund durchsichtig, da Kreis separat gezeichnet wird
                DrawingBrush = BrushesRepo.CreateCircleWithNumberBrush(labelText, Brushes.White, Brushes.Black, 1, Brushes.Black, new Vector(4, 0)),
                ZIndex = 100,
                Opacity = 1.0,
                Tag = $"Label_{labelText}"
            };
            return labelGeometry;
        }
        private IDrawingGeometry GetLabelForSubContruction(IDrawingGeometry layerGeometry, string labelText, Vector labelOffset = new Vector())
        {
            // Label für SubConstruction hinzufügen (mit "b")
            var subConstrLabel = new DrawingGeometry()
            {
                Rectangle = new Rectangle(
                    new Point(layerGeometry.Rectangle.Left + layerGeometry.Rectangle.Width / 2 - 10 + labelOffset.X,
                        layerGeometry.Rectangle.Top + layerGeometry.Rectangle.Height / 2 - 10 + labelOffset.Y),
                    20, // Kreis-Durchmesser
                    20
                ),
                BackgroundColor = Brushes.Transparent,
                DrawingBrush = BrushesRepo.CreateCircleWithNumberBrush(labelText, Brushes.White, Brushes.Gray, 1, Brushes.Gray, new Vector(8, 0)),
                ZIndex = 100,
                Opacity = 1.0,
                Tag = $"Label_{labelText}"
            };
            return subConstrLabel;
        }

        private void UpdateGeometries()
        {
            foreach (var l in Element.Layers)
            {
                // Main Layer Geometry
                if (DrawingType == DrawingType.CrossSection)
                    l.Rectangle = new Rectangle(new Point(0, 0), CanvasSize.Width, l.Thickness * SizeOf1Cm);
                else if (DrawingType == DrawingType.VerticalCut)
                    l.Rectangle = new Rectangle(new Point(0, 0), l.Thickness * SizeOf1Cm, CanvasSize.Height);

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
                            l.SubConstruction.RectangleStrokeDashArray = new DoubleCollection();
                            l.SubConstruction.RectangleBorderThickness = 0.5;
                        }
                        // if sub construction runs parallel to main layer
                        else
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.Rectangle.Width, l.SubConstruction.Thickness * SizeOf1Cm);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 0.4 : 0.2;
                            l.SubConstruction.RectangleStrokeDashArray = new DoubleCollection(new double[] { 16, 12 });
                            l.SubConstruction.RectangleBorderThickness = 1.2;
                        }
                    }
                    else if (DrawingType == DrawingType.VerticalCut)
                    {
                        // if it shows Faces of Cutted Vertical/Crosssections
                        if ((int)DrawingType == (int)l.SubConstruction.Direction)
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.SubConstruction.Thickness * SizeOf1Cm, l.SubConstruction.Width * SizeOf1Cm);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 1 : 0.4;
                            l.SubConstruction.RectangleStrokeDashArray = new DoubleCollection();
                            l.SubConstruction.RectangleBorderThickness = 0.5;
                        }
                        // if sub construction runs parallel to main layer
                        else
                        {
                            l.SubConstruction.Rectangle = new Rectangle(new Point(0, 0), l.SubConstruction.Thickness * SizeOf1Cm, l.Rectangle.Height);
                            l.SubConstruction.Opacity = l.SubConstruction.IsEffective ? 0.4 : 0.2;
                            l.SubConstruction.RectangleStrokeDashArray = new DoubleCollection(new double[] { 16, 12 });
                            l.SubConstruction.RectangleBorderThickness = 1.2;
                        }
                    }
                    UpdateSubConstructionGeometry(l);
                }
            }
        }

        private IDrawingGeometry UpdateLayerGeometry(Layer layer)
        {
            layer.BackgroundColor = new SolidColorBrush(layer.Material.Color);
            layer.DrawingBrush = BrushesRepo.GetHatchPattern(layer.Material.MaterialCategory, 0.5, layer.Rectangle);
            layer.RectangleBorderColor = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            layer.RectangleBorderThickness = layer.IsSelected ? 2 : 0.2;
            layer.Opacity = layer.IsEffective ? 1 : 0.3;
            layer.Tag = $"Layer_{layer.LayerNumber}";
            return layer;
        }

        private IDrawingGeometry UpdateSubConstructionGeometry(Layer layer)
        {
            if (layer.SubConstruction is null) return new DrawingGeometry();

            var subConstruction = layer.SubConstruction;
            subConstruction.BackgroundColor = new SolidColorBrush(subConstruction.Material.Color);
            subConstruction.DrawingBrush = BrushesRepo.GetHatchPattern(subConstruction.Material.MaterialCategory, 0.5, subConstruction.Rectangle);
            subConstruction.RectangleBorderColor = subConstruction.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            subConstruction.Tag = $"Layer_{layer.LayerNumber}b";
            return subConstruction;
        }
    }
}
