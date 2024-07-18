using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    // TODO add enum for ConstructionTypes

    public class Construction
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ConstructionId { get; set; }

        [NotNull]
        public string TypeName { get; set; } = "Außenwand";

        [NotNull]
        public int IsVertical { get; set; }

        //------Not part of the Database-----//

        // m:n relationship with Requirement
        [ManyToMany(typeof(ConstructionRequirement), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [Ignore]
        public bool IsLayoutVertical // true = 1
        {
            get => IsVertical == 1;
            set => IsVertical = (value) ? 1 : 0;
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return TypeName + " (Id: " + ConstructionId + ")";
        }
    }
}
