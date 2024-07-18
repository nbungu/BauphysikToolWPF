using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    public class Requirement
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull, ForeignKey(typeof(RequirementSource))] // FK for the n:1 relationship with RequirementSource
        public int RequirementSourceId { get; set; }

        [NotNull]
        public string RefNumber { get; set; } = string.Empty;

        [NotNull]
        public double ValueA { get; set; }

        public string? ValueACondition { get; set; }

        public double? ValueB { get; set; }

        public string? ValueBCondition { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with RequirementSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public RequirementSource RequirementSource { get; set; } = new RequirementSource();

        // m:n relationship with Construction
        [ManyToMany(typeof(ConstructionRequirement), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Construction> Constructions { get; set; } = new List<Construction>();

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return "(Id: " + Id + ")";
        }
    }
}
