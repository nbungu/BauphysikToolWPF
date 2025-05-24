using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Calculation.EnvelopeCalculation;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_BuildingEnvelope_VM : ObservableObject
    {
        private static readonly EnvelopeItem _presetEnvelopeItem = new EnvelopeItem()
        {
            FloorLevel = "EG",
            RoomName = "Wohnzimmer",
            RoomUsageType = RoomUsageType.Wohnen,
        };
        private readonly EnvelopeCalculation _envelopeCalc;

        private EnvelopeItem? _previousItem;

        public Page_BuildingEnvelope_VM()
        {
            Session.EnvelopeItemsChanged += UpdateXamlBindings;

            PropertyItem<double>.PropertyChanged += UpdatePropertyBags;
            PropertyItem<int>.PropertyChanged += UpdatePropertyBags;
            PropertyItem<string>.PropertyChanged += UpdatePropertyBags;

            _envelopeCalc = new EnvelopeCalculation(EnvelopeItems);
        }

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }
        
        [RelayCommand]
        private void AddEnvelopeItemRoom()
        {
            var newItem = EnvelopeItem.Empty;
            if (IsInfoPresetChecked)
            {
                newItem.FloorLevel = _presetEnvelopeItem.FloorLevel;
                newItem.RoomName = _presetEnvelopeItem.RoomName;
                newItem.RoomUsageType = _presetEnvelopeItem.RoomUsageType;
            }
            if (IsElementPresetChecked)
            {
                newItem.EnvelopeArea = _presetEnvelopeItem.EnvelopeArea;
                newItem.UValue = _presetEnvelopeItem.UValue;
                newItem.ElementInternalId = _presetEnvelopeItem.ElementInternalId;
                newItem.OrientationType = _presetEnvelopeItem.OrientationType;
            }
            if (IsRoomPresetChecked)
            {
                newItem.RoomHeightGross = _presetEnvelopeItem.RoomHeightGross;
                newItem.RoomAreaGross = _presetEnvelopeItem.RoomAreaGross;
                newItem.RoomVolumeGross = _presetEnvelopeItem.RoomVolumeGross;
                newItem.RoomHeightNet = _presetEnvelopeItem.RoomHeightNet;
                newItem.RoomAreaNet = _presetEnvelopeItem.RoomAreaNet;
                newItem.RoomVolumeNet = _presetEnvelopeItem.RoomVolumeNet;
            }
            // Add as new Room
            newItem.RoomNumber = RoomNumbers.Count;
            newItem.ShowRoomData = true;

            Session.SelectedProject?.AddEnvelopeItem(newItem);
            Session.OnEnvelopeItemsChanged();
        }

        [RelayCommand]
        private void AddEnvelopeItem()
        {
            var parentRoomItem = SelectedEnvelopeItem != null ? EnvelopeItems.First(e => e.RoomNumber == SelectedEnvelopeItem.RoomNumber) : EnvelopeItems.Last();
            
            var newItem = EnvelopeItem.Empty;

            newItem.FloorLevel = parentRoomItem.FloorLevel;
            newItem.RoomName = parentRoomItem.RoomName;
            newItem.RoomUsageType = parentRoomItem.RoomUsageType;
            newItem.RoomHeightGross = parentRoomItem.RoomHeightGross;
            newItem.RoomAreaGross = parentRoomItem.RoomAreaGross;
            newItem.RoomVolumeGross = parentRoomItem.RoomVolumeGross;
            newItem.RoomHeightNet = parentRoomItem.RoomHeightNet;
            newItem.RoomAreaNet = parentRoomItem.RoomAreaNet;
            newItem.RoomVolumeNet = parentRoomItem.RoomVolumeNet;

            if (IsElementPresetChecked)
            {
                newItem.EnvelopeArea = _presetEnvelopeItem.EnvelopeArea;
                newItem.UValue = _presetEnvelopeItem.UValue;
                newItem.ElementInternalId = _presetEnvelopeItem.ElementInternalId;
                newItem.OrientationType = _presetEnvelopeItem.OrientationType;
            }

            // Add to existing selected room or append to last room
            //newItem.RoomNumber = SelectedEnvelopeItem?.RoomNumber ?? RoomNumbers.Last();
            newItem.RoomNumber = parentRoomItem.RoomNumber;
            newItem.ShowRoomData = false;

            Session.SelectedProject?.AddEnvelopeItem(newItem);
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

        partial void OnSelectedEnvelopeItemChanged(EnvelopeItem? value)
        {
            // Set the PropertyChanged event handler dynamically for the new selected item
            SetEventHandler(value);
            SetAsSelected(value, EnvelopeItems);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRowSelected))]
        private static EnvelopeItem? _selectedEnvelopeItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isInfoPresetChecked = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isElementPresetChecked = false;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isRoomPresetChecked = false;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public string Title => "Eingabe der thermischen Hüllfläche";
        public ObservableCollection<EnvelopeItem> EnvelopeItems =>
            new ObservableCollection<EnvelopeItem>(
                (Session.SelectedProject?.EnvelopeItems ?? new List<EnvelopeItem>())
                .OrderBy(e => e.RoomNumber)
            );

        public bool IsRowSelected => SelectedEnvelopeItem != null;
        public Visibility PresetActiveVisibility => AnyPresetActive ? Visibility.Visible : Visibility.Collapsed;
        public bool AnyPresetActive => IsInfoPresetChecked || IsElementPresetChecked || IsRoomPresetChecked;
        public int ItemsCount => EnvelopeItems.Count;
        public string ItemsCountString => $"Bereiche/Bauteile angelegt: {ItemsCount}";
        public int ItemsCheckedCount => EnvelopeItems.Where(e => e.IsSelected).ToList().Count;
        public string ItemsCheckedCountString => $"Zeilen markiert: {ItemsCheckedCount}";
        public string DuplicateButtonTooltip => SelectedEnvelopeItem is null ? "" : SelectedEnvelopeItem.ShowRoomData ? "gewählten Raum duplizieren" : "gewählten Bereich duplizieren";
        public string DeleteButtonTooltip => SelectedEnvelopeItem is null ? "" : SelectedEnvelopeItem.ShowRoomData ? "gewählten Raum löschen" : "gewählten Bereich löschen";
        public Visibility NoEntriesVisibility => EnvelopeItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public bool IsNonResidential => Session.SelectedProject?.BuildingUsage == BuildingUsageType.NonResidential;
        public List<int> RoomNumbers => EnvelopeItems.Where(e => e.RoomNumber >= 0).Select(e => e.RoomNumber).Distinct().OrderBy(e => e).ToList();

        public IEnumerable<IPropertyItem> TestPropBag => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.Area, () => _envelopeCalc.UsableArea) { SymbolSubscriptText = "N", Comment = "Nutzbare Grundfläche"},
            new PropertyItem<double>(Symbol.Area, () => _envelopeCalc.EnvelopeArea) { SymbolSubscriptText = "", Comment = "Fläche der thermische Gebäudehülle"},
            new PropertyItem<double>(Symbol.PrimaryEnergyPerArea, () => _envelopeCalc.PrimaryEnergyPerArea),

            new PropertyItem<int>("Berücksichtigung Wärmebrücken", () => (int)_envelopeCalc.ThermalBridgeSurcharge, value => _envelopeCalc.ThermalBridgeSurcharge = (ThermalBridgeSurchargeType)value)
            {
                PropertyValues = ThermalBridgeSurchargeMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.ThermalBridgeSurcharge, () => _envelopeCalc.ThermalBridgeSurchargeValue, value => _envelopeCalc.ThermalBridgeSurchargeValue = value) { IsReadonly = _envelopeCalc.ThermalBridgeSurchargeCustomValue },
        };

        public IEnumerable<IPropertyItem> InfoPresetProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Etage", () => _presetEnvelopeItem.FloorLevel, value => _presetEnvelopeItem.FloorLevel = value),
            new PropertyItem<string>("Raumbezeichnung", () => _presetEnvelopeItem.RoomName, value => _presetEnvelopeItem.RoomName = value),
            new PropertyItem<int>("Zone", () => (int)_presetEnvelopeItem.RoomUsageType, value => _presetEnvelopeItem.RoomUsageType = (RoomUsageType)value)
            {
                PropertyValues = RoomUsageTypeMapping.Values.Cast<object>().ToArray()
            },
        };
        public IEnumerable<IPropertyItem> ElementPresetProperties => new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.Area, () => _presetEnvelopeItem.EnvelopeArea, value => _presetEnvelopeItem.EnvelopeArea = value),
            new PropertyItem<double>(Symbol.UValue, () => _presetEnvelopeItem.UValue, value => _presetEnvelopeItem.UValue = value),
            new PropertyItem<int>("Bauteil", () => _presetEnvelopeItem.ElementInternalId, value => _presetEnvelopeItem.ElementInternalId = value)
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

        public IEnumerable<Element> GetElements() => Session.SelectedProject?.Elements.OrderBy(e => e.InternalId) ?? Enumerable.Empty<Element>();
        public IEnumerable<string> GetOrientationTypeNames() => OrientationTypeMapping.Values;
        public IEnumerable<string> GetUsageZoneNames() => RoomUsageTypeMapping.Values;
        //public IEnumerable<string> GetTempCorrFactorNames() => DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TempCorrectionFactor).Select(e => e.Name);

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>

        private void UpdateXamlBindings()
        {
            // For updating MVVM Capsulated Properties
            SetShowRoomDataFirstInGroupOnly(EnvelopeItems);

            OnPropertyChanged(nameof(EnvelopeItems));
            OnPropertyChanged(nameof(IsRowSelected));
            OnPropertyChanged(nameof(AnyPresetActive));
            OnPropertyChanged(nameof(ItemsCountString));
            OnPropertyChanged(nameof(NoEntriesVisibility));
        }
        private void UpdatePropertyBags()
        {
            OnPropertyChanged(nameof(InfoPresetProperties));
            OnPropertyChanged(nameof(ElementPresetProperties));
            OnPropertyChanged(nameof(RoomPresetProperties));
            OnPropertyChanged(nameof(TestPropBag));
        }

        private void UpdatePropertyDependencies(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(EnvelopeItems));
        }

        private void SetEventHandler(EnvelopeItem? item)
        {
            // Unsubscribe from the previous item's PropertyChanged event if it exists
            if (_previousItem != null) _previousItem.PropertyChanged -= UpdatePropertyDependencies;

            _previousItem = item;

            // Subscribe to the new item's PropertyChanged event if it exists
            if (item != null) item.PropertyChanged += UpdatePropertyDependencies;
        }

        private void SetShowRoomDataFirstInGroupOnly(IEnumerable<EnvelopeItem> items)
        {
            // Ensure the items are ordered by RoomNumber so first-in-group is predictable
            var orderedItems = items.OrderBy(item => item.RoomNumber).ToList();

            int lastRoomNumber = -1;

            foreach (var item in orderedItems)
            {
                if (item.RoomNumber != lastRoomNumber)
                {
                    item.ShowRoomData = true;
                    lastRoomNumber = item.RoomNumber;
                }
                else
                {
                    item.ShowRoomData = false;
                }
            }
        }
        private void SetAsSelected(EnvelopeItem? item, IEnumerable<EnvelopeItem> items)
        {
            if (item == null) return;
            items.ToList().ForEach(i => i.IsSelected = false); // Deselect all items first
            item.IsSelected = true;
        }
    }
}
