using SQLite;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class ConstructionType
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique] 
        public int ConstructionTypeId { get; set; }

        [NotNull]
        public string Name { get; set; }

        //------Not part of the Database-----//


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " (Id: " + ConstructionTypeId + ")";
        }
    }
}
