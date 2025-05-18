using BauphysikToolWPF.Services.Application;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using BauphysikToolWPF.UI.CustomControls;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    public class EnvelopeItem : IDomainObject<EnvelopeItem>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Serialization Objects

        public string RoomName { get; set; } = string.Empty;
        public string FloorLevel { get; set; } = string.Empty;

        private double _roomHeightGross;
        public double RoomHeightGross
        {
            get => _roomHeightGross;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomHeightGross = value;
            }
        }

        private double _roomAreaGross;
        public double RoomAreaGross
        {
            get => _roomAreaGross;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomAreaGross = value;
            }
        }

        private double _roomVolumeGross;
        public double RoomVolumeGross
        {
            get => _roomVolumeGross;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomVolumeGross = value;
            }
        }

        private double _roomHeightNet;
        public double RoomHeightNet
        {
            get => _roomHeightNet;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomHeightNet = value;
            }
        }

        private double _roomAreaNet;
        public double RoomAreaNet
        {
            get => _roomAreaNet;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomAreaNet = value;
            }
        }

        private double _roomVolumeNet;
        public double RoomVolumeNet
        {
            get => _roomVolumeNet;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomVolumeNet = value;
            }
        }

        private double _envelopeArea;
        public double EnvelopeArea
        {
            get => _envelopeArea;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _envelopeArea = value;
            }
        }

        private double _uValue;
        public double UValue
        {
            get => Element?.UValue ?? _uValue;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                if (value > 10) MainWindow.ShowToast("Unrealistischer U-Wert", ToastType.Warning);
                _uValue = value;
                ElementInternalId = -1;
            }
        }
        private int _elementInternalId = -1; // For PropertyItem ComboBox selection
        public int ElementInternalId // For PropertyItem ComboBox selection
        {
            get => _elementInternalId;
            set
            {
                _elementInternalId = value;
                OnPropertyChanged();
            }
        }

        private double _tempCorrectionFactor = 1.0;

        public double TempCorrectionFactor
        {
            get => _tempCorrectionFactor;
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                if (value > 10) MainWindow.ShowToast("Unrealistischer Temperatur-Korrekturfaktor", ToastType.Warning);
                _tempCorrectionFactor = value;
            }
        }
        public string Tag { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public OrientationType OrientationType { get; set; } = OrientationType.North;
        public RoomUsageType RoomUsageType { get; set; } = RoomUsageType.Wohnen;
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        #endregion

        #region Non-serialized Properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;

        // n:1 relationship with Element
        [JsonIgnore]
        public Element? Element
        {
            get => Session.SelectedProject?.Elements.FirstOrDefault(e => e?.InternalId == ElementInternalId, null);
            set => ElementInternalId = value?.InternalId ?? -1;
        }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public bool IsReadonly { get; set; }
        [JsonIgnore]
        public static EnvelopeItem Empty => new EnvelopeItem(); // Optional static default (for easy reference)

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
            get => RoomUsageTypeMapping[RoomUsageType];
            set
            {
                var match = RoomUsageTypeMapping.FirstOrDefault(x => x.Value == value);
                if (!match.Equals(default(KeyValuePair<RoomUsageType, string>)))
                {
                    RoomUsageType = match.Key;
                }
            }
        }

        /// <summary>
        /// H_T,j
        /// </summary>
        [JsonIgnore]
        public double TransmissionHeatTransferCoef => TempCorrectionFactor * UValue * EnvelopeArea;
        
        #endregion

        #region ctors
        #endregion

        #region Public Methods

        public EnvelopeItem Copy()
        {
            var copy = new EnvelopeItem
            {
                ElementInternalId = this.ElementInternalId,
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
                RoomUsageType = this.RoomUsageType,
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
            return $"[{InternalId}] {RoomName} ({FloorLevel}) - Element: {Element} | U-Value: {UValue:0.##} | {OrientationType} / {RoomUsageType}";
        }

        #endregion
    }
}
