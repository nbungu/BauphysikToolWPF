using SQLite;
using SQLiteNetExtensions.Attributes;
using static BauphysikToolWPF.Models.Database.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class RoomUsageProfile
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public RoomUsageType RoomUsageType { get; set; }

        [NotNull]
        public string UsageName { get; set; } = string.Empty;

        public string UsageDescription { get; set; } = string.Empty;

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource { get; set; } = new DocumentSource();

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return UsageDescription + " (Id: " + Id + ")";
        }
    }
}
