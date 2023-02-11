using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo
{
    // Verknüpfungstabelle für m:n Beziehung von 'Construction' und 'Requirement'
    // Intermediate class, not used directly anywhere in the code
    public class ConstructionRequirement
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int Id { get; set; }

        [ForeignKey(typeof(Construction))] // FK for the m:n relation
        public int ConstructionId { get; set; }

        [ForeignKey(typeof(Requirement))] // FK for the m:n relation
        public int RequirementId { get; set; }

        public string? Comment { get; set; }
    }
}
