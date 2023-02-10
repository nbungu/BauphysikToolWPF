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

        // 1:n relationship with Requirement
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all Requirements linked to this 'RequirementSource')
        public List<Requirement> Requirements { get; set; }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " (Id: " + RequirementSourceId + ")";
        }
    }
}
