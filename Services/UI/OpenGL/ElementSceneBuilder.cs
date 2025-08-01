using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BT.Geometry;
using Rectangle = BT.Geometry.Rectangle;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Converts geometry from CrossSectionBuilder into OpenGL-ready vertex data (rectangles and lines),
    /// and provides vertex streams + texture usage data for the renderer using OglDrawingRepo and IOglSceneBuilder.
    /// </summary>
    public class ElementSceneBuilder : OglDrawingRepo, IOglSceneBuilder
    {
        private readonly CrossSectionBuilder _crossSectionBuilder;
        private readonly int _scenePadding = 1; // Avoids clipping

        public Rectangle SceneBounds => GetSceneBoundaries();
        private double SizeOf1Cm => CrossSectionBuilder.SizeOf1Cm; // Size of 1 cm in OpenGL units, used for scaling dimensions
        
        public ElementSceneBuilder(Element element, DrawingType drawingType = DrawingType.CrossSection)
        {
            _crossSectionBuilder = new CrossSectionBuilder(element, drawingType);
        }

        #region public

        public void BuildScene()
        {
            _crossSectionBuilder.RebuildCrossSection();

            if (!_crossSectionBuilder.DrawingGeometries.Any()) return;
            
            RectVertices.Clear();
            LineVertices.Clear();
            RectBatches.Clear();
            LineBatches.Clear();
            SceneShapes.Clear();

            DebugMode = false;
            if (_crossSectionBuilder.DrawingType == DrawingType.CrossSection)
            {
                DrawFullCrossSectionScene();
            }
            else if (_crossSectionBuilder.DrawingType == DrawingType.VerticalCut)
            {
                DrawFullVerticalCutScene();
            }
        }

        #endregion

        #region private

        private void DrawFullVerticalCutScene()
        {
            var elementBounds = GetContentBounds(_crossSectionBuilder.DrawingGeometries);

            ZIndex = 0;
            AddLine(elementBounds.TopLine, style: LineStyle.Dashed);
            AddLine(elementBounds.BottomLine, style: LineStyle.Dashed);

            foreach (var geom in _crossSectionBuilder.DrawingGeometries)
            {
                ZIndex = -1;
                AddRectangle(geom.Rectangle, geom.BackgroundColor, geom.TextureBrush, geom.HatchFitMode, geom.Opacity);
                SceneShapes.Add(geom);

                // When highlighted
                if (geom.BorderPen.Brush != Brushes.Black)
                {
                    ZIndex = 1;
                    AddLine(geom.Rectangle.LeftLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.RightLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                }
                else
                {
                    ZIndex = 0;
                    AddLine(geom.Rectangle.LeftLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.RightLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                }


                Point markerPos = geom.Rectangle.Center;
                int fontSize = 18;
                string layerNumber = Session.SelectedElement.GetLayerByShapeId(geom.ShapeId).LayerNumber.ToString();
                if (geom.ShapeId.Type == ShapeType.SubConstructionLayer)
                {
                    if (geom.Rectangle.Height < elementBounds.Height)
                    {
                        ZIndex = 0;
                        AddLine(geom.Rectangle.TopLine);
                        AddLine(geom.Rectangle.BottomLine);
                    }
                    layerNumber += "b";
                    fontSize = 16;
                    markerPos += new Vector(0, 2*fontSize); // Adjust position for sub-construction layers
                }
                else if (geom.ShapeId.Type == ShapeType.Layer)
                {
                    DrawSingleDimChain(geom.Rectangle.TopLine,
                        40,
                        Math.Round(geom.Rectangle.Width / SizeOf1Cm, 2).ToString(),
                        geom.BorderPen.Brush,
                        TextAlignment.Bottom | TextAlignment.CenterH,
                        new ShapeId(ShapeType.Layer, geom.InternalId),
                        fontSize: 16);
                }

                ZIndex = 2;
                AddLayerTextMarker(markerPos, layerNumber, fontSize, geom.BorderPen.Brush, geom.Opacity, new ShapeId(ShapeType.Layer, geom.InternalId));
            }
            //DrawSingleDimChain(elementBounds.RightLine,
            //    110,
            //    Math.Round(elementBounds.Height / SizeOf1Cm, 2).ToString(),
            //    alignment: TextAlignment.Bottom | TextAlignment.CenterH, fontSize: 18);

            if (DebugMode)
            {
                AddLine(SceneBounds.TopLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.LeftLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.RightLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.BottomLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.BottomLeft, SceneBounds.TopRight);
                AddLine(SceneBounds.TopLeft, SceneBounds.BottomRight);
                AddText("0", elementBounds.TopLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Top);
                AddText("1", elementBounds.BottomLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Bottom);
                AddText("2", elementBounds.BottomRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Bottom);
                AddText("3", elementBounds.TopRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Top);
                AddText("5", elementBounds.Center, Brushes.Black, 24, alignment: TextAlignment.Center);
            }
        }

        private void DrawFullCrossSectionScene()
        {
            var elementBounds = GetContentBounds(_crossSectionBuilder.DrawingGeometries);

            ZIndex = 0;
            AddLine(elementBounds.LeftLine, style: LineStyle.Dashed);
            AddLine(elementBounds.RightLine, style: LineStyle.Dashed);

            foreach (var geom in _crossSectionBuilder.DrawingGeometries)
            {
                ZIndex = -1;
                AddRectangle(geom.Rectangle, geom.BackgroundColor, geom.TextureBrush, geom.HatchFitMode, geom.Opacity);
                SceneShapes.Add(geom);

                // When highlighted
                if (geom.BorderPen.Brush != Brushes.Black)
                {
                    ZIndex = 1;
                    AddLine(geom.Rectangle.LeftLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.RightLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                }
                else
                {
                    ZIndex = 0;
                    AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                    AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                }


                string layerNumber = Session.SelectedElement.GetLayerByShapeId(geom.ShapeId).LayerNumber.ToString();
                int fontSize = 18;
                if (geom.ShapeId.Type == ShapeType.SubConstructionLayer)
                {
                    ZIndex = 0;
                    AddLine(geom.Rectangle.LeftLine);
                    AddLine(geom.Rectangle.RightLine);
                    layerNumber += "b";
                    fontSize = 16;
                }
                else if (geom.ShapeId.Type == ShapeType.Layer)
                {
                    DrawSingleDimChain(geom.Rectangle.RightLine,
                        40,
                        Math.Round(geom.Rectangle.Height / SizeOf1Cm, 2).ToString(),
                        geom.BorderPen.Brush,
                        TextAlignment.Left | TextAlignment.CenterV,
                        new ShapeId(ShapeType.Layer, geom.InternalId),
                        fontSize: 16);
                }

                ZIndex = 2;
                AddLayerTextMarker(geom.Rectangle.Center, layerNumber, fontSize, geom.BorderPen.Brush, geom.Opacity, new ShapeId(ShapeType.Layer, geom.InternalId));
            }
            DrawSingleDimChain(elementBounds.RightLine,
                110,
                Math.Round(elementBounds.Height / SizeOf1Cm, 2).ToString(),
                alignment: TextAlignment.Left | TextAlignment.CenterV, fontSize: 18);

            if (DebugMode)
            {
                AddLine(SceneBounds.TopLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.LeftLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.RightLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.BottomLine, Brushes.Blue, LineStyle.Dashed);
                AddLine(SceneBounds.BottomLeft, SceneBounds.TopRight);
                AddLine(SceneBounds.TopLeft, SceneBounds.BottomRight);
                AddText("0", elementBounds.TopLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Top);
                AddText("1", elementBounds.BottomLeft, Brushes.Black, 24, alignment: TextAlignment.Left | TextAlignment.Bottom);
                AddText("2", elementBounds.BottomRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Bottom);
                AddText("3", elementBounds.TopRight, Brushes.Black, 24, alignment: TextAlignment.Right | TextAlignment.Top);
                AddText("5", elementBounds.Center, Brushes.Black, 24, alignment: TextAlignment.Center);
            }
        }

        private Rectangle GetContentBounds(IEnumerable<IDrawingGeometry> drawingGeometries, double pad = 0.0)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var g in drawingGeometries)
            {
                var r = g.Rectangle;
                minX = MathF.Min(minX, (float)r.X) - (float)pad;
                minY = MathF.Min(minY, (float)r.Y) - (float)pad;
                maxX = MathF.Max(maxX, (float)r.X + (float)r.Width) + (float)pad;
                maxY = MathF.Max(maxY, (float)r.Y + (float)r.Height) + (float)pad;
            }
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private Rectangle GetSceneBoundaries()
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            // Each rect vertex = 9 floats: x, y, z, r, g, b, a, u, v
            for (int i = 0; i < RectVertices.Count; i += 9)
            {
                float x = RectVertices[i];
                float y = RectVertices[i + 1];

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            // Each line vertex = 10 floats: x, y, z, r, g, b, a, dash, gap, distance
            for (int i = 0; i < LineVertices.Count; i += 10)
            {
                float x = LineVertices[i];
                float y = LineVertices[i + 1];

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            // If no geometry was found
            if (RectVertices.Count == 0 && LineVertices.Count == 0)
                return new Rectangle(0, 0, 0, 0);

            return new Rectangle(
                minX - _scenePadding,
                minY - _scenePadding,
                (maxX - minX) + _scenePadding * 2,
                (maxY - minY) + _scenePadding * 2
            );
        }

        #endregion
    }
}
