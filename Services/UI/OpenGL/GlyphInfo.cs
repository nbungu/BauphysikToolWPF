using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class GlyphInfo
    {
        public char Character { get; set; }
        public RectangleF Bounds { get; set; }   // On screen
        public RectangleF UVRect { get; set; }   // In texture
        public float Advance { get; set; }
    }
    public class SdfFont
    {
        public Dictionary<char, GlyphInfo> Glyphs { get; } = new();
        public int TextureId { get; }
        public float LineHeight { get; }

        public SdfFont(Dictionary<char, GlyphInfo> glyphs, int textureId, float lineHeight)
        {
            Glyphs = glyphs;
            TextureId = textureId;
            LineHeight = lineHeight;
        }
    }

    /// <summary>
    /// loader for BMFont (text format)
    /// </summary>
    public static class SdfFontLoader
    {
        public static SdfFont LoadFromFntFile(string fntPath, TextureManager texManager)
        {
            var glyphs = new Dictionary<char, GlyphInfo>();
            float lineHeight = 32; // fallback default
            int texWidth = 1, texHeight = 1;

            string dir = Path.GetDirectoryName(fntPath)!;
            string[] lines = File.ReadAllLines(fntPath);

            string? imageFile = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("common"))
                {
                    texWidth = GetInt(line, "scaleW");
                    texHeight = GetInt(line, "scaleH");
                    lineHeight = GetFloat(line, "lineHeight");
                }
                else if (line.StartsWith("page"))
                {
                    imageFile = GetString(line, "file");
                }
                else if (line.StartsWith("char id"))
                {
                    int id = GetInt(line, "id");
                    char ch = (char)id;

                    float x = GetInt(line, "x");
                    float y = GetInt(line, "y");
                    float width = GetInt(line, "width");
                    float height = GetInt(line, "height");
                    float xoffset = GetInt(line, "xoffset");
                    float yoffset = GetInt(line, "yoffset");
                    float xadvance = GetInt(line, "xadvance");

                    var bounds = new RectangleF(xoffset, yoffset, width, height);
                    var uvRect = new RectangleF(x / texWidth, y / texHeight, width / texWidth, height / texHeight);

                    glyphs[ch] = new GlyphInfo
                    {
                        Character = ch,
                        Bounds = bounds,
                        UVRect = uvRect,
                        Advance = xadvance
                    };
                }
            }

            if (imageFile == null)
                throw new FileNotFoundException("No texture page defined in FNT file.");

            string atlasPath = Path.Combine(dir, imageFile);
            int texId = texManager.LoadTextureFromFile(atlasPath);

            return new SdfFont(glyphs, texId, lineHeight);
        }

        private static int GetInt(string line, string key) =>
            int.Parse(Extract(line, key), CultureInfo.InvariantCulture);

        private static float GetFloat(string line, string key) =>
            float.Parse(Extract(line, key), CultureInfo.InvariantCulture);

        private static string GetString(string line, string key)
        {
            string val = Extract(line, key);
            if (val.StartsWith("\"") && val.EndsWith("\""))
                return val.Substring(1, val.Length - 2);
            return val;
        }

        private static string Extract(string line, string key)
        {
            int i = line.IndexOf(key + "=");
            if (i == -1) throw new Exception($"Key {key} not found in line: {line}");
            int start = i + key.Length + 1;
            int end = line.IndexOf(' ', start);
            return (end == -1) ? line.Substring(start) : line.Substring(start, end - start);
        }
    }

    /*
     
    To support SDF text rendering in OpenGL:

    1. Preprocess your font into a SDF atlas + metadata (e.g., with msdfgen or a prebuilt tool like Hiero or BMFont).

    2. Load the atlas texture into OpenGL via TextureManager.

    3. Store per-glyph data (position, size, advance, UVs) via a metadata structure (like a GlyphInfo class).

    4. For each character:

        - Compute quad vertex data

        - Apply correct texture coordinates (UV)

        - Use a shader that samples the SDF and renders crisp text via alpha cutoff/smoothstep



    myfont.fnt (describes glyphs, metrics, UVs)
            A metadata file (plain text or XML)

        Contains per-glyph info:

            Position in texture atlas (UVs)

            Size & offset

            Advance width

            Line height

    myfont.png (texture atlas with SDF-encoded glyphs)
            The SDF atlas texture

        Contains all glyphs packed into one image

        Each glyph is encoded as a grayscale distance field (for smooth scaling and crisp edges)


    Option A: Use BMFont (Windows, Free)

    Download & install BMFont

    In BMFont:

        Go to Options → Font Settings

            Choose your font (e.g., "Segoe UI")

            Set size (e.g., 32 or 64)

        Go to Options → Export Options

            Output format: Text (not binary)

            Texture format: PNG

        Enable Distance Field Font (requires custom build or plugin if using MSDF)

    Click Generate to export:

        myfont.fnt

        myfont.png

💡 For best quality, use a font size ≥ 64 and enable distance field generation (if available).

    /Assets/Fonts/myfont.fnt
    /Assets/Fonts/myfont.png

     */
}
