using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.SQLiteRepo
{
    // Verknüpfungstabelle für n:n Beziehung von 'Element' und 'EnvVars'
    // This entity is never used directly in the application!
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
