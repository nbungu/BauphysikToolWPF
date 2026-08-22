using BauphysikToolWPF.Models.Domain;
using SharedLibrary.Models.Geometry;
using SharedLibrary.OpenGL;
using SharedLibrary.Services.Application;
using System;
using System.Linq;
using System.Windows.Media;

namespace BauphysikToolWPF.Services.UI
{
    /// <summary>
    /// Converts geometry from CrossSectionBuilder into OpenGL-ready vertex data (rectangles and lines),
    /// and provides vertex streams + texture usage data for the renderer using OglDrawingRepo and IOglSceneBuilder.
    /// </summary>
    public class SceneBuilder : OglDrawingRepo, IOglSceneBuilder
    {
        private readonly int _scenePadding = 1; // Avoids clipping
        private readonly Element _element;


        public Rectangle SceneBounds => SceneShapes.GetContentBounds();
        public bool ShowSceneDecoration { get; set; } = true;
        public bool IsValid => RenderCommands.Any();
        public double SizeOf1Cm { get; private set; }
        public Size CanvasSize { get; set; }
        public DrawingType DrawingType { get; set; } = DrawingType.CrossSection;

        public SceneBuilder(Element element)
        {
            _element = element;
        }

        #region public

        public void BuildScene()
        {
            RenderCommands.Clear();
            SceneShapes.Clear();
           
            if (DrawingType == DrawingType.CrossSection)
            {
                try
                {
                    DrawFullCrossSectionScene();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error while drawing full cross-section scene: {ex}");
                }
            }
            else if (DrawingType == DrawingType.VerticalCut)
            {
                try
                {
                    DrawFullVerticalCutScene();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error while drawing full vertical cut scene: {ex}");
                }
            }

            if (DebugMode)
            {
                var elementBounds = SceneShapes.GetContentBounds();

                AddLine(SceneBounds.TopLine, Brushes.Blue, LineStyle.Dashed, zIndex: 32);
                AddLine(SceneBounds.LeftLine, Brushes.Blue, LineStyle.Dashed, zIndex: 32);
                AddLine(SceneBounds.RightLine, Brushes.Blue, LineStyle.Dashed, zIndex: 32);
                AddLine(SceneBounds.BottomLine, Brushes.Blue, LineStyle.Dashed, zIndex: 32);

                AddLine(SceneBounds.TopLeft, SceneBounds.BottomRight, Brushes.Blue, LineStyle.Dashed, zIndex: -32);
                AddLine(SceneBounds.BottomLeft, SceneBounds.TopRight, Brushes.Blue, LineStyle.Dashed, zIndex: -32);

                AddText("0", elementBounds.TopLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Top);
                AddText("1", elementBounds.BottomLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Bottom);
                AddText("2", elementBounds.BottomRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Bottom);
                AddText("3", elementBounds.TopRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Top);
                AddText("5", elementBounds.Center, Brushes.Black, 24, alignment: TextAlignment.Center);
            }
        }

        #endregion

        #region private

        private void DrawFullCrossSectionScene()
        {
            SetCanvasSize();

            SetUpPixelResolution();

            // Stacking
            double scale = SizeOf1Cm;
            foreach (var l in _element.Layers)
            {
                double x = 0;
                double y = 0;
                var shp = new ShapeId(ShapeType.Layer, l.InternalId);
                var rect = new Rectangle(new Point(x, y), CanvasSize.Width, l.Thickness * scale);
                AddPolygon(
                    points: rect.Points,
                    fillBrush: new SolidColorBrush(l.Material.Color),
                    borderPen: Pens.GetSolidPen(l.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black, l.IsSelected ? 2 : 0.4),
                    opacity: l.IsEffective ? 1 : 0.3,
                    zIndex: 0,
                    shp: shp);

                //// SubConstruction
                //if (l.SubConstruction != null)
                //{
                //    double subConstrWidth = l.SubConstruction.Rectangle.Width;
                //    double subConstrHeight = l.SubConstruction.Rectangle.Height;
                //    double spacing = l.SubConstruction.Spacing * scale;
                //}

            }
        }

        private void DrawFullVerticalCutScene()
        {
            SetCanvasSize();

            SetUpPixelResolution();

            // Stacking

        }

        private void SetUpPixelResolution()
        {
            switch (DrawingType)
            {
                case DrawingType.FrontView:
                case DrawingType.TopView:
                case DrawingType.VerticalCut:
                    SizeOf1Cm = CanvasSize.Width / _element.Thickness;
                    break;
                case DrawingType.CrossSection:
                    SizeOf1Cm = CanvasSize.Height / _element.Thickness;
                    break;
                default:
                    SizeOf1Cm = 1.0;
                    break;
            }
        }
        private void SetCanvasSize()
        {
            switch (DrawingType)
            {
                case DrawingType.FrontView:
                case DrawingType.TopView:
                case DrawingType.VerticalCut:
                    CanvasSize = new Size(480, 960);
                    break;
                case DrawingType.CrossSection:
                    CanvasSize = new Size(960, 480);
                    break;
                default:
                    CanvasSize = new Size(480, 480);
                    break;
            }
        }

