using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class Requirement : IDatabaseObject<Requirement>
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

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }
        [NotNull]
        public long CreatedAt { get; set; }
        [NotNull]
        public long UpdatedAt { get; set; }

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

        public Requirement Copy()
        {
            var copy = new Requirement();
            copy.Id = -1;
            copy.RefNumber = this.RefNumber;
            copy.ValueA = this.ValueA;
            copy.ValueACondition = this.ValueACondition;
            copy.ValueB = this.ValueB;
            copy.ValueBCondition = this.ValueBCondition;
            copy.Symbol = this.Symbol;
            copy.DocumentSourceId = this.DocumentSourceId;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
