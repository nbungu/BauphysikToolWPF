using System.Collections.Generic;
using System.Text.Json.Serialization;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    public enum ConstructionType
    {
        NotDefined,
        Aussenwand,
        AussenwandGegErdreich,
        Innenwand,
        WhgTrennwand,
        GeschossdeckeGegAussenluft,
        DeckeGegUnbeheizt,
        InnenwandGegUnbeheizt,
        Flachdach,
        Schraegdach,
        Umkehrdach,
        Bodenplatte,
        Kellerdecke,
    }

    public class Construction
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ConstructionId { get; set; }
        [NotNull]
        public ConstructionType Type { get; set; }
        [NotNull]
        public string TypeName { get; set; } = string.Empty;

        public string TypeDescription { get; set; } = string.Empty;

        [NotNull]
        public int IsVertical { get; set; }

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource { get; set; } = new DocumentSource();

        // m:n relationship with Requirement
        [ManyToMany(typeof(ConstructionRequirement), CascadeOperations = CascadeOperation.CascadeRead), JsonIgnore]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [Ignore, JsonIgnore]
        public bool IsLayoutVertical // true = 1
        {
            get => IsVertical == 1;
            set => IsVertical = (value) ? 1 : 0;
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return TypeName + " (Id: " + ConstructionId + ")";
        }
    }
}
