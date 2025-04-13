using BauphysikToolWPF.Services;
using SQLite;
using System.Text.Json.Serialization;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Repository.Models
{
    public enum UsageZone
    {
        Wohnen,
        Verkehrsflaechen
    }

    public class EnvelopeItem
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet
        [NotNull]
        public string RoomName { get; set; } = string.Empty;
        [NotNull]
        public string FloorLevel { get; set; } = string.Empty;
        [NotNull]
        public double RoomHeightGross { get; set; } = 0.0;
        [NotNull]
        public double RoomAreaGross { get; set; } = 0.0;
        [NotNull]
        public double RoomVolumeGross { get; set; } = 0.0;
        [NotNull]
        public double RoomHeightNet { get; set; } = 0.0;
        [NotNull]
        public double RoomAreaNet { get; set; } = 0.0;
        [NotNull]
        public double RoomVolumeNet { get; set; } = 0.0;
        [NotNull]
        public double EnvelopeArea { get; set; } = 0.0;

        [NotNull, ForeignKey(typeof(Element))] // FK for the n:1 relationship with Element
        public int ElementId { get; set; } = -1;

        private double _uValue = 0.0;
        [NotNull]
        public double UValue
        {
            get => Element is null ? _uValue : Element.UValue;
            set => _uValue = value;
        }
        [NotNull]
        public string Tag { get; set; } = string.Empty;
        [NotNull]
        public string Comment { get; set; } = string.Empty;

        private OrientationType _orientationType = OrientationType.Norden;
        [NotNull]
        public OrientationType OrientationType
        {
            get => ElementId == -1 ? _orientationType : Element.OrientationType;
            set => _orientationType = value;
        }
        [NotNull]
        public UsageZone Zone { get; set; } = UsageZone.Wohnen;
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        [Ignore, JsonIgnore]
        public int InternalId { get; set; }
        // n:1 relationship with Element
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead), JsonIgnore]
        public Element Element { get; set; }

        [Ignore, JsonIgnore]
        public bool IsSelected { get; set; }

        [Ignore, JsonIgnore]
        public bool IsValid => FloorLevel != string.Empty && RoomName != string.Empty;

        // Readonly Properties

        [Ignore, JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore, JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        //------Konstruktor-----//


        //------Methoden-----//
        public EnvelopeItem Copy()
        {
            var copy = new EnvelopeItem();
            copy.Id = -1;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.InternalId = this.InternalId;

            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return "";
        }
    }
}
