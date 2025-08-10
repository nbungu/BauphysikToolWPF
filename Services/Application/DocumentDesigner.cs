using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.UI;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Services.Application
{
    public class DocumentDesigner
    {
        public static void FullCatalogueExport(Project? project)
        {
            if (project == null) return;

            
            XFont titleFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            //XFont titleFontSm = new XFont("Verdana", 9, XFontStyleEx.Bold);
            //XFont bodyFont = new XFont("Verdana", 9, XFontStyleEx.Regular);
            XFont tableHeaderFont = new XFont("Verdana", 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont("Verdana", 7, XFontStyleEx.Regular);
            XFont tableBodyFont2 = new XFont("Verdana", 8, XFontStyleEx.Bold);

            var thinPen = new XPen(XColors.Black, 0.5); // 0.5pt line

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Elements Table";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Draw title
            gfx.DrawString("1. Übersichtsliste: U- und R-Werte aller Bauteile", titleFont, XBrushes.Black,
                new XRect(0, 20, page.Width, 30),
                XStringFormats.TopCenter);

            project.SortElements(ElementSortingType.TypeNameAscending);
            var elements = project.Elements;

            // Define starting point and table layout
            double startX = 40;
            double startY = 70;
            double cellHeight = 20;
            double maxWidth = page.Width - startX * 2;

            var tf = new XTextFormatter(gfx);

            // New headers and adjusted column widths
            string[] headers = { "Nr.", "Bauteilbezeichnung", "", "Kategorie", "U-Wert\nGEG\n[W/(m²K)]", "Wärmedurchlasswiderstand\nR [m²K/W]", "" };
            string[] secondRowHeaders = { "", "", "", "", "", "Ist-Wert", "Soll-Wert\nDIN 4108-2" }; // New row header for Wärmedurchlasswiderstand

            double[] columnProportions = { 28, 24, 140, 120, 56, 64, 64 }; // Original widths
            double totalWeight = columnProportions.Sum();
            double[] columnWidths = columnProportions.Select(p => p / totalWeight * maxWidth).ToArray();

            // Adjust data table size to match new header count
            string[,] data = new string[elements.Count, secondRowHeaders.Length];

            for (int i = 0; i < elements.Count; i++)
            {
                var el = elements[i];
                data[i, 0] = el.ShortName;
                data[i, 1] = ""; // Color visual only
                data[i, 2] = el.Name; // Bauteilbezeichnung
                data[i, 3] = el.Construction.TypeName; // Kategorie
                data[i, 4] = el.UValue.ToString("F3", CultureInfo.CurrentCulture);
                data[i, 5] = $"R = {el.RTotValue.ToString("F2", CultureInfo.CurrentCulture)}";
                data[i, 6] = "R ≥ 1,20";
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
                if (secondRowHeaders[i] == ""){
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
            double currentY = startY + (cellHeight * 3); // Move below both header rows
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
                        System.Windows.Media.Color mediaColor = System.Windows.Media.Colors.Transparent;
                        try { mediaColor = elements[row].Color; } catch { }

                        // Convert to PdfSharp XColor
                        XColor fillColor = XColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);

                        gfx.DrawRectangle(new XSolidBrush(fillColor), new XRect(currentX+0.5, currentY+0.5, columnWidths[col]-1, rowHeight-1));
                    }
                    else
                    {
                        var contentRect = new XRect(currentX + 4, currentY + 4, columnWidths[col] - 8, rowHeight - 8);
                        if (col == 4) // U-Wert hervorheben mit anderer Font
                        {
                            tf.DrawString(data[row, col], tableBodyFont2, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                        }
                        else
                        {
                            tf.DrawString(data[row, col], tableBodyFont, XBrushes.Black, contentRect, XStringFormats.TopLeft);
                        }
                        
                    }

                    currentX += columnWidths[col];
                }

                currentY += rowHeight;

                // Page break
                if (currentY + cellHeight > page.Height - 50)
                {
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    currentY = startY;
                }
            }

            foreach (var element in project.Elements)
            {
                AddElementPage(element, document);
            }

            var pdfFilePath = SaveDoc(document, $"Bauteilkatalog_{project.Name}");

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }

        public static void CreateSingleElementDocument(Element? element)
        {
            if (element is null) return;

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

            XFont titleFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            XFont titleFontSm = new XFont("Verdana", 9, XFontStyleEx.Bold);
            XFont bodyFont = new XFont("Verdana", 9, XFontStyleEx.Regular);
            XFont bodyFontItalic = new XFont("Verdana", 9, XFontStyleEx.Italic);
            XFont bodyFontBold = new XFont("Verdana", 9, XFontStyleEx.Bold);
            XFont tableHeaderFont = new XFont("Verdana", 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont("Verdana", 8, XFontStyleEx.Regular);
            XFont tableBodyFontBold = new XFont("Verdana", 8, XFontStyleEx.Bold);

            // Add a page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Draw Header and Footer
            DrawHeader(gfx, page, project.Name, DateTime.Now.ToString("yyyy-MM-dd"));
            DrawFooter(gfx, page, project.UserName, document.PageCount);

            double startY = 50;

            // Draw title
            gfx.DrawString($"Bauteil: {element.Name}", titleFont, XBrushes.Black,
                new XRect(new XUnitPt(50), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(30)), XStringFormats.TopLeft);
            startY += 16;

            gfx.DrawString($"Bauteiltyp: \"{element.Construction.TypeName}\"", bodyFontItalic, XBrushes.Black,
                new XRect(new XUnitPt(50), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 32;

            // Draw Element Properties
            gfx.DrawString($"Rges = {element.RGesValue:0.00} m²K/W (nur Bauteil)", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            gfx.DrawString($"RT = {element.RTotValue:0.00} m²K/W (inkl. Übergangswiderstände)", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            gfx.DrawString($"U = {element.UValue:0.000} W/m²K", bodyFontBold, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            gfx.DrawString($"m' = {element.AreaMassDens:0.00} kg/m²", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 24;

            gfx.DrawString($"Ausrichtung: {element.OrientationTypeName}", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            var str = element.IsInhomogeneous ? "Ja" : "Nein";
            gfx.DrawString($"Inhomogener Schichtaufbau: {str}", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;

            if (element.Comment != string.Empty)
            {
                gfx.DrawString($"Kommentar:", bodyFont, XBrushes.Black,
                    new XRect(new XUnitPt(70), new XUnitPt(startY),
                        new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
                var textBlockHeight = DrawWrappedText(gfx, $"\"{element.Comment}\"", bodyFont, XBrushes.Black,
                    new XRect(new XUnitPt(130), new XUnitPt(startY),
                        new XUnitPt(page.Width - 160), new XUnitPt(80)), bodyFont.GetHeight());
                startY += textBlockHeight + 16;
            }

            // Draw Layer Information
            startY += 8; // Extra padding
            gfx.DrawString("Querschnitt", titleFontSm, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(30)), XStringFormats.TopLeft);
            startY += 16;

            gfx.DrawString("von innen nach außen", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;

            #region Image

            // Draw image from Image property

            var imgToDraw = element.Image;

            if (imgToDraw.Length == 0)
            {
                // No image available, skip drawing
                startY += 16;
                gfx.DrawString("Keine Abbildung verfügbar", bodyFontItalic, XBrushes.Black,
                    new XRect(new XUnitPt(70 + 35), new XUnitPt(startY),
                        new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
                startY += 32;
            }
            else
            {
                double imageHeight;
                using (MemoryStream ms = new MemoryStream(imgToDraw, 0, imgToDraw.Length, false, true))
                {
                    ms.Position = 0; // Ensure the stream position is at the start
                    XImage image = XImage.FromStream(ms);

                    // Get the original width and height of the image
                    double originalWidth = image.PixelWidth;
                    double originalHeight = image.PixelHeight;

                    // Define the maximum dimensions based on page size and desired margins
                    double maxWidth = page.Width - 100; // Leave 50 units of margin on each side
                    double maxHeight = page.Height - 200; // Leave 100 units of margin at top and bottom

                    // Calculate the scaling factor
                    double scale = Math.Min(maxWidth / originalWidth, maxHeight / originalHeight);

                    // Scale the width and height based on the scaling factor
                    double scaledWidth = originalWidth * scale;
                    double scaledHeight = originalHeight * scale;

                    // Draw the image with scaled dimensions
                    gfx.DrawImage(image, 70, startY, scaledWidth, scaledHeight);

                    // Update imageHeight for subsequent content positioning
                    imageHeight = scaledHeight + 10; // Add margin below the image
                }
                startY += imageHeight + 8;
            }
            
            #endregion

            // Draw Layer Information
            gfx.DrawString("Schichtaufbau", titleFontSm, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(30)), XStringFormats.TopLeft);
            startY += 16;

            gfx.DrawString("von innen nach außen", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;

            #region Table

            string[] headers = { "Schicht", "d [cm]", "ρ [kg/m³]", "m' [kg/m²]", "λ [W/mK]", "R [m²K/W]" };
            string[,] data = GetLayerData(element);

            // Draw table headers
            double startX = 70;
            double cellHeight = 16;
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

        private static void DrawHeader(XGraphics gfx, PdfPage page, string projectName, string date)
        {
            XFont headerFont = new XFont("Verdana", 10, XFontStyleEx.Regular);
            gfx.DrawString($"Projekt: {projectName} | Datum: {date}", headerFont, XBrushes.Gray,
                new XRect(new XUnitPt(0), new XUnitPt(0),
                    new XUnitPt(page.Width), new XUnitPt(30)), XStringFormats.TopCenter);
        }

        private static void DrawFooter(XGraphics gfx, PdfPage page, string author, int pageNumber)
        {
            XFont footerFont = new XFont("Verdana", 8, XFontStyleEx.Regular);
            gfx.DrawString($"Bearbeiter: {author} | Seite {pageNumber}", footerFont, XBrushes.Gray,
                new XRect(new XUnitPt(0), new XUnitPt(page.Height - 30),
                    new XUnitPt(page.Width), new XUnitPt(30)), XStringFormats.TopCenter);
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

        #endregion
    }
}
