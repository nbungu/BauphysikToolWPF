using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.SQLiteRepo
{
    // Verknüpfungstabelle für n:n Beziehung von 'Element' und 'EnvVars'
    // This entity is never used directly in the application!
    public class ElementEnvVars
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int ElementEnvVarsId { get; set; }

        [ForeignKey(typeof(EnvVars))] // FK for the x:x relation
        public int EnvVarId { get; set; }
        public string EnvVarSymbol { get; set; }

        [ForeignKey(typeof(Element))] // FK for the x:x relation
        public int ElementId { get; set; }
        public string ElementName { get; set; }
    }
}
