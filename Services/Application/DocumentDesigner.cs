using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.UI.OpenGL;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Controls;
using System.Windows.Media;
using ABI.Windows.Devices.Input;
using static BauphysikToolWPF.Models.UI.Enums;
using static OpenTK.Audio.OpenAL.ALC;

namespace BauphysikToolWPF.Services.Application
{
    public class DocumentDesigner
    {
        private const int MarginTop = 32;
        private const int MarginBottom = 32;
        private const int MarginOuter = 32; // the smaller margin
        private const int MarginInner = 48; // the larger margin

        private const int Padding = 8;
        private const int RowHeight = 14;
        private const string FontFamilyRegular = "Arial";
        private const string FontFamilyForData = "Verdana";

        public static void FullCatalogueExport(Project? project)
        {
            if (project == null) return;

            // Force rendering of all element images
            project.RenderAllElementImages(target: RenderTarget.Document, withDecorations: true);
            
            XFont titleFont = new XFont(FontFamilyRegular, 10, XFontStyleEx.Bold);
            XFont bodyFont = new XFont(FontFamilyRegular, 9, XFontStyleEx.Regular);
            XFont bodyFontBold = new XFont(FontFamilyRegular, 9, XFontStyleEx.Bold);
            XFont tableHeaderFont = new XFont(FontFamilyRegular, 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont(FontFamilyForData, 8, XFontStyleEx.Regular);
            XFont tableBodyFontBold = new XFont(FontFamilyForData, 8, XFontStyleEx.Bold);

            var thinPen = new XPen(XColors.Black, 0.5); // 0.5pt line

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Elements Table";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            var (marginLeft, marginRight) = GetPageMargins(document.PageCount);

            // Draw Header and Footer
            DrawHeader(out double headerBottom, gfx, page, project.Name, marginLeft, marginRight);
            DrawFooter(gfx, page, project.UserName, document.PageCount);
            
            double startX = marginLeft;
            double startY = headerBottom + 2 * Padding;
            double contentWidth = page.Width - marginLeft - marginRight;
            double contentHeight = page.Height - MarginTop - MarginBottom;

            // Bauteilkatalog Title
            gfx.DrawString($"Bauteilkatalog - {project.Name}", titleFont, XBrushes.Black,
                new XRect(startX, startY, contentWidth, titleFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight + Padding;

            // Linker Block:
            var startRightBlockTop = startY;
            var text = "Projektname:";
            var textWidth = gfx.MeasureString(text, bodyFont).Width;
            var maxTextWidth = textWidth;
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            text = "Bearbeiter:";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            maxTextWidth = Math.Max(maxTextWidth, textWidth);
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            text = "Nutzungsart:";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            maxTextWidth = Math.Max(maxTextWidth, textWidth);
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            text = "Art des Gebäudes:";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            maxTextWidth = Math.Max(maxTextWidth, textWidth);
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            text = "Anzahl Bauteile:";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            maxTextWidth = Math.Max(maxTextWidth, textWidth);
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            // Rechter Block:
            gfx.DrawString($"{project.Name}", bodyFont, XBrushes.Black, new XRect(startX + maxTextWidth + Padding, startRightBlockTop, contentWidth / 2, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;
            gfx.DrawString($"{project.UserName}", bodyFont, XBrushes.Black, new XRect(startX + maxTextWidth + Padding, startRightBlockTop, contentWidth / 2, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;
            gfx.DrawString($"{Enums.BuildingUsageTypeMapping[project.BuildingUsage]}", bodyFont, XBrushes.Black, new XRect(startX + maxTextWidth + Padding, startRightBlockTop, contentWidth / 2, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;
            gfx.DrawString($"{Enums.BuildingAgeTypeMapping[project.BuildingAge]}", bodyFont, XBrushes.Black, new XRect(startX + maxTextWidth + Padding, startRightBlockTop, contentWidth / 2, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;
            gfx.DrawString($"{project.Elements.Count}", bodyFont, XBrushes.Black, new XRect(startX + maxTextWidth + Padding, startRightBlockTop, contentWidth / 2, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;
            
            //
            startY += Padding; // extra space
            gfx.DrawString("Liste: U- und R-Werte aller Bauteile", bodyFontBold, XBrushes.Black,
                new XRect(startX, startY, contentWidth, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            project.SortElements(ElementSortingType.TypeNameAscending);
            var elements = project.Elements;

            double cellHeight = RowHeight + 0.5 * Padding;

            var tf = new XTextFormatter(gfx);

            // New headers and adjusted column widths
            string[] headers = { "Nr.", "Bauteilbezeichnung", "", "Kategorie", "U-Wert\nGEG\n[W/(m²K)]", "Wärmedurchlasswiderstand\nR [m²K/W]", "" };
            string[] secondRowHeaders = { "", "", "", "", "", "Ist-Wert", "Soll-Wert\nDIN 4108-2" }; // New row header for Wärmedurchlasswiderstand

            double[] columnProportions = { 34, 24, 140, 120, 56, 64, 64 }; // Original widths
            double totalWeight = columnProportions.Sum();
            double[] columnWidths = columnProportions.Select(p => p / totalWeight * contentWidth).ToArray();

            // Adjust data table size to match new header count
            string[,] data = new string[elements.Count, secondRowHeaders.Length];

            for (int i = 0; i < elements.Count; i++)
            {
                var el = elements[i];
                data[i, 0] = el.ShortName;
                data[i, 1] = "";                        // Color visual only
                data[i, 2] = el.Name;                   // Bauteilbezeichnung
                data[i, 3] = el.Construction.TypeName;  // Kategorie
                data[i, 4] = el.UValue.ToString("F3", CultureInfo.CurrentCulture);
                data[i, 5] = $"R = {el.RTotValue.ToString("F2", CultureInfo.CurrentCulture)}";
                data[i, 6] = $"R ≥ {el.Requirements.RMin:0.00}";
            }

            // Draw table headers with line break support
            double currentX = startX;

            // Draw first row of headers (spanning columns 5 and 6 for Wärmedurchlasswiderstand)
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i] == "") continue; // Skip empty header

                if (i == 1)
                {
                    var headerRect = new XRect(currentX, startY, columnWidths[i] + columnWidths[i+1], cellHeight * 3);
                    gfx.DrawRectangle(thinPen, XBrushes.LightGray, headerRect);
                    tf.Alignment = XParagraphAlignment.Center;
                    var contentRect = new XRect(currentX + 2, startY + 4, headerRect.Width - 4, headerRect.Height - 8);
                    tf.DrawString(headers[i], tableHeaderFont, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                    currentX += columnWidths[i] + columnWidths[i+1]; 
                }
                // Span "Wärmedurchlasswiderstand" across columns 4 and 5 (columns 5 and 6 span across the two cells)
                else if (i == 5)
                {
                    var headerRect = new XRect(currentX, startY, columnWidths[i] + columnWidths[i+1], cellHeight * 1.5);
                    gfx.DrawRectangle(thinPen, XBrushes.LightGray, headerRect);
                    tf.Alignment = XParagraphAlignment.Center;
                    var contentRect = new XRect(currentX + 2, startY + 4, headerRect.Width - 4, headerRect.Height - 8);
                    tf.DrawString(headers[i], tableHeaderFont, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                    currentX += columnWidths[i] + columnWidths[i+1]; // Skip columns 4 and 5 since they last
                }
                else
                {
                    var headerRect = new XRect(currentX, startY, columnWidths[i], cellHeight * 3); // taller header for first row
                    gfx.DrawRectangle(thinPen, XBrushes.LightGray, headerRect);
                    var contentRect = new XRect(currentX + 2, startY + 4, headerRect.Width - 4, headerRect.Height - 8);
                    tf.Alignment = XParagraphAlignment.Center;
                    tf.DrawString(headers[i], tableHeaderFont, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                    currentX += columnWidths[i];
                }
            }
            // Draw second row of headers 
            currentX = startX;
            for (int i = 0; i < secondRowHeaders.Length; i++)
            {
                if (secondRowHeaders[i] == "") {
                    currentX += columnWidths[i]; // Skip empty header
                    continue;
                }

                var headerRect = new XRect(currentX, startY + cellHeight * 1.5, columnWidths[i], cellHeight * 1.5); // normal height for second row
                gfx.DrawRectangle(thinPen, XBrushes.LightGray, headerRect);
                var contentRect = new XRect(headerRect.X + 2, headerRect.Y + 4, headerRect.Width - 4, headerRect.Height - 8);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(secondRowHeaders[i], tableBodyFont, XBrushes.Black, contentRect, XStringFormats.TopLeft);

                currentX += columnWidths[i];
            }

            // Draw table data with centering and wrapping
            double currentY = startY + cellHeight * 3; // Move below both header rows
            for (int row = 0; row < data.GetLength(0); row++)
            {
                currentX = startX;
                double rowHeight = cellHeight;

                // Measure row height for wrapping in Bauteilbezeichnung (column 3 after adding "Farbe")
                //var tf = new XTextFormatter(gfx);
                var nameText = data[row, 3];
                var nameSize = gfx.MeasureString(nameText, tableBodyFont);
                var lines = (int)Math.Ceiling(nameSize.Width / columnWidths[3]);
                rowHeight = Math.Max(rowHeight, lines * 10);

                // Draw each column
                for (int col = 0; col < secondRowHeaders.Length; col++)
                {
                    var rect = new XRect(currentX, currentY, columnWidths[col], rowHeight);
                    gfx.DrawRectangle(thinPen, rect);

                    // Center-align everything except Bauteilbezeichnung (for wrapping)
                    tf.Alignment = col == 2 ? XParagraphAlignment.Left : XParagraphAlignment.Center;

                    // Special case: draw color box for column 1
                    if (col == 1)
                    {
                        Color mediaColor = Colors.Transparent;
                        try { mediaColor = elements[row].Color; } catch { }

                        // Convert to PdfSharp XColor
                        XColor fillColor = XColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);

                        gfx.DrawRectangle(new XSolidBrush(fillColor), new XRect(currentX+0.5, currentY+0.5, columnWidths[col]-1, rowHeight-1));
                    }
                    else
                    {
                        string cellText = data[row, col];

                        // truncate to column width
                        cellText = TruncateTextToWidth(gfx, cellText, tableBodyFont, columnWidths[col] - 8); // subtract padding

                        var contentRect = new XRect(currentX + 4, currentY + 4, columnWidths[col] - 8, rowHeight - 8);
                        if (col == 4) // U-Wert hervorheben mit anderer Font
                        {
                            tf.DrawString(cellText, tableBodyFontBold, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                        }
                        else
                        {
                            tf.DrawString(cellText, tableBodyFont, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                        }
                    }
                    currentX += columnWidths[col];
                }
                currentY += rowHeight;

                // Page break
                if (currentY + cellHeight > page.Height - MarginBottom)
                {
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    tf = new XTextFormatter(gfx);

                    // Redraw table header if needed
                    DrawHeader(out headerBottom, gfx, page, project.Name, marginLeft, marginRight);
                    DrawFooter(gfx, page, project.UserName, document.PageCount);

                    currentY = headerBottom + 2 * Padding;
                }
            }

            // TODO: parallelisieren?
            foreach (var element in project.Elements)
            {
                AddElementPage(element, document);
            }

            var pdfFilePath = SaveDoc(document, $"Bauteilkatalog_{project.Name}");

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }

        private static string TruncateTextToWidth(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            if (gfx.MeasureString(text, font).Width <= maxWidth)
                return text;

            const string ellipsis = "...";
            double ellipsisWidth = gfx.MeasureString(ellipsis, font).Width;

            int len = text.Length;
            while (len > 0)
            {
                string sub = text.Substring(0, len);
                if (gfx.MeasureString(sub, font).Width + ellipsisWidth <= maxWidth)
                    return sub + ellipsis;
                len--;
            }

            return ellipsis; // if nothing fits
        }

        public static void CreateSingleElementDocument(Element? element)
        {
            if (element is null) return;

            element.RenderOffscreenImage(target: RenderTarget.Document, withDecorations: true);

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Overview of {element.Name}";

            AddElementPage(element, document);

            var pdfFilePath = SaveDoc(document, element.Name);

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }

        #region private Methods
        
        private static string SaveDoc(PdfDocument document, string fileName)
        {
            // Save the document
            string downloadFolder = PathService.DownloadsFolderPath;
            string pdfFilePath = Path.Combine(downloadFolder, "BauphysikTool_export.pdf");
            if (TextInputValidation.IsValidWindowsFileName($"{fileName}.pdf"))
            {
                pdfFilePath = Path.Combine(downloadFolder, $"{fileName}.pdf");
            }
            document.Save(pdfFilePath);

            return pdfFilePath;
        }

        private static void AddElementPage(Element element, PdfDocument document)
        {
            var project = element.ParentProject;

            XFont titleFont = new XFont(FontFamilyRegular, 10, XFontStyleEx.Bold);
            XFont titleFontSm = new XFont(FontFamilyRegular, 9, XFontStyleEx.Bold);
            XFont bodyFont = new XFont(FontFamilyRegular, 9, XFontStyleEx.Regular);
            XFont bodyFontItalic = new XFont(FontFamilyRegular, 9, XFontStyleEx.Italic);
            XFont bodyFontBold = new XFont(FontFamilyRegular, 9, XFontStyleEx.Bold);
            XFont tableHeaderFont = new XFont(FontFamilyRegular, 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont(FontFamilyForData, 8, XFontStyleEx.Regular);
            XFont tableBodyFontBold = new XFont(FontFamilyForData, 8, XFontStyleEx.Bold);

            // Add a page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            var (marginLeft, marginRight) = GetPageMargins(document.PageCount);
            
            // Draw Header and Footer
            DrawHeader(out double headerBottom, gfx, page, project.Name, marginLeft, marginRight);
            DrawFooter(gfx, page, project.UserName, document.PageCount);

            double startX = marginLeft;
            double startY = headerBottom + 2 * Padding;
            double contentWidth = page.Width - marginLeft - marginRight;
            double contentHeight = page.Height - MarginTop - MarginBottom;

            #region Title, Caption and Color Circle

            double circleDiameter = 0;
            if (element.Color != Colors.Transparent)
            {
                // Kreis rechts neben dem Titel
                circleDiameter = 10; // z. B. 12pt
                double circleX = startX; // 5pt Abstand vom Rand
                double circleY = startY; // vertikal mittig
                Color mediaColor = Colors.Transparent;
                try { mediaColor = element.Color; } catch { }
                // Convert to PdfSharp XColor
                XColor fillColor = XColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
                gfx.DrawEllipse(new XSolidBrush(fillColor), circleX, circleY, circleDiameter, circleDiameter);
            }
            var titleX = circleDiameter > 0 ? startX + circleDiameter + Padding : startX;
            var titleRect = new XRect(titleX, startY, contentWidth / 2, titleFont.GetHeight());
            gfx.DrawString($"{element.ShortName} | {element.Name}", titleFont, XBrushes.Black, titleRect, XStringFormats.TopLeft);
            startY += RowHeight;

            gfx.DrawString($"Bauteiltyp: {element.Construction.TypeName}", bodyFontItalic, XBrushes.Black,
                new XRect(startX, startY, contentWidth / 2, bodyFontItalic.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight + Padding;

            #endregion

            var startRightBlockTop = startY;

            #region Draw Element Properties

            var text = "";
            var textWidth = 0.0;
            var maxTextWidth = textWidth;
            var infoAsterisk = "";

            // U-Value
            gfx.DrawString("U", bodyFontBold, XBrushes.Black, new XRect(startX, startY, startX + 28, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            infoAsterisk = Math.Abs(element.UValueUserDef - element.UValue) > 1E-04 ? " *" : "";
            text = $"= {element.UValueUserDef:0.000} W/m²K{infoAsterisk}";
            textWidth = gfx.MeasureString(text, bodyFontBold).Width;
            gfx.DrawString(text, bodyFontBold, XBrushes.Black, new XRect(startX + 28, startY, textWidth, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            maxTextWidth = Math.Max(maxTextWidth, textWidth);
            startY += RowHeight;

            // R-ges
            gfx.DrawString("Rges", bodyFontBold, XBrushes.Black, new XRect(startX, startY, startX + 28, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            infoAsterisk = Math.Abs(element.RGesValueUserDef - element.RGesValue) > 1E-04 ? " *" : "";
            text = $"= {element.RGesValueUserDef:0.00} m²K/W{infoAsterisk}";
            textWidth = gfx.MeasureString(text, bodyFontBold).Width;
            gfx.DrawString(text, bodyFontBold, XBrushes.Black, new XRect(startX + 28, startY, textWidth, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            maxTextWidth = Math.Max(maxTextWidth, textWidth);
            startY += RowHeight + Padding;
            
            // R-T
            gfx.DrawString("RT", bodyFont, XBrushes.Black, new XRect(startX, startY, startX + 28, bodyFont.GetHeight()), XStringFormats.TopLeft);
            infoAsterisk = Math.Abs(element.RTotValueUserDef - element.RTotValue) > 1E-04 ? " *" : "";
            text = $"= {element.RTotValueUserDef:0.00} m²K/W (inkl. Übergangswiderstände){infoAsterisk}";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX + 28, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            //maxTextWidth = Math.Max(maxTextWidth, textWidth);
            startY += RowHeight;

            // m'
            gfx.DrawString("m'", bodyFont, XBrushes.Black, new XRect(startX, startY, startX + 28, bodyFont.GetHeight()), XStringFormats.TopLeft);
            infoAsterisk = Math.Abs(element.AreaMassDensUserDef - element.AreaMassDens) > 1E-04 ? " *" : "";
            text = $"= {element.AreaMassDensUserDef:0.00} kg/m²{infoAsterisk}";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX + 28, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            //maxTextWidth = Math.Max(maxTextWidth, textWidth);
            startY += RowHeight;

            // sd
            gfx.DrawString("sd", bodyFont, XBrushes.Black, new XRect(startX, startY, startX + 28, bodyFont.GetHeight()), XStringFormats.TopLeft);
            infoAsterisk = Math.Abs(element.SdThicknessUserDef - element.SdThickness) > 1E-04 ? " *" : "";
            text = $"= {element.SdThicknessUserDef:0.0} m{infoAsterisk}";
            textWidth = gfx.MeasureString(text, bodyFont).Width;
            gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX + 28, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            //maxTextWidth = Math.Max(maxTextWidth, textWidth);
            startY += RowHeight + Padding;


            //
            text = element.IsInhomogeneous ? "Ja" : "Nein";
            gfx.DrawString($"Inhomogener Schichtaufbau: {text}", bodyFont, XBrushes.Black,
                new XRect(startX, startY, contentWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            gfx.DrawString($"Bauteil zuletzt geändert: {element.UpdatedAtString}", bodyFont, XBrushes.Black,
                new XRect(startX, startY, contentWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            #endregion

            var startRightBlockLeft = marginLeft + 28 + maxTextWidth + Padding;

            var color = element.Requirements.IsUValueOk ? XBrushes.DarkGreen : XBrushes.Red;
            gfx.DrawString($"→ {element.Requirements.UMaxCaption}:", bodyFont, XBrushes.Black,
                new XRect(startRightBlockLeft, startRightBlockTop, contentWidth / 3 + 3 * Padding, bodyFont.GetHeight()), XStringFormats.TopLeft);
            gfx.DrawString($"U ≤ {element.Requirements.UMax:0.000} W/m²K", bodyFontBold, color,
                new XRect(startRightBlockLeft + contentWidth / 3 + 3 * Padding, startRightBlockTop, marginLeft + 32, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;

            color = element.Requirements.IsRValueOk ? XBrushes.DarkGreen : XBrushes.Red;
            gfx.DrawString($"→ {element.Requirements.RMinCaption}:", bodyFont, XBrushes.Black,
                new XRect(startRightBlockLeft, startRightBlockTop, contentWidth / 3 + 3 * Padding, bodyFont.GetHeight()), XStringFormats.TopLeft);
            gfx.DrawString($"R ≥ {element.Requirements.RMin:0.00} m²K/W", bodyFontBold, color,
                new XRect(startRightBlockLeft + contentWidth / 3 + 3 * Padding, startRightBlockTop, marginLeft + 32, bodyFontBold.GetHeight()), XStringFormats.TopLeft);
            startRightBlockTop += RowHeight;


            if (element.Comment != string.Empty)
            {
                text = "Kommentar: ";
                textWidth = gfx.MeasureString(text, bodyFont).Width;
                gfx.DrawString(text, bodyFont, XBrushes.Black, new XRect(startX, startY, textWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
                var textBlockHeight = DrawWrappedText(gfx, $"\"{element.Comment}\"", bodyFont, XBrushes.Black,
                    new XRect(marginLeft + textWidth, startY, contentWidth - textWidth, 80), bodyFont.GetHeight());
                startY += textBlockHeight;
            }

            if (element.IsUserDefValuesEnabled)
            {
 
                startY += Padding; // extra space
                gfx.DrawString("* Hinweis: frei eingegebener Wert, der nicht aus der Berechnung stammt.", bodyFontItalic, XBrushes.Gray,
                    new XRect(startX, startY, contentWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
                startY += RowHeight;
            }

            // Draw Layer Information
            startY += Padding; // extra space
            gfx.DrawString("Querschnitt", titleFontSm, XBrushes.Black,
                new XRect(startX, startY, contentWidth, titleFontSm.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            gfx.DrawString("von innen nach außen", bodyFont, XBrushes.Black,
                new XRect(startX, startY, contentWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            #region Image

            // Draw image from Image property

            var imgToDraw = element.Image;
            if (imgToDraw.Length == 0)
            {
                // No image available, skip drawing
                startY += RowHeight;
                gfx.DrawString("Keine Abbildung verfügbar", bodyFontItalic, XBrushes.Black,
                    new XRect(0, startY, page.Width, bodyFontItalic.GetHeight()), XStringFormats.TopCenter);
                startY += RowHeight;
            }
            else
            {
                double imageHeight = DrawScaledImage(
                    gfx,
                    imgToDraw,
                    marginLeft,
                    startY,
                    contentWidth,   // max width
                    6.0,            // target height in cm
                    240.0           // render DPI of offscreen renderer // TODO:
                );
                startY += imageHeight;
            }

            #endregion

            // Draw Layer Information
            startY += Padding; // extra space
            gfx.DrawString("Schichtaufbau", titleFontSm, XBrushes.Black,
                new XRect(startX, startY, contentWidth, titleFontSm.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            gfx.DrawString("von innen nach außen", bodyFont, XBrushes.Black,
                new XRect(startX, startY, contentWidth, bodyFont.GetHeight()), XStringFormats.TopLeft);
            startY += RowHeight;

            #region Table

            string[] headers = { "Schicht", "d [cm]", "ρ [kg/m³]", "m' [kg/m²]", "λ [W/mK]", "R [m²K/W]" };
            string[,] data = GetLayerData(element);

            // Draw table headers
            double cellHeight = RowHeight;
            double[] columnWidths = { 200, 48, 60, 60, 60, 60 };

            double currentY = startY;
            double currentX = startX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, new XRect(currentX, currentY, columnWidths[i], cellHeight));
                gfx.DrawString(headers[i], tableHeaderFont, XBrushes.Black, new XRect(currentX + 2, currentY + 3, columnWidths[i], cellHeight), XStringFormats.TopLeft);
                currentX += columnWidths[i];
            }

            // Draw table data
            currentY += cellHeight;
            for (int row = 0; row < data.GetLength(0); row++)
            {
                var txtColor = data[row, 0][2] == 'b' ? XBrushes.DarkGray : XBrushes.Black;
                currentX = startX;
                for (int col = 0; col < data.GetLength(1); col++)
                {
                    //gfx.DrawRectangle(XPens.Black, new XRect(currentX, currentY, columnWidths[col], cellHeight));
                    gfx.DrawString(data[row, col], tableBodyFont, txtColor, new XRect(currentX + 2, currentY + 3, columnWidths[col], cellHeight), XStringFormats.TopLeft);
                    currentX += columnWidths[col];
                }
                currentY += cellHeight;
            }

            // Draw a thick line below the table
            XPen thickPen = new XPen(XColors.Black, 1);
            gfx.DrawLine(thickPen, startX, currentY, startX + columnWidths.Sum(), currentY);

            // Add the sum row below the thick line
            currentY += 5; // Space between line and sum row
            string[] sumValues = { "Summe", element.Thickness.ToString("F2"), "", element.AreaMassDens.ToString("F1"), "", element.RTotValue.ToString("F2") }; // Hardcoded values
            currentX = startX;
            for (int col = 0; col < sumValues.Length; col++)
            {
                //gfx.DrawRectangle(XPens.Black, new XRect(currentX, currentY, columnWidths[col], cellHeight));
                gfx.DrawString(sumValues[col], tableBodyFontBold, XBrushes.Black, new XRect(currentX + 2, currentY + 3, columnWidths[col], cellHeight), XStringFormats.TopLeft);
                currentX += columnWidths[col];
            }

            #endregion
        }

        private static string[,] GetLayerData(Element element)
        {
            var layers = element.Layers;
            var rows = layers.Count + layers.Count(l => l.SubConstruction != null) + 2;
            string[,] data = new string[rows, 6]; // Zeilen, Spalten

            int currentRow = 0;
            data[currentRow, 0] = "Rsi";
            data[currentRow, 1] = "";
            data[currentRow, 2] = "";
            data[currentRow, 3] = "";
            data[currentRow, 4] = "";
            data[currentRow, 5] = element.ThermalResults.Rsi.ToString("N", CultureInfo.CurrentCulture);
            currentRow += 1;
            for (int i = 0; i < layers.Count; i++)
            {
                string numberLayer = i+1 < 10 ? $"0{i + 1}" : $"{i + 1}";
                data[currentRow, 0] = $"{numberLayer} - {layers[i].Material.Name}";
                data[currentRow, 1] = layers[i].Thickness.ToString("F2");
                data[currentRow, 2] = layers[i].Material.BulkDensity.ToString("F0");
                data[currentRow, 3] = layers[i].AreaMassDensity.ToString("F1");
                data[currentRow, 4] = layers[i].Material.ThermalConductivity.ToString("F3");
                data[currentRow, 5] = layers[i].R_Value.ToString("F2");
                currentRow += 1;

                if (layers[i].SubConstruction != null)
                {
                    string numberSubC = i+1 < 10 ? $"0{i + 1}b" : $"{i + 1}b";
                    data[currentRow, 0] = $"{numberSubC} - {layers[i].SubConstruction?.Material.Name}";
                    data[currentRow, 1] = layers[i].SubConstruction?.Thickness.ToString("F2") ?? "";
                    data[currentRow, 2] = layers[i].SubConstruction?.Material.BulkDensity.ToString("F0") ?? "";
                    data[currentRow, 3] = layers[i].SubConstruction?.AreaMassDensity.ToString("F1") ?? "";
                    data[currentRow, 4] = layers[i].SubConstruction?.Material.ThermalConductivity.ToString("F3") ?? "";
                    data[currentRow, 5] = layers[i].SubConstruction?.R_Value.ToString("F2") ?? "";
                    currentRow += 1;
                }
            }
            data[currentRow, 0] = "Rse";
            data[currentRow, 1] = "";
            data[currentRow, 2] = "";
            data[currentRow, 3] = "";
            data[currentRow, 4] = "";
            data[currentRow, 5] = element.ThermalResults.Rse.ToString("N", CultureInfo.CurrentCulture);

            return data;
        }

        private static void DrawFooter(XGraphics gfx, PdfPage page, string author, int pageNumber)
        {
            XFont footerFont = new XFont(FontFamilyRegular, 8, XFontStyleEx.Regular);
            gfx.DrawString($"Bearbeiter: {author} | Seite {pageNumber}", footerFont, XBrushes.Gray,
                new XRect(0, page.Height - MarginBottom, page.Width, footerFont.GetHeight()), XStringFormats.TopCenter);
        }

        private static void DrawHeader(out double bottomY, XGraphics gfx, PdfPage page, string projectName, int marginLeft, int marginRight)
        {
            XFont headerFont = new XFont(FontFamilyRegular, 8, XFontStyleEx.Regular);

            // Linke Spalte (3 Zeilen, linksbündig)
            string[] leftLines =
            {
                $"Bauphysik Tool {UpdaterManager.LocalUpdaterManagerFile.CurrentTag}",
                "https://bauphysik-tool.de",
                ""
            };

            // Rechte Spalte (3 Zeilen, rechtsbündig)
            string[] rightLines =
            {
                $"Projekt: {projectName}",
                $"Erzeugt am: {DateTime.Now.ToString("dd.MM.yyyy")}",
                ""
            };

            double contentWidth = page.Width - marginLeft - marginRight;
            double lineHeight = (int)headerFont.GetHeight();
            double currentY = MarginTop;

            // Linke Seite zeichnen
            foreach (var line in leftLines)
            {
                gfx.DrawString(line, headerFont, XBrushes.Black,
                    new XRect(marginLeft, currentY, contentWidth / 2, lineHeight), XStringFormats.TopLeft);
                currentY += lineHeight;
            }

            // Rechte Seite zeichnen
            currentY = MarginTop;
            foreach (var line in rightLines)
            {
                gfx.DrawString(line, headerFont, XBrushes.Black,
                    new XRect(0, currentY, page.Width - marginRight, lineHeight), XStringFormats.TopRight);
                currentY += lineHeight;
            }

            // Linie unten durchgehend
            bottomY = MarginTop + lineHeight * 3 + 2; // unterhalb der 3 Zeilen
            gfx.DrawLine(new XPen(XColors.Black, 1.0), marginLeft, bottomY, page.Width - marginRight, bottomY);
        }
        
        private static double DrawWrappedText(XGraphics gfx, string text, XFont font, XBrush brush, XRect rect, double lineHeight)
        {
            // Split the text into words
            string[] words = text.Split(' ');
            string currentLine = string.Empty;
            double currentY = rect.Y;
            double totalHeight = 0;

            foreach (var word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                double testWidth = gfx.MeasureString(testLine, font).Width;

                // If the line width exceeds the rect width, draw the current line and start a new one
                if (testWidth > rect.Width)
                {
                    gfx.DrawString(currentLine, font, brush, new XRect(rect.X, currentY, rect.Width, lineHeight), XStringFormats.TopLeft);
                    currentLine = word;
                    currentY += lineHeight;
                    totalHeight += lineHeight;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            // Draw the last line
            if (!string.IsNullOrEmpty(currentLine))
            {
                gfx.DrawString(currentLine, font, brush, new XRect(rect.X, currentY, rect.Width, lineHeight), XStringFormats.TopLeft);
                totalHeight += lineHeight;
            }

            // Return the total height of the wrapped text block
            return totalHeight;
        }

        private static (int left, int right) GetPageMargins(int pageNumber)
        {
            // Odd page → larger left margin
            if (pageNumber % 2 == 1)
                return (MarginInner, MarginOuter);
            // Even page → larger right margin
            else
                return (MarginOuter, MarginInner);
        }

        private static double DrawScaledImage(
            XGraphics gfx,
            byte[] imgBytes,
            double x, double y,
            double maxWidthPt,
            double targetHeightCm,
            double renderDpi,
            double padding = 0)
        {
            double imageHeight = 0;

            using (MemoryStream ms = new MemoryStream(imgBytes, 0, imgBytes.Length, false, true))
            {
                ms.Position = 0;
                using (XImage image = XImage.FromStream(ms))
                {
                    // Convert pixels -> points based on render DPI
                    double widthPt = image.PixelWidth * 72.0 / renderDpi;
                    double heightPt = image.PixelHeight * 72.0 / renderDpi;

                    // Target height in points
                    double targetHeightPt = targetHeightCm * 72.0 / 2.54; // cm → pt

                    // Scale to target height
                    double scale = targetHeightPt / heightPt;
                    double scaledWidth = widthPt * scale;
                    double scaledHeight = heightPt * scale;

                    // Clamp by maximum width
                    if (scaledWidth > maxWidthPt)
                    {
                        scale = maxWidthPt / widthPt;
                        scaledWidth = widthPt * scale;
                        scaledHeight = heightPt * scale;
                    }
                    // Draw centered
                    double xPos = x + (maxWidthPt - scaledWidth) / 2.0;
                    gfx.DrawImage(image, xPos, y, scaledWidth, scaledHeight);
                    // Draw Left-aligned
                    //gfx.DrawImage(image, x, y, scaledWidth, scaledHeight);

                    imageHeight = scaledHeight + padding;
                }
            }

            return imageHeight; // return how much vertical space was consumed
        }

        #endregion
    }
}
