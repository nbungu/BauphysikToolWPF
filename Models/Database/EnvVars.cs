using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class EnvVars : IDatabaseObject<EnvVars>
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; } = -1;
        [NotNull]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public double Value { get; set; }
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
        public Unit Unit => SymbolMapping[Symbol].unit;

        public EnvVars Copy()
        {
            var copy = new EnvVars();
            copy.Id = -1;
            copy.Value = this.Value;
            copy.Symbol = this.Symbol;
            copy.Name = this.Name;
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
