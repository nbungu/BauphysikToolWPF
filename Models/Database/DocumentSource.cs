using BauphysikToolWPF.Services.Application;
using SQLite;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class DocumentSource : IDatabaseObject<DocumentSource>
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public RequirementSourceType Source { get; set; }

        [NotNull]
        public string SourceName { get; set; } = string.Empty;

        public string SourceDescription { get; set; } = string.Empty;

        [NotNull]
        public long CreatedAt { get; set; }
        [NotNull]
        public long UpdatedAt { get; set; }


        //------Not part of the Database-----//


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return SourceDescription + " (Id: " + Id + ")";
        }

        public DocumentSource Copy()
        {
            var copy = new DocumentSource();
            copy.Id = -1;
            copy.Source = this.Source;
            copy.SourceName = this.SourceName;
            copy.SourceDescription = this.SourceDescription;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
