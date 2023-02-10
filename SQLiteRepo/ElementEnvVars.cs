using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.SQLiteRepo
{
    // Verknüpfungstabelle für m:n Beziehung von 'Element' und 'EnvVars'
    // Intermediate class, not used directly anywhere in the code
    public class ElementEnvVars
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int Id { get; set; }

        [ForeignKey(typeof(EnvVars))] // FK for the x:x relation
        public int EnvVarId { get; set; }

        [ForeignKey(typeof(Element))] // FK for the x:x relation
        public int ElementId { get; set; }
    }
}
