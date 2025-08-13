using BauphysikToolWPF.Models.UI;
using BT.Geometry;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = BT.Geometry.Point;
using Size = BT.Geometry.Size;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class OglDrawingRepo
    {
        public TextureManager TextureManager { get; set; }
        private SdfFont? SdfFont => TextureManager.SdfFont;
        private bool CanDraw => TextureManager != null && SdfFont != null;
        public int ZIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the font size. 
        /// If <see cref="IsTextSizeZoomable"/> is <c>false</c>, the value represents a fixed height in screen pixels, 
        /// independent of zoom or viewport scaling. 
        /// If <see cref="IsTextSizeZoomable"/> is <c>true</c>, the value is interpreted as a height in world units, 
        /// and scales along with the rest of the scene.
        /// </summary>
        public int FontSize { get; set; } = 32;
        public float ZoomFactor { get; set; } = 1f;
        public bool IsTextSizeZoomable { get; set; } = false;
        public bool DebugMode { get; set; } = false; // Enable debug mode for additional rendering features

        public List<float> RectVertices { get; } = new();
        public List<float> LineVertices { get; } = new();
        public List<(int? TextureId, int Count)> RectBatches { get; } = new();
        public List<(float LineWidth, int Count)> LineBatches { get; } = new();
        public List<IDrawingGeometry> SceneShapes { get; } = new();

        #region Basics

        public int AddRectangle(Rectangle rect, Brush bgColor, Brush? textureBrush = null, HatchFitMode mode = HatchFitMode.FitToHeight, double opacity = 1.0, ShapeId? shp = null)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");
            
            // Extract color and apply opacity (in alpha channel)
            if (bgColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                bgColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 color = bgColor.ToVectorColor();
            int? texId = null;

            float[] verts;
            if (textureBrush is DrawingBrush hatch)
            {
                texId = TextureManager.GetTextureIdForBrush(hatch);
                Size texSize = texId.HasValue ? TextureManager.GetTextureSize(texId.Value) : Size.Empty;
                float texRepeatX = 1f, texRepeatY = 1f;
                
                if (!texSize.IsEmpty)
                {
                    float w = (float)rect.Width;
                    float h = (float)rect.Height;
                    float texW = (float)texSize.Width;
                    float texH = (float)texSize.Height;

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
                verts = CreateRectVertices(rect, color, texRepeatX, texRepeatY, ZIndex);
            }
            else
            {
                verts = CreateRectVertices(rect, color, ZIndex);
            }

            var vertexStartIndex = RectVertices.Count;
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));

            if (shp is ShapeId id) SceneShapes.Add(new DrawingGeometry(id, rect, ZIndex));

            return vertexStartIndex;
        }
        
        public void AddRectangle(IDrawingGeometry geom)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");
            
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
                var texSize = texId.HasValue ? TextureManager.GetTextureSize(texId.Value) : Size.Empty;

                texRepeatX = 1f;
                texRepeatY = 1f;

                if (!texSize.IsEmpty)
                {
                    float w = (float)geom.Rectangle.Width;
                    float h = (float)geom.Rectangle.Height;
                    float texW = (float)texSize.Width;
                    float texH = (float)texSize.Height;

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
            var verts = CreateRectVertices(geom.Rectangle, color, texRepeatX, texRepeatY, ZIndex);
            //geom.VertexStartIndex = RectVertices.Count; // Store vertex start index in the geometry
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
        }

        public void AddLine(Line line, Brush? lineColor = null, LineStyle style = LineStyle.Solid, double thickness = 1.0, double opacity = 1.0)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");
            
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

            var verts = CreateLineVertices(line, col, dash, gap, ZIndex);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)thickness, 2));
        }

        public void AddLine(Point ptStart, Point ptEnd, Brush? lineColor = null, LineStyle style = LineStyle.Solid, double thickness = 1.0, double opacity = 1.0)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");
            
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

            var verts = CreateLineVertices(line, col, dash, gap, ZIndex);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)thickness, 2));
        }

        public void AddLine(Line line, Pen pen)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");
            
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

            var verts = CreateLineVertices(line, color, dashLength, gapLength, ZIndex);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)pen.Thickness, 2));
        }

        public void AddCircle(Point center, double radius, Brush fillColor, Brush? outlineColor = null, int outlineWidth = 1, int segments = 32, double opacity = 1.0)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");

            // Apply opacity to fill color
            if (fillColor is SolidColorBrush solidFill)
            {
                var c = solidFill.Color;
                byte alpha = (byte)(opacity * 255);
                fillColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 fill = fillColor.ToVectorColor();
            var circleVerts = CreateCircleVertices(center, radius, fill, segments, ZIndex);

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

                    var verts = CreateLineVertices(new Line(x1, y1, x2, y2), col, dash, gap, ZIndex);
                    outlineVerts.AddRange(verts);
                }

                LineVertices.AddRange(outlineVerts);
                LineBatches.Add((outlineWidth, segments * 2));
            }
        }

        #endregion

        #region Text

        /// <summary>
        /// rasterized texture-based text
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="fontSize"></param>
        /// <param name="opacity"></param>
        public void AddTextRasterized(Point position, string text, Brush color, double fontSize = 32, double opacity = 1.0, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");

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
            var verts = CreateRectVertices(rect, white, 1f, 1f, ZIndex);
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
        public void AddTextVectorized(string text, Point position, Brush brush, double fontSize = 32, double opacity = 1.0)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");

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
        public void AddText(string text, Point position, Brush brush, double fontSize = 32, double opacity = 1.0, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, ShapeId? shp = null)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");

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

                totalWidth += glyph.Advance * scale;
                float glyphHeight = glyph.Bounds.Height * scale;
                if (glyphHeight > maxHeight)
                    maxHeight = glyphHeight;
            }
            // Apply alignment offset
            var alignmentOffset = GetAlignedOffset(alignment, totalWidth, maxHeight);
            float x = (float)position.X + alignmentOffset.X;
            float y = (float)position.Y + alignmentOffset.Y; // TESTING: Static offset (maxHeight * scale)

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
                var verts = CreateRectVertices(rect, color, uv, ZIndex);
                // Manually assign correct UVs later if needed

                RectVertices.AddRange(verts);
                RectBatches.Add((SdfFont.TextureId, 6));

                x += glyph.Advance * scale;
            }
            // Add to Shapes if a ShapeId is given
            if (shp is ShapeId id) SceneShapes.Add(new DrawingGeometry(id, boundingBox, ZIndex));
        }

        public void AddLayerTextMarker(Point pt, string text, Brush color, int fontSize = 32, double opacity = 1.0, ShapeId? shp = null)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");

            var charLength = text.Length;
            var zoomFactor = IsTextSizeZoomable ? (1 / ZoomFactor) : 1.0;
            var rad = (fontSize / 3 + charLength) * zoomFactor;

            AddCircle(pt, rad, Brushes.White, color, opacity: opacity);
            // Make the circles behave like if a layer is clicked
            if (shp is ShapeId id) SceneShapes.Add(new DrawingGeometry(id, Rectangle.FromCircle(pt, rad), ZIndex));

            ZIndex++;
            AddText(text, pt, color, fontSize, 1.0, TextAlignment.Center);
        }

        #endregion

        #region Dimensional Chains

        public void AddDimensionalChainsHorizontal(Point ptStart, double[] xIncrements, double distance, bool drawBelow = false, double oneCmConversion = 1, int fontSize = 32)
        {
            if (xIncrements is null || xIncrements.Length < 1) return;

            var xCoords = new List<double> { ptStart.X };
            for (int i = 0; i < xIncrements.Length; i++)
            {
                double lastX = xCoords[^1];
                double nextX = lastX + xIncrements[i];
                xCoords.Add(nextX);
            }
            AddDimensionalChainsHorizontal(ptStart.Y, xCoords.ToArray(), distance, drawBelow, oneCmConversion, fontSize);
        }
        public void AddDimensionalChainsHorizontal(Point ptStart, double[] xIncrements, double[] xDisplayValues, double distance, bool drawBelow = false, int fontSize = 32)
        {
            if (xIncrements is null || xIncrements.Length < 1) return;

            var xCoords = new List<double> { ptStart.X };
            for (int i = 0; i < xIncrements.Length; i++)
            {
                double lastX = xCoords[^1];
                double nextX = lastX + xIncrements[i];
                xCoords.Add(nextX);
            }
            AddDimensionalChainsHorizontal(ptStart.Y, xCoords.ToArray(), xDisplayValues, distance, drawBelow, fontSize);
        }

        public void AddDimensionalChainsHorizontal(double yValue, double[] xValues, double distance, bool drawBelow = false, double oneCmConversion = 1, int fontSize = 32)
        {
            if (xValues is null || xValues.Length <= 1) return;

            var xCoords = xValues.ToList();

            if (drawBelow) xCoords.Reverse();
            for (int i = 0; i < xCoords.Count - 1; i++)
            {
                double startX = xCoords[i];
                double endX = xCoords[i + 1];
                var startPt = new Point(startX, yValue);
                var endPt = new Point(endX, yValue);
                var widthInCm = new Line(startPt, endPt).Length / oneCmConversion;

                string formattedValueString = NumberConverter.ConvertToString(widthInCm, 2);

                // Only last one is a full chain, others are just start ticks
                if (i == xCoords.Count - 2) DrawSingleDimChain(startPt, endPt, distance, formattedValueString, Brushes.DimGray, (drawBelow ? TextAlignment.Top : TextAlignment.Bottom) | TextAlignment.CenterH, fontSize);
                else
                {
                    DrawSingleDimChainOnlyStartTick(startPt, endPt, distance, formattedValueString, Brushes.DimGray, (drawBelow ? TextAlignment.Top : TextAlignment.Bottom) | TextAlignment.CenterH, fontSize);
                }
            }
        }
        public void AddDimensionalChainsHorizontal(double yValue, double[] xValues, double[] displayValues, double distance, bool drawBelow = false, int fontSize = 32)
        {
            if (xValues is null || xValues.Length <= 1) return;

            var xCoords = xValues.ToList();

            if (drawBelow) xCoords.Reverse();
            for (int i = 0; i < xCoords.Count - 1; i++)
            {
                double startX = xCoords[i];
                double endX = xCoords[i + 1];
                var startPt = new Point(startX, yValue);
                var endPt = new Point(endX, yValue);

                string formattedValueString = NumberConverter.ConvertToString(displayValues[i], 2);

                // Only last one is a full chain, others are just start ticks
                if (i == xCoords.Count - 2) DrawSingleDimChain(startPt, endPt, distance, formattedValueString, Brushes.DimGray, (drawBelow ? TextAlignment.Top : TextAlignment.Bottom) | TextAlignment.CenterH, fontSize);
                else
                {
                    DrawSingleDimChainOnlyStartTick(startPt, endPt, distance, formattedValueString, Brushes.DimGray, (drawBelow ? TextAlignment.Top : TextAlignment.Bottom) | TextAlignment.CenterH, fontSize);
                }
            }
        }

        public void AddDimensionalChainsVertical(double xValue, double[] yValues, double distance, bool drawLeft = false, double oneCmConversion = 1, int fontSize = 32)
        {
            if (yValues is null || yValues.Length <= 1) return;

            var yCoords = yValues.ToList();
            
            if (drawLeft) yCoords.Reverse();
            for (int i = 0; i < yCoords.Count - 1; i++)
            {
                double startY = yCoords[i];
                double endY = yCoords[i + 1];
                var startPt = new Point(xValue, startY);
                var endPt = new Point(xValue, endY);

                var heightInCm = new Line(startPt, endPt).Length / oneCmConversion;
                string formattedValueString = NumberConverter.ConvertToString(heightInCm, 2);

                // Only last one is a full chain, others are just start ticks
                if (i == yCoords.Count - 2) DrawSingleDimChain(startPt, endPt, distance, formattedValueString, alignment: TextAlignment.CenterV | (drawLeft ? TextAlignment.Right : TextAlignment.Left), fontSize: fontSize);
                else
                {
                    DrawSingleDimChainOnlyStartTick(startPt, endPt, distance, formattedValueString, alignment: TextAlignment.CenterV | (drawLeft ? TextAlignment.Right : TextAlignment.Left), fontSize: fontSize);
                }
            }
        }

        public void AddDimensionalChainsVertical(Point ptStart, double[] yIncrements, double distance, bool drawLeft = false, double oneCmConversion = 1, int fontSize = 32)
        {
            if (yIncrements is null || yIncrements.Length < 1) return;

            var yCoords = new List<double> { ptStart.Y };
            for (int i = 0; i < yIncrements.Length; i++)
            {
                double lastY = yCoords[^1];
                double nextY = lastY + yIncrements[i];
                yCoords.Add(nextY);
            }
            AddDimensionalChainsVertical(ptStart.Y, yCoords.ToArray(), distance, drawLeft, oneCmConversion, fontSize);
        }
        
        public void DrawSingleDimChain(Line line, double distance, string displayedValue, Brush? lineColor = null, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, int fontSize = 32, ShapeId? shp = null)
        {
            DrawSingleDimChain(line.Start, line.End, distance, displayedValue, lineColor, alignment, fontSize, shp);
        }

        public void DrawSingleDimChain(Point ptStart, Point ptEnd, double distance, string displayedValue, Brush? lineColor = null, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, int fontSize = 32, ShapeId? shp = null)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");
            
            lineColor ??= Brushes.DimGray;
            int tickSize = 10;
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

            AddLine(offsetLine.Start - tickOffset, offsetLine.Start + tickOffset.Scale(2), lineColor);
            AddLine(offsetLine.End - tickOffset, offsetLine.End + tickOffset.Scale(2), lineColor);

            // 45° diagonal cross ticks (offset ±45° between normal and direction)
            var diagOffset = (vectNormalized - nVect).Normalize() * tickSize;

            AddLine(offsetLine.Start - diagOffset, offsetLine.Start + diagOffset, lineColor);
            AddLine(offsetLine.End - diagOffset, offsetLine.End + diagOffset, lineColor);

            var pos = offsetLine.GetCenter() - tickOffset * 2;
            AddText(displayedValue, pos, lineColor, fontSize: fontSize, alignment: alignment, shp: shp);
        }

        private void DrawSingleDimChainOnlyStartTick(Point ptStart, Point ptEnd, double distance, string displayedValue, Brush? lineColor = null, TextAlignment alignment = TextAlignment.Left | TextAlignment.Top, int fontSize = 32, ShapeId? shp = null)
        {
            if (!CanDraw) throw new InvalidOperationException("Cannot draw: TextureManager or SdfFont is not initialized.");

            lineColor ??= Brushes.DimGray;
            int tickSize = 10;
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

            AddLine(offsetLine.Start - tickOffset, offsetLine.Start + tickOffset.Scale(2), lineColor);

            // 45° diagonal cross ticks (offset ±45° between normal and direction)
            var diagOffset = (vectNormalized - nVect).Normalize() * tickSize;

            AddLine(offsetLine.Start - diagOffset, offsetLine.Start + diagOffset, lineColor);

            var pos = offsetLine.GetCenter() - tickOffset * 2;
            AddText(displayedValue, pos, lineColor, fontSize: fontSize, alignment: alignment, shp: shp);
        }

        #endregion

        #region private

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
