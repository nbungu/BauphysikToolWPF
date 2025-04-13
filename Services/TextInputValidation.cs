using System.Text.RegularExpressions;

namespace BauphysikToolWPF.Services
{
    public static class TextInputValidation
    {
        // Formatting numbers for Germany (12,45 cm)

        public static Regex NumericCurrentCulture = new Regex("[^0-9,]+"); //regex that matches disallowed text

        public static Regex IntegerCurrentCulture = new Regex("[^0-9]+"); //regex that matches disallowed text

        public static Regex Any = new Regex("[^a-zA-Z0-9 .,!?@#()-]+"); // Matches any character that is NOT a letter, number, or basic punctuation

        public static bool IsValidWindowsFileName(string fileName)
        {
            // Regular expression to validate Windows file names
            string pattern = @"^(?!^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$)[^\x00-\x1F<>:""/\\|?*]+(?<![ .])$";

            // Match the pattern with the file name and ensure it's not too long
            return fileName.Length <= 255 && Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase);
        }
    }
}
