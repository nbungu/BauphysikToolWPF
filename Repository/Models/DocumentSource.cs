using SQLite;

namespace BauphysikToolWPF.Repository.Models
{
    public enum RequirementSourceType
    {
        NotDefined,
        GEG_Anlage1,
        GEG_Anlage2,
        GEG_Anlage7,
        DIN_4108_2_Tabelle3,
        DIN_V_18599_10_AnhangA,
    }
    public class DocumentSource
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public RequirementSourceType Source { get; set; }

        [NotNull]
        public string SourceName { get; set; } = string.Empty;

        public string SourceDescription { get; set; } = string.Empty;


        //------Not part of the Database-----//


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return SourceDescription + " (Id: " + Id + ")";
        }
    }
}
