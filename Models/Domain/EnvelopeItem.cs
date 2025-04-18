using BauphysikToolWPF.Services.Application;
using System.Text.Json.Serialization;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    public class EnvelopeItem
    {
        #region Serialization Objects

        public string RoomName { get; set; } = string.Empty;
        public string FloorLevel { get; set; } = string.Empty;
        public double RoomHeightGross { get; set; } = 0.0;
        public double RoomAreaGross { get; set; } = 0.0;
        public double RoomVolumeGross { get; set; } = 0.0;
        public double RoomHeightNet { get; set; } = 0.0;
        public double RoomAreaNet { get; set; } = 0.0;
        public double RoomVolumeNet { get; set; } = 0.0;
        public double EnvelopeArea { get; set; } = 0.0;

        private double _uValue = 0.0;
        public double UValue
        {
            get => Element?.UValue ?? _uValue;
            set => _uValue = value;
        }
        public string Tag { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        private OrientationType _orientationType = OrientationType.North;
        public OrientationType OrientationType
        {
            get => Element?.OrientationType ?? _orientationType;
            set => _orientationType = value;
        }
        public UsageZone Zone { get; set; } = UsageZone.Wohnen;
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        #endregion

        #region Non-serialized Properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;
        // n:1 relationship with Element
        [JsonIgnore]
        public Element? Element { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }
        [JsonIgnore]
        public bool IsValid => FloorLevel != string.Empty && RoomName != string.Empty;
        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        #endregion
        
        #region ctors
        #endregion

        #region Public Methods

        public EnvelopeItem Copy()
        {
            var copy = new EnvelopeItem();
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

        #endregion
    }
}