        //private void DrawFullVerticalCutScene()
        //{
        //    Logger.LogInfo("Start drawing scene: Vertical-Cut");
        //    //var fontSizePx = 28;
        //    var elementBounds = GetContentBounds(CrossSectionBuilder.DrawingGeometries);

        //    ZIndex = 0;
        //    AddLine(elementBounds.TopLine, style: LineStyle.Dashed);
        //    AddLine(elementBounds.BottomLine, style: LineStyle.Dashed);

        //    foreach (var geom in CrossSectionBuilder.DrawingGeometries)
        //    {
        //        ZIndex = -1;
        //        AddRectangle(geom.Rectangle, geom.BackgroundColor, geom.TextureBrush, geom.HatchFitMode, geom.Opacity, geom.ShapeId);

        //        // When highlighted
        //        if (geom.BorderPen.Brush != Brushes.Black)
        //        {
        //            ZIndex = 1;
        //            AddLine(geom.Rectangle.LeftLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.RightLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //        }
        //        else
        //        {
        //            ZIndex = 0;
        //            AddLine(geom.Rectangle.LeftLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.RightLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //        }

        //        Point markerPos = geom.Rectangle.Center;
        //        string layerNumber = CrossSectionBuilder.Element?.GetLayerByShapeId(geom.ShapeId)?.LayerNumber.ToString() ?? "";
        //        if (geom.ShapeId.Type == ShapeType.SubConstructionLayer)
        //        {
        //            if (geom.Rectangle.Height < elementBounds.Height)
        //            {
        //                ZIndex = 0;
        //                AddLine(geom.Rectangle.TopLine);
        //                AddLine(geom.Rectangle.BottomLine);
        //            }
        //            layerNumber += "b";
        //            markerPos += new Vector(0, 32*WorldUnitsPerPixel); // Adjust position for sub-construction layers
        //        }
        //        else if (geom.ShapeId.Type == ShapeType.Layer)
        //        {
        //            //DrawSingleDimChain(geom.Rectangle.TopLine,
        //            //    40,
        //            //    Math.Round(geom.Rectangle.Width / SizeOf1Cm, 2).ToString(),
        //            //    geom.BorderPen.Brush,
        //            //    TextAlignment.Bottom | TextAlignment.CenterH,
        //            //    fontSize: 16,
        //            //    new ShapeId(ShapeType.Layer, geom.InternalId));
        //        }

        //        ZIndex = 2;
        //        if (ShowSceneDecoration) AddLayerTextMarker(markerPos, layerNumber, geom.BorderPen.Brush, 32, opacity: geom.Opacity, shp: new ShapeId(ShapeType.Annotation, geom.InternalId));
        //    }

        //    if (ShowSceneDecoration)
        //    {
        //        // TESTING:
        //        var relevantGeometries = CrossSectionBuilder.DrawingGeometries.Where(geom => geom.ShapeId.Type == ShapeType.Layer);
        //        double[] xIncrements = relevantGeometries.Select(geom => geom.Rectangle.Width).ToArray();

        //        var matchingLayers = CrossSectionBuilder.Element.Layers
        //            .Where(layer => relevantGeometries.Any(dg => dg.ShapeId.Type == layer.ShapeId.Type && dg.ShapeId.Index == layer.ShapeId.Index))
        //            .ToList();
        //        double[] layerWidths = matchingLayers.Select(layer => layer.Thickness).ToArray();

        //        AddDimensionalChainsHorizontal(elementBounds.TopLeft, xIncrements, layerWidths, 50, fontSizePx: 32);
        //        AddDimensionalChainsHorizontal(elementBounds.TopLeft, new[] { elementBounds.Width }, 110, oneCmConversion: SizeOf1Cm, fontSizePx: 32);
        //    }

        //    if (DebugMode)
        //    {
        //        AddLine(SceneBounds.TopLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.LeftLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.RightLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.BottomLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.BottomLeft, SceneBounds.TopRight);
        //        AddLine(SceneBounds.TopLeft, SceneBounds.BottomRight);
        //        AddText("0", elementBounds.TopLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Top);
        //        AddText("1", elementBounds.BottomLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Bottom);
        //        AddText("2", elementBounds.BottomRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Bottom);
        //        AddText("3", elementBounds.TopRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Top);
        //        AddText("5", elementBounds.Center, Brushes.Black, 24, alignment: TextAlignment.Center);
        //    }
        //}

        //private void DrawFullCrossSectionScene()
        //{
        //    Logger.LogInfo("Start drawing scene: Cross-Section");

