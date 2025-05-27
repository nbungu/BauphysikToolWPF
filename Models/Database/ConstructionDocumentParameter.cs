using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models.Database
{
    // Verknüpfungstabelle für m:n Beziehung von 'Construction' und 'DocumentParameter'
    // Intermediate class, not used directly anywhere in the code
    public class ConstructionDocumentParameter : IDatabaseObject<ConstructionDocumentParameter>
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int Id { get; set; }

        [ForeignKey(typeof(Construction))] // FK for the m:n relation
        public int ConstructionId { get; set; }

        [ForeignKey(typeof(DocumentParameter))] // FK for the m:n relation
        public int DocumentParameterId { get; set; }
        [NotNull]
        public string Comment { get; set; } = string.Empty;
        [NotNull]
        public long CreatedAt { get; set; }
        [NotNull]
        public long UpdatedAt { get; set; }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public ConstructionDocumentParameter Copy()
        {
            var copy = new ConstructionDocumentParameter();
            copy.Id = -1;
            copy.ConstructionId = this.ConstructionId;
            copy.DocumentParameterId = this.DocumentParameterId;
            copy.Comment = this.Comment;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }
    }
}
