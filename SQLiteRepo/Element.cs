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

        //------Not part of the Database-----//

        [OneToMany] // 1:n relationship with Layer, ON DELETE CASCADE
        public List<Layer> Layers { get; set; } // the corresp. object/Type for the foreign-key. The 'List<Layer>' object itself is not stored in DB!

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
    }
}
