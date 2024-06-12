using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    public enum RequirementSourceType
    {
        // enum values have to match the RequirementSourceId in DB
        GEG_Anlage1 = 1,
        GEG_Anlage2 = 2,
        GEG_Anlage7 = 3,
        DIN_4108_2_Tabelle3 = 4
    }
    public class RequirementSource
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int RequirementSourceId { get; set; }

        [NotNull]
        public string Name { get; set; } = string.Empty;

        [NotNull]
        public int Year { get; set; }

        [NotNull]
        public string Specification { get; set; } = string.Empty;

        [NotNull]
        public string RequirementType { get; set; } = string.Empty;

        [NotNull]
        public string RequirementUnit { get; set; } = string.Empty;

        //------Not part of the Database-----//

        // 1:n relationship with Requirement
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all Requirements linked to this 'RequirementSource')
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [Ignore]
        public RequirementSourceType SourceType => (RequirementSourceType)RequirementSourceId; // Returns the enum by its number value (via RequirementSourceId)

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " (Id: " + RequirementSourceId + ")";
        }
    }
}