        //    var fontSizePx = 32;
        //    var elementBounds = GetContentBounds(CrossSectionBuilder.DrawingGeometries);
        //    int dimChainOffset = (int)SizeOf1Cm * 3;

        //    ZIndex = 0;
        //    AddLine(elementBounds.LeftLine, style: LineStyle.Dashed);
        //    AddLine(elementBounds.RightLine, style: LineStyle.Dashed);

        //    foreach (var geom in CrossSectionBuilder.DrawingGeometries)
        //    {
        //        string layerNumber = CrossSectionBuilder.Element?.GetLayerByShapeId(geom.ShapeId)?.LayerNumber.ToString() ?? "";

        //        // Rectangle
        //        if (geom.ShapeId.Type == ShapeType.Layer)
        //        {
        //            ZIndex = -2;
        //            AddRectangle(geom.Rectangle, geom.BackgroundColor, geom.TextureBrush, geom.HatchFitMode, geom.Opacity, geom.ShapeId);
        //        }
        //        else if (geom.ShapeId.Type == ShapeType.SubConstructionLayer)
        //        {
        //            layerNumber += "b";

        //            ZIndex = -1;
        //            AddRectangle(geom.Rectangle, geom.BackgroundColor, geom.TextureBrush, geom.HatchFitMode, geom.Opacity, geom.ShapeId);
                    
        //            ZIndex = 0;
        //            AddLine(geom.Rectangle.LeftLine);
        //            AddLine(geom.Rectangle.RightLine);
        //        }

        //        // Layer Marker
        //        ZIndex = 2;
        //        if (ShowSceneDecoration) AddLayerTextMarker(geom.Rectangle.Center, layerNumber, geom.BorderPen.Brush, fontSizePx, opacity: geom.Opacity, shp: geom.ShapeId);

        //        // Rectangle Borders
        //        if (geom.BorderPen.Brush != Brushes.Black)
        //        {
        //            ZIndex = 1;
        //            AddLine(geom.Rectangle.LeftLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.RightLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //        }
        //        else
        //        {
        //            ZIndex = 0;
        //            AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //            AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
        //        }
        //    }

        //    if (ShowSceneDecoration)
        //    {
        //        double[] intervals;

        //        // Vertical full element
        //        DrawSingleDimChain(elementBounds.RightLine, CrossSectionBuilder.DrawingGeometries.Count > 1 ? dimChainOffset * 3 : dimChainOffset,
        //            NumberConverter.ConvertToString(elementBounds.Height / SizeOf1Cm),
        //            alignment: TextAlignment.Left | TextAlignment.CenterV, fontSizePx: fontSizePx);

        //        // Vertical Layers
        //        if (CrossSectionBuilder.DrawingGeometries.Count > 1)
        //        {
        //            intervals = MeasurementDrawing.GetMeasurementChainIntervals(CrossSectionBuilder.DrawingGeometries.Where(g => g.ShapeId.Type == ShapeType.Layer), Axis.Z, false);
        //            AddDimensionalChainsVertical(elementBounds.Right, intervals, dimChainOffset, false, SizeOf1Cm, fontSizePx);
        //        }

        //        // Sub Constructions
        //        if (CrossSectionBuilder.DrawingGeometries.Any(g => g.ShapeId.Type == ShapeType.SubConstructionLayer))
        //        {
        //            // Vertical Sub Constructions
        //            intervals = MeasurementDrawing.GetMeasurementChainIntervals(CrossSectionBuilder.DrawingGeometries, Axis.Z, true);
        //            AddDimensionalChainsVertical(elementBounds.Left, intervals, dimChainOffset, true, SizeOf1Cm, fontSizePx);

        //            // Horizontal Sub Constructions
        //            intervals = MeasurementDrawing.GetMeasurementChainIntervals(CrossSectionBuilder.DrawingGeometries, Axis.X, true);
        //            AddDimensionalChainsHorizontal(elementBounds.Top, intervals, dimChainOffset, false, SizeOf1Cm, fontSizePx);
        //        }
        //    }

        //    if (DebugMode)
        //    {
        //        AddLine(SceneBounds.TopLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.LeftLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.RightLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.BottomLine, Brushes.Blue, LineStyle.Dashed);
        //        AddLine(SceneBounds.BottomLeft, SceneBounds.TopRight);
        //        AddLine(SceneBounds.TopLeft, SceneBounds.BottomRight);
        //        AddText("0", elementBounds.TopLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Top);
        //        AddText("1", elementBounds.BottomLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Bottom);
        //        AddText("2", elementBounds.BottomRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Bottom);
        //        AddText("3", elementBounds.TopRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Top);
        //        AddText("5", elementBounds.Center, Brushes.Black, 24, alignment: TextAlignment.Center);
        //    }
        //}

        #endregion
    }
}
