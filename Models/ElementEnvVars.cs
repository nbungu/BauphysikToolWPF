using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    // Verknüpfungstabelle für m:n Beziehung von 'Element' und 'EnvVars'
    // Intermediate class, not used directly anywhere in the code
    public class ElementEnvVars
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [ForeignKey(typeof(EnvVars))] // FK for the m:n relation
        public int EnvVarId { get; set; }

        [ForeignKey(typeof(Element))] // FK for the m:n relation
        public int ElementId { get; set; }
    }
}
