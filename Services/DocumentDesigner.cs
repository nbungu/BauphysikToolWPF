﻿using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using BauphysikToolWPF.Repository.Models;

namespace BauphysikToolWPF.Services
{
    public class DocumentDesigner
    {
        public static void FullCatalogueExport(Project project)
        {
            XFont titleFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            //XFont titleFontSm = new XFont("Verdana", 9, XFontStyleEx.Bold);
            //XFont bodyFont = new XFont("Verdana", 9, XFontStyleEx.Regular);
            XFont tableHeaderFont = new XFont("Verdana", 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont("Verdana", 8, XFontStyleEx.Regular);

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Elements Table";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Draw title
            gfx.DrawString("5. Übersichtsliste: U- und R-Werte aller Bauteile", titleFont, XBrushes.Black,
                new XRect(0, 20, page.Width, 30),
                XStringFormats.TopCenter);

            // Table data
            string[] headers = { "Bauteil-nummer", "Kategorie", "Bauteilbezeichnung", "U-Wert", "GEG W/(m²K)", "Wärmedurchlasswiderstand R (m²K/W)", "Ist-Wert", "Soll-Wert", "DIN 4108-2" };
            string[,] data = {
                { "FD1", "Flachdach", "Decke über 2.und 3.OG", "0,118", "", "R = 8,32", "R = 8,32", "R ≥ 1,20", "" },
                { "FD2", "Flachdach", "Terrasse Haus 1", "0,184", "", "R = 5,28", "R = 5,28", "R ≥ 1,20", "" },
                { "AW1", "Außenwand", "", "0,179", "", "R = 5,42", "R = 5,42", "R ≥ 1,20", "" },
                { "AW2", "Außenwand gegen TG", "", "0,272", "", "R = 3,51", "R = 3,51", "R ≥ 1,20", "" },
                // Add more rows as needed...
            };

            // Define starting point and table layout
            double startX = 40;
            double startY = 70;
            double cellHeight = 20;
            double[] columnWidths = { 60, 80, 140, 50, 50, 80, 60, 60, 60 };

            // Draw table headers
            double currentX = startX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, new XRect(currentX, startY, columnWidths[i], cellHeight));
                gfx.DrawString(headers[i], tableHeaderFont, XBrushes.Black, new XRect(currentX + 2, startY + 3, columnWidths[i], cellHeight), XStringFormats.TopLeft);
                currentX += columnWidths[i];
            }

            // Draw table data
            double currentY = startY + cellHeight;
            for (int row = 0; row < data.GetLength(0); row++)
            {
                currentX = startX;
                for (int col = 0; col < data.GetLength(1); col++)
                {
                    gfx.DrawRectangle(XPens.Black, new XRect(currentX, currentY, columnWidths[col], cellHeight));
                    gfx.DrawString(data[row, col], tableBodyFont, XBrushes.Black, new XRect(currentX + 2, currentY + 3, columnWidths[col], cellHeight), XStringFormats.TopLeft);
                    currentX += columnWidths[col];
                }
                currentY += cellHeight;
            }

            foreach (var element in project.Elements)
            {
                AddElementPage(element, document);
            }

            var pdfFilePath = SaveDoc(document, $"Bauteilkatalog_{project.Name}");

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }

        public static void CreateSingleElementDocument(Element element)
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Overview of {element.Name}";

            AddElementPage(element, document);

