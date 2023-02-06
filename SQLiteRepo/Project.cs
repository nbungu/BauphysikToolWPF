using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Project
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ProjectId { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        [NotNull]
        public int BuildingUsage { get; set; }

        [NotNull]
        public int BuildingAge { get; set; }

        //------Not part of the Database-----//

        [OneToMany] // 1:n relationship with Element, ON DELETE CASCADE
        public List<Element> Elements { get; set; } // the corresp. object/Type for the foreign-key. The 'List<Element>' object itself is not stored in DB!

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return ProjectId + "_" + Name;
        }
    }
}
