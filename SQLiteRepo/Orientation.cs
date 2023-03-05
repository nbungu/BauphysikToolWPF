using SQLite;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Orientation
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int OrientationId { get; set; }

        [NotNull]
        public string Type { get; set; }

        //------Not part of the Database-----//


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Type + " (Id: " + OrientationId + ")";
        }
    }
}
