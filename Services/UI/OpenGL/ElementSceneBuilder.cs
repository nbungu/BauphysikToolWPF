using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Line = BT.Geometry.Line;
using Pen = System.Windows.Media.Pen;
using Point = BT.Geometry.Point;
using Rectangle = BT.Geometry.Rectangle;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Converts geometry from CrossSectionBuilder into OpenGL-ready vertex data (rectangles and lines),
    /// and provides vertex streams + texture usage data for the renderer.
    /// </summary>
    public class ElementSceneBuilder : IOglSceneBuilder
    {
        private readonly OglController _parent;
        private readonly CrossSectionBuilder _crossSectionBuilder;
        private int _zIndex; // Z-level for rendering order, can be used to layer elements in the scene

        public bool DebugMode { get; set; } = false; // Enable debug mode for additional rendering features
        public bool IsTextSizeZoomable { get; set; } = false;
        private TextureManager TextureManager => _parent.TextureManager;
        private SdfFont? SdfFont => TextureManager.SdfFont;
        private double SizeOf1Cm => CrossSectionBuilder.SizeOf1Cm; // Size of 1 cm in OpenGL units, used for scaling dimensions

        private float ZoomFactor => _parent.ZoomFactor; // Current zoom factor from the OglController

        public List<IDrawingGeometry> SceneShapes { get; } = new();
        public int ScenePadding { get; set; } = 0;
        public Rectangle SceneBounds => GetSceneBoundaries();
        public List<float> RectVertices { get; } = new();
        public List<float> LineVertices { get; } = new();
        public List<(int? TextureId, int Count)> RectBatches { get; } = new();
        public List<(float LineWidth, int Count)> LineBatches { get; } = new();

        public ElementSceneBuilder(OglController parent)
        {
            _parent = parent;
            _crossSectionBuilder = new CrossSectionBuilder()
            {
                Element = Session.SelectedElement,
            };
        }

        #region public

        public void BuildScene()
        {
            _crossSectionBuilder.DrawingType = DrawingType.CrossSection;
            _crossSectionBuilder.RebuildCrossSection();

            if (!_crossSectionBuilder.DrawingGeometries.Any()) return;
            
            RectVertices.Clear();
            LineVertices.Clear();
            RectBatches.Clear();
            LineBatches.Clear();
            SceneShapes.Clear();

            var elementBounds = GetContentBounds(_crossSectionBuilder.DrawingGeometries);
            AddLine(elementBounds.LeftLine, style: LineStyle.Dashed);
            AddLine(elementBounds.RightLine, style: LineStyle.Dashed);

            foreach (var geom in _crossSectionBuilder.DrawingGeometries)
            {
                _zIndex = 0;
                AddLine(geom.Rectangle.TopLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);
                if (geom.TextureBrush is DrawingBrush hatch)
                {
                    AddTexturedRectangle(geom.Rectangle, geom.BackgroundColor, hatch, geom.HatchFitMode, geom.Opacity);
                    SceneShapes.Add(geom);
                }
                else
                {
                    AddRectangle(geom.Rectangle, geom.BackgroundColor, geom.Opacity);
                    SceneShapes.Add(geom);
                }
                AddLine(geom.Rectangle.BottomLine, geom.BorderPen.Brush, LineStyle.Solid, geom.BorderPen.Thickness);

                _zIndex = 1;
                //AddLine(geom.Rectangle.BottomLine, Brushes.Black);
                //if (geom.ShapeId.Index == 0) AddLine(geom.Rectangle.TopLine, Brushes.Black);

                string layerNumber = "";
                int fontSize = 18;
                if (geom.ShapeId.Type == ShapeType.SubConstructionLayer)
                {
                    AddLine(geom.Rectangle.LeftLine);
                    AddLine(geom.Rectangle.RightLine);
                    layerNumber = Session.SelectedElement.GetLayerByShapeId(geom.ShapeId).LayerNumber + "b";
                    fontSize = 16;
                }
                else if (geom.ShapeId.Type == ShapeType.Layer)
                {
                    layerNumber = Session.SelectedElement.GetLayerByShapeId(geom.ShapeId).LayerNumber.ToString();

                    DrawSingleDimChain(geom.Rectangle.RightLine,
                        40,
                        Math.Round(geom.Rectangle.Height / SizeOf1Cm, 2).ToString(),
                        geom.BorderPen.Brush,
                        TextAlignment.Left | TextAlignment.CenterV,
                        new ShapeId(ShapeType.Layer, geom.InternalId),
                        fontSize: 16);
                }

                _zIndex = 2;
                AddLayerTextMarker(geom.Rectangle.Center, layerNumber, fontSize, geom.Opacity, new ShapeId(ShapeType.Layer, geom.InternalId));
                
                //AddCircle(geom.Rectangle.Center, 12 * (1/ZoomFactor), Brushes.White, Brushes.Black);
                //// Make the circles behave like if a layer is clicked
                //SceneShapes.Add(new DrawingGeometry(new ShapeId(ShapeType.Layer, geom.InternalId), Rectangle.FromCircle(geom.Rectangle.Center, 12), _zIndex));

                //_zIndex = 3;
                //AddText(layerNumber, geom.Rectangle.Center, Brushes.Black, fontSize, 1.0, TextAlignment.Center);
            }
            DrawSingleDimChain(elementBounds.RightLine,
                120, 
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

        private void AddLayerTextMarker(Point pt, string text, int fontSize, double opacity = 1.0, ShapeId? shp = null)
        {
            var charLength = text.Length;
            var zoomFactor = IsTextSizeZoomable ? (1 / ZoomFactor) : 1.0;
            AddCircle(pt, (fontSize / 2 + 2*charLength) * zoomFactor, Brushes.White, Brushes.Black, opacity: opacity);
            // Make the circles behave like if a layer is clicked
            if (shp is ShapeId id) SceneShapes.Add(new DrawingGeometry(id, Rectangle.FromCircle(pt, (fontSize / 2 + 2 * charLength) * zoomFactor), _zIndex));
           
            _zIndex++;
            AddText(text, pt, Brushes.Black, fontSize, 1.0, TextAlignment.Center);
        }

        //public void UpdateShapeOpacity(IDrawingGeometry shape, float newOpacity)
        //{
        //    const int vertexStride = 9;
        //    int baseIndex = shape.VertexStartIndex;

        //    if (baseIndex < 0 || baseIndex + 6 * vertexStride > RectVertices.Count)
        //        return; // safety check

        //    for (int i = 0; i < 6; i++)
        //    {
        //        int alphaIndex = baseIndex + i * vertexStride + 6; // index of `a`
        //        RectVertices[alphaIndex] = newOpacity;
        //    }
        //    shape.Opacity = newOpacity;
        //}

        #endregion

        #region private methods

        private int AddRectangle(Rectangle rect, Brush bgColor, double opacity = 1.0)
        {
            if (bgColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                bgColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 color = bgColor.ToVectorColor();
            int? texId = null;
            var verts = CreateRectVertices(rect, color, _zIndex);
            var vertexStartIndex = RectVertices.Count;
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
            return vertexStartIndex;
        }

        private void AddTexturedRectangle(Rectangle rect, Brush bgColor, DrawingBrush hatch, HatchFitMode mode, double opacity = 1.0)
        {
            // Extract color and apply opacity (in alpha channel)
            if (bgColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                bgColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 color = bgColor.ToVectorColor();
            var texId = TextureManager.GetTextureIdForBrush(hatch);
            var texSize = texId.HasValue ? TextureManager.GetTextureSize(texId.Value) : null;

            float texRepeatX = 1f, texRepeatY = 1f;

            if (texSize.HasValue)
            {
                float w = (float)rect.Width;
                float h = (float)rect.Height;
                float texW = texSize.Value.Width;
                float texH = texSize.Value.Height;

                double aspect = texW / (double)texH;
                switch (mode)
                {
                    case HatchFitMode.FitToWidth:
                        texRepeatX = 1f;
                        texRepeatY = (float)(h / (w / aspect));
                        break;
                    case HatchFitMode.FitToHeight:
                        texRepeatY = 1f;
                        texRepeatX = (float)(w / (h * aspect));
                        break;
                    case HatchFitMode.StretchToFill:
                        texRepeatX = texRepeatY = 1f;
                        break;
                    default:
                        texRepeatX = w / texW;
                        texRepeatY = h / texH;
                        break;
                }
            }

            var verts = CreateRectVertices(rect, color, texRepeatX, texRepeatY, _zIndex);
            var vertexStartIndex = RectVertices.Count;
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
        }
        private void AddRectangle(IDrawingGeometry geom)
        {
            // Extract color and apply opacity (in alpha channel)
            var bgColor = geom.BackgroundColor;
            if (bgColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(geom.Opacity * 255);
                bgColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 color = bgColor.ToVectorColor();
            
            int? texId = null;
            float texRepeatX = 0f, texRepeatY = 0f;
            if (geom.TextureBrush is DrawingBrush drawingBrush)
            {
                texId = TextureManager.GetTextureIdForBrush(drawingBrush);
                var texSize = texId.HasValue ? TextureManager.GetTextureSize(texId.Value) : null;

                texRepeatX = 1f;
                texRepeatY = 1f;

                if (texSize.HasValue)
                {
                    float w = (float)geom.Rectangle.Width;
                    float h = (float)geom.Rectangle.Height;
                    float texW = texSize.Value.Width;
                    float texH = texSize.Value.Height;

                    double aspect = texW / (double)texH;
                    switch (geom.HatchFitMode)
                    {
                        case HatchFitMode.FitToWidth:
                            texRepeatX = 1f;
                            texRepeatY = (float)(h / (w / aspect));
                            break;
                        case HatchFitMode.FitToHeight:
                            texRepeatY = 1f;
                            texRepeatX = (float)(w / (h * aspect));
                            break;
                        case HatchFitMode.StretchToFill:
                            texRepeatX = texRepeatY = 1f;
                            break;
                        default:
                            texRepeatX = w / texW;
                            texRepeatY = h / texH;
                            break;
                    }
                }
            }
            var verts = CreateRectVertices(geom.Rectangle, color, texRepeatX, texRepeatY, _zIndex);
            geom.VertexStartIndex = RectVertices.Count; // Store vertex start index in the geometry
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
        }

        private void AddLine(Line line, Brush? lineColor = null, LineStyle style = LineStyle.Solid, double thickness = 1.0, double opacity = 1.0)
        {
            lineColor ??= Brushes.Black;
            // Extract color and apply opacity (in alpha channel)
            if (lineColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                lineColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 col = lineColor.ToVectorColor();
            float dash = 0f, gap = 0f;
            switch (style)
            {
                case LineStyle.Dashed: dash = 8f; gap = 4f; break;
                case LineStyle.Dotted: dash = 2f; gap = 4f; break;
            }

            var verts = CreateLineVertices(line, col, dash, gap, _zIndex);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)thickness, 2));
        }
        
        private void AddLine(Point ptStart, Point ptEnd, Brush? lineColor = null, LineStyle style = LineStyle.Solid, double thickness = 1.0, double opacity = 1.0)
        {
            var line = new Line(ptStart, ptEnd);
            lineColor ??= Brushes.Black;
            // Extract color and apply opacity (in alpha channel)
            if (lineColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                lineColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 col = lineColor.ToVectorColor();
            float dash = 0f, gap = 0f;
            switch (style)
            {
                case LineStyle.Dashed: dash = 8f; gap = 4f; break;
                case LineStyle.Dotted: dash = 2f; gap = 4f; break;
            }

            var verts = CreateLineVertices(line, col, dash, gap, _zIndex);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)thickness, 2));
        }

        private void AddLine(Line line, Pen pen)
        {
            if (pen.Brush == null) return;

            // Extract colo.
            Vector4 color = pen.Brush.ToVectorColor();

            // Determine dash pattern
            float dashLength = 0f, gapLength = 0f;

            if (pen.DashStyle != null && pen.DashStyle.Dashes.Count > 0)
            {
                var dashes = pen.DashStyle.Dashes;

                // Basic support for common dash/dot styles
                if (dashes.Count == 1)
                {
                    dashLength = (float)dashes[0];
                    gapLength = dashLength; // assume equal gap
                }
                else if (dashes.Count >= 2)
                {
                    dashLength = (float)dashes[0];
                    gapLength = (float)dashes[1];
                }
            }

            var verts = CreateLineVertices(line, color, dashLength, gapLength, _zIndex);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)pen.Thickness, 2));
        }

        //public void AddDimensionalChainsHorizontal(Point ptStart, double[] xIncrements, double distance, bool drawAbove)
        //{
        //    if (xIncrements is null || xIncrements.Length < 1) return;
            
        //    var xCoords = new List<double> { ptStart.X };
        //    for (int i = 0; i < xIncrements.Length; i++)
        //    {
        //        double lastX = xCoords[^1];
        //        double nextX = lastX + xIncrements[i];
        //        xCoords.Add(nextX);
        //    }
        //    if (drawAbove) xCoords.Reverse();
        //    for (int i = 0; i < xCoords.Count - 1; i++)
        //    {
        //        double startX = xCoords[i];
        //        double endX = xCoords[i + 1];
        //        var startPt = new Point(startX, ptStart.Y);
        //        var endPt = new Point(endX, ptStart.Y);

        //        var text = (new Line(startPt, endPt).Length * CrossSectionBuilder.SizeOf1Cm).ToString();
        //        DrawSingleDimChain(startPt,
        //            endPt,
        //            distance,
        //            text,
        //            alignment: TextAlignment.Bottom | TextAlignment.CenterH);
        //    }
        //}
        //public void AddDimensionalChainsVertical(Point ptStart, double[] yIncrements, double distance, bool drawRight)
        //{
        //    if (yIncrements is null || yIncrements.Length < 1) return;

        //    var yCoords = new List<double> { ptStart.Y };
        //    for (int i = 0; i < yIncrements.Length; i++)
        //    {
        //        double lastY = yCoords[^1];
        //        double nextY = lastY + yIncrements[i];
        //        yCoords.Add(nextY);
        //    }

        //    if (drawRight) yCoords.Reverse();

        //    for (int i = 0; i < yCoords.Count - 1; i++)
        //    {
        //        double startY = yCoords[i];
        //        double endY = yCoords[i + 1];
        //        var startPt = new Point(ptStart.X, startY);
        //        var endPt = new Point(ptStart.X, endY);
        //        var text = (new Line(startPt, endPt).Length * CrossSectionBuilder.SizeOf1Cm).ToString();
        //        DrawSingleDimChain(
        //            startPt,
        //            endPt,
        //            distance,
        //            text,
        //            alignment: TextAlignment.CenterV | (drawRight ? TextAlignment.Left : TextAlignment.Right)
        //        );
        //    }
        //}

        private void DrawSingleDimChain(Line line, double distance, string displayedValue, Brush? lineColor = null, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, ShapeId? shp = null, int fontSize = 24)
        {
            DrawSingleDimChain(line.Start, line.End, distance, displayedValue, lineColor, alignment, shp, fontSize);
        }

        private void DrawSingleDimChain(Point ptStart, Point ptEnd, double distance, string displayedValue, Brush? lineColor = null, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, ShapeId? shp = null, int fontSize = 24)
        {
            lineColor ??= Brushes.Black;
            int tickSize = 8;
            var line = new Line(ptStart, ptEnd);

            // Direction vector from start to end
            var vect = ptEnd - ptStart;
            var length = vect.Length;
            if (length == 0) return;

            // Normalize direction and get perpendicular (normal) vector
            var vectNormalized = vect.Normalize();
            var nVect = vectNormalized.Normal();
            
            // Offset the dimension line by the normal * distance
            var offsetLine = new Line(line, distance);
            
            // Draw the main dimension line
            AddLine(offsetLine, lineColor);

            // Perpendicular ticks at both ends (along the normal direction)
            var tickOffset = nVect * tickSize;

            AddLine(offsetLine.Start - tickOffset, offsetLine.Start + tickOffset, lineColor);
            AddLine(offsetLine.End - tickOffset, offsetLine.End + tickOffset, lineColor);

            // 45° diagonal cross ticks (offset ±45° between normal and direction)
            var diagOffset = (vectNormalized - nVect).Normalize() * tickSize;

            AddLine(offsetLine.Start - diagOffset, offsetLine.Start + diagOffset, lineColor);
            AddLine(offsetLine.End - diagOffset, offsetLine.End + diagOffset, lineColor);

            var pos = offsetLine.GetCenter() - tickOffset * 2;
            AddText(displayedValue, pos, lineColor, fontSize: fontSize, alignment: alignment, shp: shp);
        }

        private void AddCircle(Point center, double radius, Brush fillColor, Brush? outlineColor = null, int outlineWidth = 1, int segments = 32, double opacity = 1.0)
        {
            // Apply opacity to fill color
            if (fillColor is SolidColorBrush solidFill)
            {
                var c = solidFill.Color;
                byte alpha = (byte)(opacity * 255);
                fillColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 fill = fillColor.ToVectorColor();
            var circleVerts = CreateCircleVertices(center, radius, fill, segments, _zIndex);

            RectVertices.AddRange(circleVerts);
            RectBatches.Add((null, segments * 3));

            // Optional outline
            if (outlineColor != null)
            {
                Vector4 col = outlineColor.ToVectorColor();
                var outlineVerts = new List<float>();
                float dash = 0f, gap = 0f;

                float cx = (float)center.X;
                float cy = (float)center.Y;

                for (int i = 0; i < segments; i++)
                {
                    float angle1 = MathF.PI * 2f * i / segments;
                    float angle2 = MathF.PI * 2f * (i + 1) / segments;

                    float x1 = cx + MathF.Cos(angle1) * (float)radius;
                    float y1 = cy + MathF.Sin(angle1) * (float)radius;
                    float x2 = cx + MathF.Cos(angle2) * (float)radius;
                    float y2 = cy + MathF.Sin(angle2) * (float)radius;

                    var verts = CreateLineVertices(new Line(x1, y1, x2, y2), col, dash, gap, _zIndex);
                    outlineVerts.AddRange(verts);
                }

                LineVertices.AddRange(outlineVerts);
                LineBatches.Add((outlineWidth, segments * 2));
            }
        }

        /// <summary>
        /// rasterized texture-based text
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="fontSize"></param>
        /// <param name="opacity"></param>
        private void AddTextRasterized(Point position, string text, Brush color, double fontSize = 16, double opacity = 1.0, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Render text to bitmap
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                fontSize,
                color,
                VisualTreeHelper.GetDpi(new System.Windows.Controls.Control()).PixelsPerDip
            );

            // Render to bitmap
            int width = (int)Math.Ceiling(formattedText.Width);
            int height = (int)Math.Ceiling(formattedText.Height);
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                var c = ((SolidColorBrush)color).Color;
                var alpha = (byte)(opacity * 255);
                var solid = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
                dc.DrawText(new FormattedText(
                    text,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    solid,
                    1.0
                ), new System.Windows.Point(0, 0));
            }

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            // Upload to OpenGL texture
            int texId = TextureManager.CreateTextureFromBitmap(bmp, isText: true);
            if (texId == -1) return;

            var offset = GetAlignedOffset(alignment, width, height);
            var alignedPosition = new Point(position.X + offset.X, position.Y + offset.Y);

            // Create rectangle for text quad
            var rect = new Rectangle(alignedPosition.X, alignedPosition.Y, width, height);
            Vector4 white = new(1f, 1f, 1f, 1f); // vertex color = white, since texture has pre-colored glyphs
            var verts = CreateRectVertices(rect, white, 1f, 1f, _zIndex);
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
        }

        /// <summary>
        /// polyline-only vector outlines
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="brush"></param>
        /// <param name="fontSize"></param>
        /// <param name="opacity"></param>
        private void AddTextVectorized(string text, Point position, Brush brush, double fontSize = 16, double opacity = 1.0)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (brush is not SolidColorBrush solidBrush)
                return;


            var c = solidBrush.Color;
            var alpha = (byte)(opacity * 255);
            var solidColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            var formatted = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                fontSize,
                solidColor,
                VisualTreeHelper.GetDpi(new System.Windows.Controls.Control()).PixelsPerDip);

            var geometry = formatted.BuildGeometry(new System.Windows.Point(position.X, position.Y));
            var flattened = geometry.GetFlattenedPathGeometry(0.25, ToleranceType.Absolute);

            foreach (var figure in flattened.Figures)
            {
                var start = figure.StartPoint;
                foreach (var seg in figure.Segments)
                {
                    if (seg is PolyLineSegment poly)
                    {
                        var prev = start;
                        foreach (var pt in poly.Points)
                        {
                            var line = new Line(new Point(prev.X, prev.Y), new Point(pt.X, pt.Y));
                            AddLine(line, brush, LineStyle.Solid, 1.0, opacity);
                            prev = pt;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// SDF-based text rendering using a preloaded font texture atlas.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="brush"></param>
        /// <param name="fontSize"></param>
        /// <param name="opacity"></param>
        private void AddText(string text, Point position, Brush brush, double fontSize = 16, double opacity = 1.0, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, ShapeId? shp = null)
        {
            if (string.IsNullOrWhiteSpace(text) || brush is not SolidColorBrush solidBrush || SdfFont is null)
                return;

            var zoomFactor = IsTextSizeZoomable ? 1 / ZoomFactor : 1f;

            var color = new SolidColorBrush(Color.FromArgb(
                (byte)(opacity * 255),
                solidBrush.Color.R,
                solidBrush.Color.G,
                solidBrush.Color.B)).ToVectorColor();

            float scale = (float)(fontSize / SdfFont.LineHeight) * zoomFactor;

            // Measure text dimensions
            float totalWidth = 0f;
            float maxHeight = 0f;

            foreach (char c in text)
            {
                if (!SdfFont.Glyphs.TryGetValue(c, out var glyph))
                    continue;

                totalWidth += glyph.Advance * scale * 0.9f; // * 0.9f to reduce char spacing
                float glyphHeight = glyph.Bounds.Height * scale;
                if (glyphHeight > maxHeight)
                    maxHeight = glyphHeight;
            }
            // Apply alignment offset
            float padX = SdfFont.OffsetX * 0.0f, padY = SdfFont.OffsetY*0.9f; // *0.9f just for fine tuning
            var alignmentOffset = GetAlignedOffset(alignment, totalWidth, maxHeight);
            float x = (float)position.X + padX * scale + alignmentOffset.X;
            float y = (float)position.Y + padY * scale + alignmentOffset.Y; // TESTING: Static offset (maxHeight * scale)

            var boundingBox = new Rectangle((float)position.X + alignmentOffset.X, 
                (float)position.Y + alignmentOffset.Y,
                totalWidth,
                maxHeight);

            if (DebugMode)
            {
                AddLine(boundingBox.TopLine);
                AddLine(boundingBox.BottomLine);
                AddLine(boundingBox.LeftLine);
                AddLine(boundingBox.RightLine);
            }
            
            // Second pass: layout and render
            foreach (char c in text)
            {
                if (!SdfFont.Glyphs.TryGetValue(c, out var glyph))
                    continue;

                float gx = x + glyph.Bounds.X * scale;
                float gy = y + glyph.Bounds.Y * scale;
                float gw = glyph.Bounds.Width * scale;
                float gh = glyph.Bounds.Height * scale;

                var rect = new Rectangle(gx, gy, gw, gh);

                var uv = Rectangle.FromRectF(glyph.UVRect);
                var verts = CreateRectVertices(rect, color, uv, _zIndex);
                // Manually assign correct UVs later if needed

                RectVertices.AddRange(verts);
                RectBatches.Add((SdfFont.TextureId, 6));

                x += glyph.Advance * scale * 0.9f; // * 0.9f to reduce char spacing
            }
            // Add to Shapes if a ShapeId is given
            if (shp is ShapeId id) SceneShapes.Add(new DrawingGeometry(id, boundingBox, _zIndex));
        }


        private static float[] CreateRectVertices(Rectangle r, Vector4 col, float uX = 0f, float uY = 0f, int zLevel = 0)
        {
            float x = (float)r.X;
            float y = (float)r.Y;
            float z = zLevel;
            float w = (float)r.Width;
            float h = (float)r.Height;

            return new float[]
            {
                x,     y,     z, col.X, col.Y, col.Z, col.W, 0,     0,
                x + w, y,     z, col.X, col.Y, col.Z, col.W, uX,    0,
                x,     y + h, z, col.X, col.Y, col.Z, col.W, 0,     uY,
                x + w, y,     z, col.X, col.Y, col.Z, col.W, uX,    0,
                x + w, y + h, z, col.X, col.Y, col.Z, col.W, uX,    uY,
                x,     y + h, z, col.X, col.Y, col.Z, col.W, 0,     uY,
            };
        }
        private static float[] CreateRectVertices(Rectangle r, Vector4 col, Rectangle uv, int zLevel = 0)
        {
            float x = (float)r.X;
            float y = (float)r.Y;
            float z = zLevel;
            float w = (float)r.Width;
            float h = (float)r.Height;

            float u1 = (float)uv.X;
            float v1 = (float)uv.Y;
            float u2 = u1 + (float)uv.Width;
            float v2 = v1 + (float)uv.Height;

            return new float[]
            {
                x,     y,     z, col.X, col.Y, col.Z, col.W, u1, v1,
                x + w, y,     z, col.X, col.Y, col.Z, col.W, u2, v1,
                x,     y + h, z, col.X, col.Y, col.Z, col.W, u1, v2,
                x + w, y,     z, col.X, col.Y, col.Z, col.W, u2, v1,
                x + w, y + h, z, col.X, col.Y, col.Z, col.W, u2, v2,
                x,     y + h, z, col.X, col.Y, col.Z, col.W, u1, v2,
            };
        }
        private static float[] CreateLineVertices(Line l, Vector4 col, float dash, float gap, int zLevel = 0)
        {
            var p1 = l.Start;
            var p2 = l.End;
            float z = zLevel;

            float length = (float)l.Length;

            return new float[]
            {
                (float)p1.X, (float)p1.Y, z, col.X, col.Y, col.Z, col.W, dash, gap, 0f,
                (float)p2.X, (float)p2.Y, z, col.X, col.Y, col.Z, col.W, dash, gap, length
            };
        }
        private static float[] CreateCircleVertices(Point center, double radius, Vector4 color, int segments = 32, int zLevel = 0)
        {
            float cx = (float)center.X;
            float cy = (float)center.Y;
            float r = (float)radius;

            var verts = new List<float>(segments * 3 * 9); // 3 vertices per triangle, 9 floats per vertex

            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathF.PI * 2f * i / segments;
                float angle2 = MathF.PI * 2f * (i + 1) / segments;

                float x1 = cx + MathF.Cos(angle1) * r;
                float y1 = cy + MathF.Sin(angle1) * r;
                float x2 = cx + MathF.Cos(angle2) * r;
                float y2 = cy + MathF.Sin(angle2) * r;

                verts.AddRange(new float[]
                {
                    cx, cy, zLevel, color.X, color.Y, color.Z, color.W, 0f, 0f, // center
                    x1, y1, zLevel, color.X, color.Y, color.Z, color.W, 0f, 0f, // edge1
                    x2, y2, zLevel, color.X, color.Y, color.Z, color.W, 0f, 0f  // edge2
                });
            }

            return verts.ToArray();
        }

        private static Vector2 GetAlignedOffset(TextAlignment alignment, double width, double height)
        {
            float offsetX = alignment.HasFlag(TextAlignment.CenterH) ? -(float)width / 2 :
                alignment.HasFlag(TextAlignment.Right) ? -(float)width :
                0;

            float offsetY = alignment.HasFlag(TextAlignment.CenterV) ? -(float)height / 2 :
                alignment.HasFlag(TextAlignment.Bottom) ? -(float)height :
                0;

            return new Vector2(offsetX, offsetY);
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
                minX - ScenePadding,
                minY - ScenePadding,
                (maxX - minX) + ScenePadding * 2,
                (maxY - minY) + ScenePadding * 2
            );
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

        #endregion
    }

    [Flags]
    public enum TextAlignment
    {
        // Horizontal
        Left = 1 << 0,  // 000001
        CenterH = 1 << 1,  // 000010
        Right = 1 << 2,  // 000100

        // Vertical
        Top = 1 << 3,  // 001000
        CenterV = 1 << 4,  // 010000
        Bottom = 1 << 5,  // 100000

        // Combined
        Center = CenterH | CenterV // 010010
    }

    public enum HatchFitMode
    {
        OriginalPixelSize,
        FitToWidth,
        FitToHeight,
        StretchToFill
    }

    public enum LineStyle
    {
        Solid,
        Dashed,
        Dotted
    }
}
