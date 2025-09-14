using System;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using static BauphysikToolWPF.Calculation.EnvelopeCalculation;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.Domain.Enums;

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
        public int RoomNumber { get; set; } = -1; // not assigned by default
        public string FloorLevel { get; set; } = string.Empty;
        
        private double _roomHeightGross;
        public double RoomHeightGross
        {
            get => _roomHeightGross;
            set
            {
                if (Math.Abs(value - _roomHeightGross) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomHeightGross = value;

                RoomVolumeGross = RoomAreaGross * _roomHeightGross; // Update RoomVolumeGross when RoomHeightGross changes
            }
        }

        private double _roomAreaGross;
        public double RoomAreaGross
        {
            get => _roomAreaGross;
            set
            {
                if (Math.Abs(value - _roomAreaGross) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomAreaGross = value;

                RoomVolumeGross = _roomAreaGross * RoomHeightGross; // Update RoomVolumeGross when RoomAreaGross changes
            }
        }

        private double _roomVolumeGross;
        public double RoomVolumeGross
        {
            get => _roomVolumeGross;
            set
            {
                if (Math.Abs(value - _roomVolumeGross) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomVolumeGross = value;
                OnPropertyChanged(nameof(RoomVolumeGross)); // Notify change
            }
        }

        private double _roomHeightNet;
        public double RoomHeightNet
        {
            get => _roomHeightNet;
            set
            {
                if (Math.Abs(value - _roomHeightNet) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomHeightNet = value;

                RoomVolumeNet = RoomAreaNet * _roomHeightNet; // Update RoomVolumeNet when RoomHeightNet changes
            }
        }

        private double _roomAreaNet;
        public double RoomAreaNet
        {
            get => _roomAreaNet;
            set
            {
                if (Math.Abs(value - _roomAreaNet) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomAreaNet = value;

                RoomVolumeNet = _roomAreaNet * RoomHeightNet; // Update RoomVolumeNet when RoomAreaNet changes
            }
        }

        private double _roomVolumeNet;
        public double RoomVolumeNet
        {
            get => _roomVolumeNet;
            set
            {
                if (Math.Abs(value - _roomVolumeNet) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                _roomVolumeNet = value;
                OnPropertyChanged(nameof(RoomVolumeNet)); // Notify change
            }
        }

        private double _envelopeArea;
        public double EnvelopeArea
        {
            get => _envelopeArea;
            set
            {
                if (Math.Abs(value - _envelopeArea) < 1E-06) return; // No change
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
            get => _uValue;
            set
            {
                if (Math.Abs(value - _uValue) < 1E-06) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                if (value > 10) MainWindow.ShowToast("Unrealistischer U-Wert", ToastType.Warning);
                _uValue = value;
                OnPropertyChanged(nameof(UValue));
            }
        }
        private int _elementInternalId = -1; // For PropertyItem ComboBox selection
        public int ElementInternalId // For PropertyItem ComboBox selection
        {
            get => _elementInternalId;
            set
            {
                if (value == _elementInternalId) return; // No change
                _elementInternalId = value;
                // Update other Properties when ElementInternalId changes
                UValue = Element?.UValue ?? 0;
                TransmissionTransferType = Element?.Construction.TransmissionType ?? TransmissionTransfer.ToOutside;
            }
        }

        private double _fxValue = 1.0;

        public double FxValue
        {
            get => _fxValue;
            set
            {
                if (Math.Abs(value - _fxValue) < 1E-04) return; // No change
                if (value < 0)
                {
                    MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                    return;
                }
                if (value > 5) MainWindow.ShowToast("Unrealistischer Temperatur-Korrekturfaktor", ToastType.Warning);
                _fxValue = value;
            }
        }

        private TransmissionTransfer _transmissionTransferType = TransmissionTransfer.ToOutside;
        public TransmissionTransfer TransmissionTransferType
        {
            get => _transmissionTransferType;
            set
            {
                if (value == _transmissionTransferType) return; // No change
                _transmissionTransferType = value;
                OnPropertyChanged(nameof(TransmissionTransferTypeName)); // This is the relevant property for ComboBox binding
            }
        }

        public ThermalBridgeItem ThermalBridge { get; set; } = new ThermalBridgeItem();

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
        
        private bool _isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelectedArrowVisibility)); // Notify change
                }
            }
        }
        [JsonIgnore]
        public Visibility IsSelectedArrowVisibility => IsSelected ? Visibility.Visible : Visibility.Collapsed;
        [JsonIgnore]
        public bool IsReadonly { get; set; }
        [JsonIgnore]
        public int RoomGroupColorIndex => RoomNumber % 2; // 0 if even, 1 if odd
        [JsonIgnore]
        public bool ShowRoomData { get; set; } // For UI purposes

        [JsonIgnore]
        public Visibility ShowRoomDataVisibility => ShowRoomData ? Visibility.Visible : Visibility.Collapsed;

        [JsonIgnore]
        public static EnvelopeItem Empty => new EnvelopeItem(); // Optional static default (for easy reference)
        
        [JsonIgnore]
        public bool IsValid => RoomNumber != -1 && InternalId != -1;
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

        [JsonIgnore]
        public string TransmissionTransferTypeName
        {
            get => TransmissionTransferMapping[TransmissionTransferType];
            set
            {
                var match = TransmissionTransferMapping.FirstOrDefault(x => x.Value == value);
                if (!match.Equals(default(KeyValuePair<TransmissionTransfer, string>)))
                {
                    TransmissionTransferType = match.Key;
                }
            }
        }

        #endregion

        #region Public Methods

        public EnvelopeItem Copy()
        {
            var copy = new EnvelopeItem
            {
                RoomNumber = this.RoomNumber,
                RoomName = this.RoomName,
                FloorLevel = this.FloorLevel,
                RoomHeightGross = this.RoomHeightGross,
                RoomAreaGross = this.RoomAreaGross,
                RoomVolumeGross = this.RoomVolumeGross,
                RoomHeightNet = this.RoomHeightNet,
                RoomAreaNet = this.RoomAreaNet,
                RoomVolumeNet = this.RoomVolumeNet,
                EnvelopeArea = this.EnvelopeArea,
                UValue = this.UValue,
                ElementInternalId = this.ElementInternalId,
                FxValue = this.FxValue,
                TransmissionTransferType = this.TransmissionTransferType,
                Tag = this.Tag,
                ThermalBridge = this.ThermalBridge.Copy(),
                Comment = this.Comment,
                OrientationType = this.OrientationType,
                RoomUsageType = this.RoomUsageType,
                CreatedAt = this.CreatedAt,
                UpdatedAt = TimeStamp.GetCurrentUnixTimestamp(),
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
            return $"[{RoomNumber}] {RoomName} ({FloorLevel}) - Element: {Element} | U-Value: {UValue:0.##} | {OrientationType} / {RoomUsageType}";
        }

        #endregion
    }
}
