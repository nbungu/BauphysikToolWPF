using System.Text.RegularExpressions;

namespace BauphysikToolWPF.Services
{
    public static class TextInputValidation
    {
        // Formatting numbers for Germany (12,45 cm)
        public static Regex NumericCurrentCulture = new Regex("[^0-9,]+"); //regex that matches disallowed text

        public static Regex IntegerCurrentCulture = new Regex("[^0-9]+"); //regex that matches disallowed text
    }
}
