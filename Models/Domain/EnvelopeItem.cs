using BauphysikToolWPF.Services.Application;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    public class EnvelopeItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Serialization Objects

        public string RoomName { get; set; } = string.Empty;
        public string FloorLevel { get; set; } = string.Empty;
        public double RoomHeightGross { get; set; }
        public double RoomAreaGross { get; set; }
        public double RoomVolumeGross { get; set; }
        public double RoomHeightNet { get; set; }
        public double RoomAreaNet { get; set; }
        public double RoomVolumeNet { get; set; }
        public double EnvelopeArea { get; set; }
        private double _uValue;
        public double UValue
        {
            get => Element?.UValue ?? _uValue;
            set
            {
                _uValue = value;
                ElementIndex = -1;
            }
        }

        public string Tag { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public OrientationType OrientationType { get; set; } = OrientationType.North;
        public UsageZone UsageZone { get; set; } = UsageZone.Wohnen;
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        #endregion

        #region Non-serialized Properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;

        // n:1 relationship with Element
        private Element? _element;
        [JsonIgnore]
        public Element? Element
        {
            get => _element;
            set
            {
                _element = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool IsSelected { get; set; }
        [JsonIgnore]
        public bool IsReadonly { get; set; }
        [JsonIgnore]
        public static EnvelopeItem Empty => new EnvelopeItem(); // Optional static default (for easy reference)
        
        [JsonIgnore]
        public int ElementIndex // For PropertyItem ComboBox selection
        {
            get => Element is null ? -1 : Session.SelectedProject?.Elements.IndexOf(Element) ?? -1;
            set
            {
                if (Session.SelectedProject != null && value >= 0)
                {
                    Element = Session.SelectedProject.Elements[value];
                }
                else if (value == -1)
                {
                    Element = null;
                }
            }
        }

        [JsonIgnore]
        public bool IsValid => FloorLevel != string.Empty && RoomName != string.Empty;
        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        [JsonIgnore]
        public string OrientationTypeName
        {
            get => OrientationTypeMapping[OrientationType];
            set
            {
                var match = OrientationTypeMapping.FirstOrDefault(x => x.Value == value);
                if (!match.Equals(default(KeyValuePair<OrientationType, string>)))
                {
                    OrientationType = match.Key;
                }
            }
        }
        [JsonIgnore]
        public string UsageZoneName
        {
            get => UsageZoneMapping[UsageZone];
            set
            {
                var match = UsageZoneMapping.FirstOrDefault(x => x.Value == value);
                if (!match.Equals(default(KeyValuePair<UsageZone, string>)))
                {
                    UsageZone = match.Key;
                }
            }
        }

        #endregion

        #region ctors
        #endregion

        #region Public Methods

        public EnvelopeItem Copy()
        {
            var copy = new EnvelopeItem
            {
                Element = this.Element, // shallow copy OK here
                CreatedAt = this.CreatedAt,
                UpdatedAt = TimeStamp.GetCurrentUnixTimestamp(),
                RoomName = this.RoomName,
                FloorLevel = this.FloorLevel,
                RoomHeightGross = this.RoomHeightGross,
                RoomAreaGross = this.RoomAreaGross,
                RoomVolumeGross = this.RoomVolumeGross,
                RoomHeightNet = this.RoomHeightNet,
                RoomAreaNet = this.RoomAreaNet,
                RoomVolumeNet = this.RoomVolumeNet,
                EnvelopeArea = this.EnvelopeArea,
                Tag = this.Tag,
                Comment = this.Comment,
                OrientationType = this.OrientationType,
                UsageZone = this.UsageZone,
            };
            return copy;
        }
        public void CopyToProject(Project project)
        {
            var copy = Copy();
            project.EnvelopeItems.Add(copy);
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return $"[{InternalId}] {RoomName} ({FloorLevel}) - Element: {Element} | U-Value: {UValue:0.##} | {OrientationType} / {UsageZone}";
        }

        #endregion
    }
}
