using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models.Database
{
    // Verknüpfungstabelle für m:n Beziehung von 'Construction' und 'Requirement'
    // Intermediate class, not used directly anywhere in the code
    public class ConstructionRequirement : IDatabaseObject<ConstructionRequirement>
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int Id { get; set; }

        [ForeignKey(typeof(Construction))] // FK for the m:n relation
        public int ConstructionId { get; set; }

        [ForeignKey(typeof(Requirement))] // FK for the m:n relation
        public int RequirementId { get; set; }
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

        public ConstructionRequirement Copy()
        {
            var copy = new ConstructionRequirement();
            copy.Id = -1;
            copy.ConstructionId = this.ConstructionId;
            copy.RequirementId = this.RequirementId;
            copy.Comment = this.Comment;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }
    }
}
