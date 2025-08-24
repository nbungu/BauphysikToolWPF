using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class ClimateData : IDatabaseObject<ClimateData>
    {
        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int Id { get; set; }
        [NotNull]
        public int Region { get; set; }
        [NotNull]
        public ReferenceLocation ReferenceLocation { get; set; }
        [NotNull]
        public double Value { get; set; }
        [NotNull]
        public Month Month { get; set; }
        [NotNull]
        public Symbol Symbol { get; set; }
        [NotNull]
        public string Comment { get; set; } = string.Empty;
        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource => DatabaseAccess.QueryDocumentSourceById(DocumentSourceId);
        
        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            
            return $"Location: {ReferenceLocationMapping[ReferenceLocation]}, " +
                   $"Month: {MonthMapping[Month]}, " +
                   $"Value: {Value} {GetUnitStringFromSymbol(Symbol)}, " +
                   $"Comment: \"{Comment}\", " +
                   $"Source: {DocumentSource.SourceName}";
        }

        public ClimateData Copy()
        {
            var copy = new ClimateData();
            copy.Id = -1;
            copy.Region = this.Region;
            copy.ReferenceLocation = this.ReferenceLocation;
            copy.Value = this.Value;
            copy.Month = this.Month;
            copy.Symbol = this.Symbol;
            copy.Comment = this.Comment;
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
