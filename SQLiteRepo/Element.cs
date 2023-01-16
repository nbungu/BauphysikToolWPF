using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class Element 
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique] 
        public int ElementId { get; set; }

        [NotNull]
        public string Name { get; set; }

        [SQLiteNetExtensions.Attributes.ForeignKey(typeof(ConstructionType))] // FK for the 1:1 relation
        public int ConstructionTypeId { get; set; } // This Element is construction type X

        //------Not part of the Database-----//

        [OneToMany] // 1:n relationship with Layer, ON DELETE CASCADE
        public List<Layer> Layers { get; set; } // the corresp. object/Type for the foreign-key. The 'List<Layer>' object itself is not stored in DB!

        [OneToOne] // 1:1 relationship with ConstructionType
        public ConstructionType ConstructionType { get; set; } // Gets the corresp. object linked by the foreign-key. The 'Material' object itself is not stored in DB!


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return ElementId + "_" + Name + " (" + this.ConstructionType.Name + ")";
        }
    }
}
