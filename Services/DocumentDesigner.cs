using BauphysikToolWPF.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using System.IO;

namespace BauphysikToolWPF.Services
{
    public class DocumentDesigner
    {
        public static void ElementList()
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Elements Table";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Create fonts
            XFont titleFont = new XFont("Verdana", 14, XFontStyleEx.Bold);
            XFont headerFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            XFont bodyFont = new XFont("Verdana", 10, XFontStyleEx.Regular);

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
                gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XRect(currentX + 2, startY + 3, columnWidths[i], cellHeight), XStringFormats.TopLeft);
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
                    gfx.DrawString(data[row, col], bodyFont, XBrushes.Black, new XRect(currentX + 2, currentY + 3, columnWidths[col], cellHeight), XStringFormats.TopLeft);
                    currentX += columnWidths[col];
                }
                currentY += cellHeight;
            }

            // Save the document
            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //%appdata%/BauphysikTool
            string appFolder = Path.Combine(programDataPath, "BauphysikTool");
            string pdfFilePath = Path.Combine(appFolder, "ProjectOverview.pdf");

            document.Save(pdfFilePath);

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }
        public static void SingleElementOverview(Element element)
        {
            if (element is null) return;

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Overview of {element.Name}";

            // Add a page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Draw Header and Footer
            DrawHeader(gfx, page, element.Project.Name, DateTime.Now.ToString("yyyy-MM-dd"));
            DrawFooter(gfx, page, element.Project.UserName, document.PageCount);

            // Define fonts
            XFont titleFont = new XFont("Verdana", 12, XFontStyleEx.Bold);
            XFont bodyFont = new XFont("Verdana", 10, XFontStyleEx.Regular);
            XFont tableHeaderFont = new XFont("Verdana", 9, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont("Verdana", 9, XFontStyleEx.Regular);

            // Draw title
            gfx.DrawString($"Element Overview: {element.Name}", titleFont, XBrushes.Black,
                new XRect(50, 50, page.Width - 100, 30), XStringFormats.TopLeft);

            // Draw Element Properties
            double startY = 80;
            gfx.DrawString($"Orientation: {element.OrientationType}", bodyFont, XBrushes.Black,
                new XRect(50, startY, page.Width - 100, 20), XStringFormats.TopLeft);

            gfx.DrawString($"R-Value: {element.RGesValue:0.00} m²K/W", bodyFont, XBrushes.Black,
                new XRect(50, startY + 20, page.Width - 100, 20), XStringFormats.TopLeft);

            gfx.DrawString($"U-Value: {element.UValue:0.00} W/m²K", bodyFont, XBrushes.Black,
                new XRect(50, startY + 40, page.Width - 100, 20), XStringFormats.TopLeft);

            gfx.DrawString($"Q-Value: {element.QValue:0.00} W/m²", bodyFont, XBrushes.Black,
                new XRect(50, startY + 60, page.Width - 100, 20), XStringFormats.TopLeft);

            gfx.DrawString($"Comment: {element.Comment}", bodyFont, XBrushes.Black,
                new XRect(50, startY + 80, page.Width - 100, 40), XStringFormats.TopLeft);

            // Draw image from Image property
            double imageHeight = 0;
            if (element.Image != null && element.Image.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(element.Image, 0, element.Image.Length,false, true))
                {
                    ms.Position = 0; // Ensure the stream position is at the start
                    XImage image = XImage.FromStream(ms);
                    imageHeight = 160;
                    gfx.DrawImage(image, 50, startY + 130, imageHeight * 2, imageHeight); // Adjust dimensions as needed
                    imageHeight += 10; // Adjust to give some margin below the image
                }
            }

            // Table data start position
            double tableStartY = startY + 150 + imageHeight;

            // Draw Layer Information
            gfx.DrawString("Layer Details", titleFont, XBrushes.Black,
                new XRect(50, tableStartY, page.Width - 100, 30), XStringFormats.TopLeft);

            string[] headers = { "Layer", "Thickness (cm)", "Density (kg/m³)", "λ (W/mK)", "R (m²K/W)" };
            string[,] data = GetLayerData(element);

            // Draw table headers
            double startX = 50;
            double cellHeight = 20;
            double[] columnWidths = { 100, 100, 100, 100, 100 };

            double currentY = tableStartY + 30;
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
                currentX = startX;
                for (int col = 0; col < data.GetLength(1); col++)
                {
                    gfx.DrawRectangle(XPens.Black, new XRect(currentX, currentY, columnWidths[col], cellHeight));
                    gfx.DrawString(data[row, col], tableBodyFont, XBrushes.Black, new XRect(currentX + 2, currentY + 3, columnWidths[col], cellHeight), XStringFormats.TopLeft);
                    currentX += columnWidths[col];
                }
                currentY += cellHeight;
            }

            // Save the document
            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //%appdata%/BauphysikTool
            string appFolder = Path.Combine(programDataPath, "BauphysikTool");
            string pdfFilePath = Path.Combine(appFolder, "ProjectOverview.pdf");

            document.Save(pdfFilePath);

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }
        public static void SingleElementOverview()
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Single Element Overview";

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Draw Header
            DrawHeader(gfx, page, "My Project Name", DateTime.Now.ToString("yyyy-MM-dd"));

            // Draw Footer
            DrawFooter(gfx, page, "Author Name", document.PageCount);

            // Define fonts
            XFont titleFont = new XFont("Verdana", 12, XFontStyleEx.Bold);
            XFont subtitleFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            XFont bodyFont = new XFont("Verdana", 9, XFontStyleEx.Regular);
            XFont tableHeaderFont = new XFont("Verdana", 8, XFontStyleEx.Bold);
            XFont tableBodyFont = new XFont("Verdana", 8, XFontStyleEx.Regular);

            // Draw the title
            gfx.DrawString("Anlage 1: U – Wert Ermittlung der Bauteile mit Nachweis nach DIN 4108-2 und -3 [5]", titleFont, XBrushes.Black,
                new XRect(0, 20, page.Width, 20),
                XStringFormats.TopCenter);

            // Draw subtitle
            gfx.DrawString("6.1.2 Bauteil: FD2 Flachdach Terrasse Haus 1", subtitleFont, XBrushes.Black,
                new XRect(50, 50, page.Width - 100, 20),
                XStringFormats.TopLeft);

            gfx.DrawString("Bauteiltyp \"Decke gegen die Außenluft\" mit den Wärmeübergangswiderständen Rsi  = 0,10 und Rse  = 0,04 m²K/W", bodyFont, XBrushes.Black,
                new XRect(50, 70, page.Width - 100, 20),
                XStringFormats.TopLeft);

            // Add an image below the text
            string imagePath = "C:\\Users\\arnes\\Dropbox\\BaupyhsikTool_Assets\\Dropdown.png"; // Specify your image path here
            double imageHeight = 0;
            double imageWidth = 0;
            if (File.Exists(imagePath))
            {
                XImage image = XImage.FromFile(imagePath);
                imageWidth = 200; // Set your desired width
                imageHeight = 150; // Set your desired height
                gfx.DrawImage(image, 50, 100, imageWidth, imageHeight); // Adjust the position and size as needed
            }
            // Calculate the starting position for the next section
            double contentStartY = 100 + imageHeight + 10; // Add a margin of 10 units below the image

            // Draw a horizontal line (simulating the dots)
            gfx.DrawLine(XPens.Black, 50, contentStartY, page.Width - 50, contentStartY);

            // Update start position for table
            contentStartY += 10; // Add a margin before the table

            // Table headers
            string[] headers = { "Querschnitt", "d (cm)", "ρ (kg/m³)", "λ (W/mK)", "R (m²K/W)" };

            // Table data
            string[,] data = {
                { "Rsi", "", "", "", "0,100" },
                { "01 Normalbeton bewehrt nach DIN 104", "22,00", "2400", "2,100", "0,105" },
                { "02 Dampfsperre", "0,07", "1150", "0,170", "0,004" },
                { "03 Gef.dämmung EPS 035 i.M.", "18,00", "20", "0,035", "5,143" },
                { "04 2 lag.bit. Abdichtung", "0,52", "1150", "0,170", "0,031" },
                { "05 Schutzvlies", "0,30", "-", "-", "-" },
                { "06 Terrassenbelag", "7,00", "1550", "-", "-" },
                { "Rse", "", "", "", "0,040" },
            };

            // Define table layout
            double startX = 50;
            double startY = contentStartY;
            double cellHeight = 18;
            double[] columnWidths = { 180, 50, 60, 60, 60 };

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

            // Draw a horizontal line (simulating the dots)
            gfx.DrawLine(XPens.Black, 50, currentY, page.Width - 50, currentY);

            // Draw thermal transmission coefficient
            gfx.DrawString("Wärmedurchgangskoeffizient U = 0,184 W/(m²K)  (ohne Korrekturen)", bodyFont, XBrushes.Black,
                new XRect(50, currentY + 10, page.Width - 100, 20),
                XStringFormats.TopLeft);

            // Draw minimum thermal resistance
            gfx.DrawString("Mindestwerte für Wärmedurchlasswiderstände nach DIN 4108-2", subtitleFont, XBrushes.Black,
                new XRect(50, currentY + 40, page.Width - 100, 20),
                XStringFormats.TopLeft);

            gfx.DrawString("R       5,28 ≥ 1,20  m²K/W erfüllt die Anforderungen", bodyFont, XBrushes.Black,
                new XRect(50, currentY + 60, page.Width - 100, 20),
                XStringFormats.TopLeft);

            // Save the document
            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //%appdata%/BauphysikTool
            string appFolder = Path.Combine(programDataPath, "BauphysikTool");
            string pdfFilePath = Path.Combine(appFolder, "ProjectOverview.pdf");

            document.Save(pdfFilePath);

            // Open the document with the default PDF viewer
            Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
        }

        static string[,] GetLayerData(Element element)
        {
            var layers = element.Layers;
            string[,] data = new string[layers.Count, 5];

            for (int i = 0; i < layers.Count; i++)
            {
                data[i, 0] = $"Layer {i + 1}";
                data[i, 1] = layers[i].Thickness.ToString("0.00");
                data[i, 2] = layers[i].AreaMassDensity.ToString("0.00");
                data[i, 3] = layers[i].Material.ThermalConductivity.ToString("0.00");
                data[i, 4] = layers[i].R_Value.ToString("0.00");
            }

            return data;
        }

        static void DrawHeader(XGraphics gfx, PdfPage page, string projectName, string date)
        {
            XFont headerFont = new XFont("Verdana", 10, XFontStyleEx.Regular);
            gfx.DrawString($"Project: {projectName} | Date: {date}", headerFont, XBrushes.Gray,
                new XRect(0, 0, page.Width, 30), XStringFormats.TopCenter);
        }

        static void DrawFooter(XGraphics gfx, PdfPage page, string author, int pageNumber)
        {
            XFont footerFont = new XFont("Verdana", 8, XFontStyleEx.Regular);
            gfx.DrawString($"Author: {author} | Page {pageNumber}", footerFont, XBrushes.Gray,
                new XRect(0, page.Height - 30, page.Width, 30), XStringFormats.TopCenter);
        }
    }
}