            var pdfFilePath = SaveDoc(document, element.Name);

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }
        
        private static string SaveDoc(PdfDocument document, string fileName)
        {
            // Save the document
            string downloadFolder = ApplicationServices.DownloadsFolderPath;
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
            XFont titleFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            XFont titleFontSm = new XFont("Verdana", 9, XFontStyleEx.Bold);
            XFont bodyFont = new XFont("Verdana", 9, XFontStyleEx.Regular);
            //XFont bodyFontBold = new XFont("Verdana", 9, XFontStyleEx.Bold);
            XFont tableHeaderFont = new XFont("Verdana", 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont("Verdana", 8, XFontStyleEx.Regular);

            // Add a page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Draw Header and Footer
            DrawHeader(gfx, page, Session.SelectedProject.Name, DateTime.Now.ToString("yyyy-MM-dd"));
            DrawFooter(gfx, page, Session.SelectedProject.UserName, document.PageCount);

            double startY = 50;

            // Draw title
            gfx.DrawString($"Bauteil: {element.Name}", titleFont, XBrushes.Black,
                new XRect(new XUnitPt(50), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(30)), XStringFormats.TopLeft);
            startY += 16;

            gfx.DrawString($"Bauteiltyp: \"{element.Construction.TypeName}\"", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(50), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 32;

            // Draw Element Properties
            gfx.DrawString($"Rges = {element.RGesValue:0.00} m²K/W (nur Bauteil)", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            gfx.DrawString($"RT = {element.RTotValue:0.00} m²K/W", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            gfx.DrawString($"U = {element.UValue:0.00} W/m²K", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            gfx.DrawString($"q = {element.QValue:0.00} W/m²", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 24;

            gfx.DrawString($"Ausrichtung: {element.OrientationType}", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;
            var str = element.Layers.Any(l => l.HasSubConstructions) ? "Ja" : "Nein";
            gfx.DrawString($"Inhomogener Schichtaufbau: {str}", bodyFont, XBrushes.Black,
                new XRect(new XUnitPt(70), new XUnitPt(startY),
                    new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
            startY += 16;

            if (element.Comment != string.Empty)
            {
                gfx.DrawString($"Kommentar:", bodyFont, XBrushes.Black,
                    new XRect(new XUnitPt(70), new XUnitPt(startY),
                        new XUnitPt(page.Width - 100), new XUnitPt(20)), XStringFormats.TopLeft);
                var textBlockHeight = DrawWrappedText(gfx, element.Comment, bodyFont, XBrushes.Black,
                    new XRect(new XUnitPt(130), new XUnitPt(startY),
                        new XUnitPt(page.Width - 160), new XUnitPt(80)), bodyFont.GetHeight());
                startY += textBlockHeight + 16;
            }

            // Draw Layer Information
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
            double imageHeight;

            var imgToDraw = element.DocumentImage;
            // Use small image if big one is not available
            if (imgToDraw.Length == 0) imgToDraw = element.Image;

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
            
            startY += imageHeight + 20;

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
                gfx.DrawString(sumValues[col], tableBodyFont, XBrushes.Black, new XRect(currentX + 2, currentY + 3, columnWidths[col], cellHeight), XStringFormats.TopLeft);
                currentX += columnWidths[col];
            }

            #endregion
        }

        private static string[,] GetLayerData(Element element)
        {
            var layers = element.Layers;
            var rows = layers.Count + layers.Count(l => l.HasSubConstructions) + 2;
            string[,] data = new string[rows, 6]; // Zeilen, Spalten

            int currentRow = 0;
            data[currentRow, 0] = "Rsi";
            data[currentRow, 1] = "";
            data[currentRow, 2] = "";
            data[currentRow, 3] = "";
            data[currentRow, 4] = "";
            data[currentRow, 5] = element.UsedEnvVars[0].Value.ToString("N", CultureInfo.CurrentCulture);
            currentRow += 1;
            for (int i = 0; i < layers.Count; i++)
            {
                data[currentRow, 0] = $"0{i+1} - {layers[i].Material.Name}";
                data[currentRow, 1] = layers[i].Thickness.ToString("F2");
                data[currentRow, 2] = layers[i].Material.BulkDensity.ToString("F0");
                data[currentRow, 3] = layers[i].AreaMassDensity.ToString("F1");
                data[currentRow, 4] = layers[i].Material.ThermalConductivity.ToString("F3");
                data[currentRow, 5] = layers[i].R_Value.ToString("F2");
                currentRow += 1;

                if (layers[i].HasSubConstructions)
                {
                    data[currentRow, 0] = $"0{i + 1}b - {layers[i].SubConstruction?.Material.Name}";
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
            data[currentRow, 5] = element.UsedEnvVars[1].Value.ToString("N", CultureInfo.CurrentCulture);

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
    }
}
