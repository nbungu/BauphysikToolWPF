using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class Construction
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ConstructionId { get; set; }

        [NotNull]
        public string Type { get; set; }

        //------Not part of the Database-----//

        [ManyToMany(typeof(ConstructionRequirement))] // m:n relationship with Requirement (ConstructionRequirement is intermediate entity)
        public List<Requirement> Requirements { get; set; }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Type + " (Id: " + ConstructionId + ")";
        }
    }
}
