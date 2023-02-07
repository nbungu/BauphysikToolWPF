using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Requirement
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int RequirementId { get; set; }

        [ForeignKey(typeof(RequirementSource))] // FK for the n:1 relation
        public int RequirementSourceId { get; set; }

        [NotNull] 
        public string RefNumber { get; set; } 

        [NotNull]
        public double ValueA { get; set; }

        public string ValueACondition { get; set; }

        public double ValueB { get; set; }

        public string ValueBCondition { get; set; }

        //------Not part of the Database-----//
        
        [ManyToOne] // n:1 relationship with RequirementSource (the parent table)
        public RequirementSource RequirementSource { get; set; } // Gets the corresp. object linked by the foreign-key. The 'RequirementSource' object itself is not stored in DB!

        [ManyToMany(typeof(ConstructionRequirement))] // m:n relationship with Construction (ConstructionRequirement is intermediate entity)
        public List<Construction> Constructions { get; set; }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        /*public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return LayerThickness + " cm, "+ Material.Name + " (Pos. " + this.LayerPosition + ")";
        }*/
    }
}
