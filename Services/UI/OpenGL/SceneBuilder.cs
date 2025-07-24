using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Windows.Media;
using BauphysikToolWPF.Models.UI;
using Line = BT.Geometry.Line;
using Rectangle = BT.Geometry.Rectangle;
using System;
using System.Windows.Media.Imaging;
using BT.Geometry;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Converts geometry from CrossSectionBuilder into OpenGL-ready vertex data (rectangles and lines),
    /// and provides vertex streams + texture usage data for the renderer.
    /// </summary>
    public class SceneBuilder
    {
        private readonly TextureManager _textureManager;

        public List<float> RectVertices { get; private set; } = new();
        public List<float> LineVertices { get; private set; } = new();
        public List<(int? TextureId, int Count)> RectBatches { get; private set; } = new();
        public List<(float LineWidth, int Count)> LineBatches { get; private set; } = new();

        public SceneBuilder(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }

        public void BuildFrom(IEnumerable<IDrawingGeometry> drawingGeometries)
        {
            RectVertices.Clear();
            LineVertices.Clear();
            RectBatches.Clear();
            LineBatches.Clear();

            foreach (var geom in drawingGeometries)
            {
                if (geom.DrawingBrush is DrawingBrush hatch)
                {
                    AddTexturedRectangle(geom.Rectangle, geom.BackgroundColor, hatch, geom.HatchFitMode);
                }
                else
                {
                    AddRectangle(geom.Rectangle, geom.BackgroundColor);
                }

                AddLine(geom.Rectangle.BottomLine, System.Windows.Media.Brushes.Black, LineStyle.Dashed);
                if (geom.ShapeId.Index == 0)
                    AddLine(geom.Rectangle.TopLine, System.Windows.Media.Brushes.Black, LineStyle.Solid, 2.0);

                if (geom.ShapeId.Type == ShapeType.SubConstructionLayer)
                {
                    AddLine(geom.Rectangle.LeftLine);
                    AddLine(geom.Rectangle.RightLine);
                }
                //AddCircle(geom.Rectangle.Center, 20, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Black);
               // AddTextLabel(geom.Rectangle.Center, "Section A", System.Windows.Media.Brushes.Black, 18, 0.9);
            }
        }

        private void AddRectangle(Rectangle rect, Brush bgColor, double opacity = 1.0)
        {
            if (bgColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                bgColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 color = GetColorFromBrush(bgColor);
            int? texId = null;
            var verts = CreateRectVertices(rect, color);
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
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
            Vector4 color = GetColorFromBrush(bgColor);
            var texId = _textureManager.GetTextureIdForBrush(hatch);
            var texSize = texId.HasValue ? _textureManager.GetTextureSize(texId.Value) : null;

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

            var verts = CreateRectVertices(rect, color, texRepeatX, texRepeatY);
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
        }

        private void AddLine(Line line, Brush? lineColor = null, LineStyle style = LineStyle.Solid, double thickness = 1.0, double opacity = 1.0)
        {
            lineColor ??= System.Windows.Media.Brushes.Black;
            // Extract color and apply opacity (in alpha channel)
            if (lineColor is SolidColorBrush solid)
            {
                var c = solid.Color;
                byte alpha = (byte)(opacity * 255);
                lineColor = new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
            }
            Vector4 col = GetColorFromBrush(lineColor);
            float dash = 0f, gap = 0f;
            switch (style)
            {
                case LineStyle.Dashed: dash = 8f; gap = 4f; break;
                case LineStyle.Dotted: dash = 2f; gap = 4f; break;
            }

            var verts = CreateLineVertices(line, col, dash, gap);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)thickness, 2));
        }
        
        private void AddLine(Line line, Pen pen)
        {
            if (pen == null || pen.Brush == null) return;

            // Extract color
            Vector4 color = GetColorFromBrush(pen.Brush);

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

            var verts = CreateLineVertices(line, color, dashLength, gapLength);
            LineVertices.AddRange(verts);
            LineBatches.Add(((float)pen.Thickness, 2));
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
            Vector4 fill = GetColorFromBrush(fillColor);

            // Center vertex
            float cx = (float)center.X;
            float cy = (float)center.Y;

            // Build triangle fan (center + outer ring)
            var verts = new List<float>();
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathF.PI * 2f * i / segments;
                float angle2 = MathF.PI * 2f * (i + 1) / segments;

                float x1 = cx + MathF.Cos(angle1) * (float)radius;
                float y1 = cy + MathF.Sin(angle1) * (float)radius;
                float x2 = cx + MathF.Cos(angle2) * (float)radius;
                float y2 = cy + MathF.Sin(angle2) * (float)radius;

                // Triangle: center, edge1, edge2
                verts.AddRange(new float[]
                {
                    cx, cy, 0f, fill.X, fill.Y, fill.Z, fill.W, 0f, 0f,
                    x1, y1, 0f, fill.X, fill.Y, fill.Z, fill.W, 0f, 0f,
                    x2, y2, 0f, fill.X, fill.Y, fill.Z, fill.W, 0f, 0f
                });
            }

            RectVertices.AddRange(verts);
            RectBatches.Add((null, segments * 3));

            // Optionally add outline as a dashed/polyline
            if (outlineColor != null)
            {
                Vector4 col = GetColorFromBrush(outlineColor);
                var outlineVerts = new List<float>();
                float dash = 0f, gap = 0f;

                for (int i = 0; i < segments; i++)
                {
                    float angle1 = MathF.PI * 2f * i / segments;
                    float angle2 = MathF.PI * 2f * (i + 1) / segments;

                    float x1 = cx + MathF.Cos(angle1) * (float)radius;
                    float y1 = cy + MathF.Sin(angle1) * (float)radius;
                    float x2 = cx + MathF.Cos(angle2) * (float)radius;
                    float y2 = cy + MathF.Sin(angle2) * (float)radius;

                    var length = MathF.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                    outlineVerts.AddRange(new float[]
                    {
                x1, y1, 0f, col.X, col.Y, col.Z, col.W, dash, gap, 0f,
                x2, y2, 0f, col.X, col.Y, col.Z, col.W, dash, gap, length
                    });
                }

                LineVertices.AddRange(outlineVerts);
                LineBatches.Add((outlineWidth, segments * 2));
            }
        }

        public void AddTextLabel(Point position, string text, Brush color, double fontSize = 16, double opacity = 1.0)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Render text to bitmap
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Windows.FlowDirection.LeftToRight,
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
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    solid,
                    1.0
                ), new System.Windows.Point(0, 0));
            }

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            // Upload to OpenGL texture
            int texId = _textureManager.CreateTextTextureFromBitmap(bmp);
            if (texId == -1) return;

            // Create rectangle for text quad
            var rect = new Rectangle(position.X, position.Y, width, height);
            Vector4 white = new(1f, 1f, 1f, 1f); // vertex color = white, since texture has pre-colored glyphs
            var verts = CreateRectVertices(rect, white, 1f, 1f);
            RectVertices.AddRange(verts);
            RectBatches.Add((texId, 6));
        }
        private static float[] CreateRectVertices(Rectangle r, Vector4 col, float uX = 0f, float uY = 0f)
        {
            float x = (float)r.X;
            float y = (float)r.Y;
            float w = (float)r.Width;
            float h = (float)r.Height;

            return new float[]
            {
                x,     y,     0, col.X, col.Y, col.Z, col.W, 0,     0,
                x + w, y,     0, col.X, col.Y, col.Z, col.W, uX,    0,
                x,     y + h, 0, col.X, col.Y, col.Z, col.W, 0,     uY,
                x + w, y,     0, col.X, col.Y, col.Z, col.W, uX,    0,
                x + w, y + h, 0, col.X, col.Y, col.Z, col.W, uX,    uY,
                x,     y + h, 0, col.X, col.Y, col.Z, col.W, 0,     uY,
            };
        }
        private static float[] CreateLineVertices(Line l, Vector4 col, float dash, float gap)
        {
            var p1 = l.Start;
            var p2 = l.End;
            float length = (float)l.Length;

            return new float[]
            {
                (float)p1.X, (float)p1.Y, 0f, col.X, col.Y, col.Z, col.W, dash, gap, 0f,
                (float)p2.X, (float)p2.Y, 0f, col.X, col.Y, col.Z, col.W, dash, gap, length
            };
        }

        public static Vector4 GetColorFromBrush(Brush b)
        {
            if (b is SolidColorBrush s)
            {
                var c = s.Color;
                return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            }
            return new Vector4(1f, 1f, 1f, 1f);
        }
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
