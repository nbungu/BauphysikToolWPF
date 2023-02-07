using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class RequirementSource
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int RequirementSourceId { get; set; }

        [NotNull] 
        public string Name { get; set; } 

        [NotNull]
        public int Year { get; set; }

        [NotNull]
        public string Specification { get; set; }

        [NotNull]
        public string RequirementType { get; set; }

        [NotNull]
        public string RequirementUnit { get; set; }

        //------Not part of the Database-----//
        
        [OneToMany] // 1:n relationship with Requirement (the child table)
        public List<Requirement> Requirements { get; set; } // Gets the corresp. object linked by the foreign-key. The 'Requirements' object itself is not stored in DB!


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        /*public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return LayerThickness + " cm, "+ Material.Name + " (Pos. " + this.LayerPosition + ")";
        }*/
    }
}
