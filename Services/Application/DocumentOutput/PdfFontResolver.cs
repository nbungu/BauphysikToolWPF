using System.IO;
using PdfSharp.Fonts;

namespace BauphysikToolWPF.Services.Application.DocumentOutput
{
    public class PdfFontResolver : IFontResolver
    {
        // Define internal keys for font files
        private const string ArialRegular = "Arial#Regular";
        private const string ArialBold = "Arial#Bold";
        private const string ArialItalic = "Arial#Italic";

        private const string VerdanaRegular = "Verdana#Regular";
        private const string VerdanaBold = "Verdana#Bold";
        private const string VerdanaItalic = "Verdana#Italic";

        private const string TimesRegular = "Times#Regular";
        private const string TimesBold = "Times#Bold";
        private const string TimesItalic = "Times#Italic";

        public byte[] GetFont(string faceName)
        {
            // Adjust these paths for your environment!
            switch (faceName)
            {
                case ArialRegular:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\arial.ttf");
                case ArialBold:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\arialbd.ttf");
                case ArialItalic:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\ariali.ttf");

                case VerdanaRegular:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\verdana.ttf");
                case VerdanaBold:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\verdanab.ttf");
                case VerdanaItalic:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\verdanai.ttf");

                case TimesRegular:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\times.ttf");    // Sometimes "timesnewroman.ttf"
                case TimesBold:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\timesbd.ttf");
                case TimesItalic:
                    return File.ReadAllBytes(@"C:\Windows\Fonts\timesi.ttf");

                default:
                    return null;
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            familyName = familyName.ToLowerInvariant();

            switch (familyName)
            {
                case "arial":
                    if (isBold) return new FontResolverInfo(ArialBold);
                    if (isItalic) return new FontResolverInfo(ArialItalic);
                    return new FontResolverInfo(ArialRegular);

                case "verdana":
                    if (isBold) return new FontResolverInfo(VerdanaBold);
                    if (isItalic) return new FontResolverInfo(VerdanaItalic);
                    return new FontResolverInfo(VerdanaRegular);

                case "times new roman":
                case "times":
                    if (isBold) return new FontResolverInfo(TimesBold);
                    if (isItalic) return new FontResolverInfo(TimesItalic);
                    return new FontResolverInfo(TimesRegular);
            }

            // Fallback to platform resolver (built-in fonts)
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }
    }
}
