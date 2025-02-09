using System.Collections.Generic;
using BauphysikToolWPF.UI.Models;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Repository.Models
{
    public class Requirement
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; } = -1;

        [NotNull]
        public string RefNumber { get; set; } = string.Empty;

        [NotNull]
        public double ValueA { get; set; }

        public string? ValueACondition { get; set; }

        public double? ValueB { get; set; }

        public string? ValueBCondition { get; set; }

        [NotNull]
        public Symbol Symbol { get; set; }
        [NotNull]
        public Unit Unit { get; set; }

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource { get; set; } = new DocumentSource();

        // m:n relationship with Construction
        [ManyToMany(typeof(ConstructionRequirement), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Construction> Constructions { get; set; } = new List<Construction>();

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return "(Id: " + Id + ")";
        }
    }
}
