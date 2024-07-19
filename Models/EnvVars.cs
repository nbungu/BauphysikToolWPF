using BauphysikToolWPF.Models.Helper;
using SQLite;
using SQLiteNetExtensions.Attributes;

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
        public double Value { get; set; }
        [NotNull]
        public Symbol Symbol { get; set; }
        [NotNull]
        public Unit Unit { get; set; }
        [NotNull]
        public string Comment { get; set; } = string.Empty;

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource { get; set; } = new DocumentSource();

    }
}
