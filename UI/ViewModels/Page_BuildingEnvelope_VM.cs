using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_BuildingEnvelope_VM : ObservableObject
    {
        private static readonly EnvelopeItem _presetEnvelopeItem = new EnvelopeItem();
        private EnvelopeItem? _previousItem;

        public Page_BuildingEnvelope_VM()
        {
            Session.EnvelopeItemsChanged += UpdateXamlBindings;

            PropertyItem<double>.PropertyChanged += UpdatePresets;
            PropertyItem<int>.PropertyChanged += UpdatePresets;
            PropertyItem<string>.PropertyChanged += UpdatePresets;
        }
        
        [RelayCommand]
        private void AddEnvelopeItem()
        {
            var item = EnvelopeItem.Empty;
            if (IsInfoPresetChecked)
            {
                item.FloorLevel = _presetEnvelopeItem.FloorLevel;
                item.RoomName = _presetEnvelopeItem.RoomName;
                item.UsageZone = _presetEnvelopeItem.UsageZone;
            }
            if (IsElementPresetChecked)
            {
                item.EnvelopeArea = _presetEnvelopeItem.EnvelopeArea;
                item.UValue = _presetEnvelopeItem.UValue;
                item.ElementIndex = _presetEnvelopeItem.ElementIndex;
                item.OrientationType = _presetEnvelopeItem.OrientationType;
            }
            if (IsRoomPresetChecked)
            {
                item.RoomHeightGross = _presetEnvelopeItem.RoomHeightGross;
                item.RoomAreaGross = _presetEnvelopeItem.RoomAreaGross;
                item.RoomVolumeGross = _presetEnvelopeItem.RoomVolumeGross;
                item.RoomHeightNet = _presetEnvelopeItem.RoomHeightNet;
                item.RoomAreaNet = _presetEnvelopeItem.RoomAreaNet;
                item.RoomVolumeNet = _presetEnvelopeItem.RoomVolumeNet;
            }
            Session.SelectedProject?.AddEnvelopeItem(item);
            Session.OnEnvelopeItemsChanged();
        }

        [RelayCommand]
        private void DeleteEnvelopeItem()
        {
            Session.SelectedProject?.RemoveEnvelopeItem(SelectedEnvelopeItem?.InternalId ?? -1);
            Session.OnEnvelopeItemsChanged();
        }

        [RelayCommand]
        private void DuplicateEnvelopeItem()
        {
            Session.SelectedProject?.DuplicateEnvelopeItem(SelectedEnvelopeItem?.InternalId ?? -1);
            Session.OnEnvelopeItemsChanged();
        }

        partial void OnIsAllSelectedChanged(bool value)
        {
            if (Session.SelectedProject != null)
            {
                foreach (var item in Session.SelectedProject.EnvelopeItems)
                {
                    item.IsSelected = value;
                }
                Session.OnEnvelopeItemsChanged();
            }
        }
        partial void OnSelectedEnvelopeItemChanged(EnvelopeItem? value)
        {
            // Set the PropertyChanged event handler dynamically for the new selected item
            SetEventHandler(value);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRowSelected))]
        private EnvelopeItem? _selectedEnvelopeItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isInfoPresetChecked = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isElementPresetChecked = false;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isRoomPresetChecked = false;

        [ObservableProperty]
        private static bool _isAllSelected = false;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public ObservableCollection<EnvelopeItem> EnvelopeItems => new ObservableCollection<EnvelopeItem>(Session.SelectedProject?.EnvelopeItems ?? new List<EnvelopeItem>());
        public bool IsRowSelected => SelectedEnvelopeItem != null;
        public Visibility PresetActiveVisibility => AnyPresetActive ? Visibility.Visible : Visibility.Collapsed;
        public bool AnyPresetActive => IsInfoPresetChecked || IsElementPresetChecked || IsRoomPresetChecked;
        public string ItemsCount => $"Bereiche angelegt: {EnvelopeItems.Count}";
        public string ItemsCheckedCount => $"Bereiche markiert: {EnvelopeItems.Where(e => e.IsSelected).ToList().Count}";
        public Visibility NoEntriesVisibility => EnvelopeItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        public IEnumerable<IPropertyItem> InfoPresetProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Etage", () => _presetEnvelopeItem.FloorLevel, value => _presetEnvelopeItem.FloorLevel = value),
            new PropertyItem<string>("Bezeichnung", () => _presetEnvelopeItem.RoomName, value => _presetEnvelopeItem.RoomName = value),
            new PropertyItem<int>("Zone", () => (int)_presetEnvelopeItem.UsageZone, value => _presetEnvelopeItem.UsageZone = (UsageZone)value)
            {
                PropertyValues = UsageZoneMapping.Values.Cast<object>().ToArray()
            },
        };
        public IEnumerable<IPropertyItem> ElementPresetProperties => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.Area, () => _presetEnvelopeItem.EnvelopeArea, value => _presetEnvelopeItem.EnvelopeArea = value),
            new PropertyItem<double>(Symbol.UValue, () => _presetEnvelopeItem.UValue, value => _presetEnvelopeItem.UValue = value),
            new PropertyItem<int>("Bauteil", () => _presetEnvelopeItem.ElementIndex, value => _presetEnvelopeItem.ElementIndex = value)
            {
                PropertyValues = GetElements().Cast<object>().ToArray()
            },
            new PropertyItem<int>("Ausrichtung", () => (int)_presetEnvelopeItem.OrientationType, value => _presetEnvelopeItem.OrientationType = (OrientationType)value)
            {
                PropertyValues = OrientationTypeMapping.Values.Cast<object>().ToArray()
            },
        };
        public IEnumerable<IPropertyItem> RoomPresetProperties => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.Length, () => _presetEnvelopeItem.RoomHeightGross, value => _presetEnvelopeItem.RoomHeightGross = value),
            new PropertyItem<double>(Symbol.Area, () => _presetEnvelopeItem.RoomAreaGross, value => _presetEnvelopeItem.RoomAreaGross = value),
            new PropertyItem<double>(Symbol.Volume, () => _presetEnvelopeItem.RoomVolumeGross, value => _presetEnvelopeItem.RoomVolumeGross = value),
            new PropertyItem<double>(Symbol.Length, () => _presetEnvelopeItem.RoomHeightNet, value => _presetEnvelopeItem.RoomHeightNet = value) { SymbolSubscriptText = "netto"},
            new PropertyItem<double>(Symbol.Area, () => _presetEnvelopeItem.RoomAreaNet, value => _presetEnvelopeItem.RoomAreaNet = value) { SymbolSubscriptText = "netto"},
            new PropertyItem<double>(Symbol.Volume, () => _presetEnvelopeItem.RoomVolumeNet, value => _presetEnvelopeItem.RoomVolumeNet = value) { SymbolSubscriptText = "netto"},
        };

        public IEnumerable<Element> GetElements() => Session.SelectedProject?.Elements ?? Enumerable.Empty<Element>();
        public IEnumerable<string> GetOrientationTypeNames() => OrientationTypeMapping.Values;
        public IEnumerable<string> GetUsageZoneNames() => UsageZoneMapping.Values;

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>

        private void UpdateXamlBindings()
        {
            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(EnvelopeItems));
            OnPropertyChanged(nameof(IsRowSelected));
            OnPropertyChanged(nameof(AnyPresetActive));
            OnPropertyChanged(nameof(ItemsCount));
            OnPropertyChanged(nameof(NoEntriesVisibility));
        }
        private void UpdatePresets()
        {
            OnPropertyChanged(nameof(InfoPresetProperties));
            OnPropertyChanged(nameof(ElementPresetProperties));
        }

        private void UpdatePropertyDependencies(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(EnvelopeItems));
        }

        private void SetEventHandler(EnvelopeItem? item)
        {
            if (_previousItem != null)
                _previousItem.PropertyChanged -= UpdatePropertyDependencies;

            _previousItem = item;

            if (item != null)
                item.PropertyChanged += UpdatePropertyDependencies;
        }
    }
}
