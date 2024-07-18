using BauphysikToolWPF.Models.Helper;
using SQLite;

namespace BauphysikToolWPF.Models
{
    public class EnvVars
    {
        //------Variablen-----//

        // Hinweis auf Normierungsfehler

        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }
        [NotNull]
        public Symbol Symbol { get; set; }
        [NotNull]
        public double Value { get; set; }
        [NotNull]
        public string Comment { get; set; } = string.Empty;
        [NotNull]
        public Unit Unit { get; set; }


        //------Not part of the Database-----//

    }
}
