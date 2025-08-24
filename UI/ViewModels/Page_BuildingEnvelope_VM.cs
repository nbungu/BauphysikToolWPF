using BauphysikToolWPF.Calculation;
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
using static BauphysikToolWPF.Calculation.EnvelopeCalculation;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.Domain.Enums;
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
        private IDialogService _dialogService;

        public Page_BuildingEnvelope_VM()
        {
            _envelopeCalc = new EnvelopeCalculation(OrderedEnvelopeItems);
            _dialogService = new DialogService();

            Session.SelectedProject.AssignInternalIdsToEnvelopeItems();

            SetShowRoomDataFirstInGroupOnly(OrderedEnvelopeItems);

            Session.NewProjectAdded += UpdateNewProjectAdded;
            Session.EnvelopeItemsChanged += UpdateXamlBindings;
            PropertyItem<double>.PropertyChanged += UpdatePropertyBags;
            PropertyItem<int>.PropertyChanged += UpdatePropertyBags;
            PropertyItem<string>.PropertyChanged += UpdatePropertyBags;
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

            if (IsElementPresetChecked)
            {
                newItem.EnvelopeArea = _presetEnvelopeItem.EnvelopeArea;
                newItem.UValue = _presetEnvelopeItem.UValue;
                newItem.ElementInternalId = _presetEnvelopeItem.ElementInternalId;
                newItem.OrientationType = _presetEnvelopeItem.OrientationType;
            }
            if (IsRoomPresetChecked)
            {
                newItem.FloorLevel = _presetEnvelopeItem.FloorLevel;
                newItem.RoomName = _presetEnvelopeItem.RoomName;
                newItem.RoomUsageType = _presetEnvelopeItem.RoomUsageType;

                newItem.RoomHeightGross = _presetEnvelopeItem.RoomHeightGross;
                newItem.RoomAreaGross = _presetEnvelopeItem.RoomAreaGross;
                newItem.RoomVolumeGross = _presetEnvelopeItem.RoomVolumeGross;
                newItem.RoomHeightNet = _presetEnvelopeItem.RoomHeightNet;
                newItem.RoomAreaNet = _presetEnvelopeItem.RoomAreaNet;
                newItem.RoomVolumeNet = _presetEnvelopeItem.RoomVolumeNet;
            }
            // Add as new Room
            newItem.RoomNumber = RoomNumbers.LastOrDefault(0) + 1;
            newItem.ShowRoomData = true;

            Session.SelectedProject?.AddEnvelopeItem(newItem);
            Session.OnEnvelopeItemsChanged();
        }

        [RelayCommand]
        private void AddEnvelopeItem()
        {
            if (OrderedEnvelopeItems.Count == 0) return;

            var parentRoomItem = SelectedEnvelopeItem != null ? OrderedEnvelopeItems.First(e => e.RoomNumber == SelectedEnvelopeItem.RoomNumber) : OrderedEnvelopeItems.Last();
            
            var newItem = EnvelopeItem.Empty;

            //newItem.FloorLevel = parentRoomItem.FloorLevel;
            //newItem.RoomName = parentRoomItem.RoomName;
            //newItem.RoomUsageType = parentRoomItem.RoomUsageType;
            //newItem.RoomHeightGross = parentRoomItem.RoomHeightGross;
            //newItem.RoomAreaGross = parentRoomItem.RoomAreaGross;
            //newItem.RoomVolumeGross = parentRoomItem.RoomVolumeGross;
            //newItem.RoomHeightNet = parentRoomItem.RoomHeightNet;
            //newItem.RoomAreaNet = parentRoomItem.RoomAreaNet;
            //newItem.RoomVolumeNet = parentRoomItem.RoomVolumeNet;

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
            Session.SelectedProject?.RemoveEnvelopeItemById(SelectedEnvelopeItem?.InternalId ?? -1);
            Session.OnEnvelopeItemsChanged();
        }

        [RelayCommand]
        private void DeleteAllEnvelopeItems()
        {
            MessageBoxResult result = _dialogService.ShowDeleteConfirmationDialog();

            switch (result)
            {
                case MessageBoxResult.Yes:
                    Session.SelectedProject?.EnvelopeItems.Clear();
                    Session.OnEnvelopeItemsChanged();
                    break;
                case MessageBoxResult.Cancel:
                    // Do nothing, user cancelled the action
                    break;
            }
        }

        [RelayCommand]
        private void DuplicateEnvelopeItem()
        {
            Session.SelectedProject?.DuplicateEnvelopeItemById(SelectedEnvelopeItem?.InternalId ?? -1);
            Session.OnEnvelopeItemsChanged();
        }

        partial void OnSelectedEnvelopeItemChanged(EnvelopeItem? value)
        {
            // Set the PropertyChanged event handler dynamically for the new selected item
            SetEventHandler(value);
            SetAsSelected(value, OrderedEnvelopeItems);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRowSelected))]
        [NotifyPropertyChangedFor(nameof(SelectedRoomName))]
        [NotifyPropertyChangedFor(nameof(AddToSelectedRoomLabel))]
        private static EnvelopeItem? _selectedEnvelopeItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isElementPresetChecked = false;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        [NotifyPropertyChangedFor(nameof(PresetActiveVisibility))]
        private static bool _isRoomPresetChecked = true;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public string Title => "Eingabe der thermischen Hüllfläche";
        public ObservableCollection<EnvelopeItem> OrderedEnvelopeItems =>
            new ObservableCollection<EnvelopeItem>(
                (Session.SelectedProject?.EnvelopeItems ?? new List<EnvelopeItem>())
                .OrderBy(e => e.RoomNumber)
            );

        public bool IsRowSelected => SelectedEnvelopeItem != null;
        public string SelectedRoomName => SelectedEnvelopeItem != null ? $"Selektion: Raum-Nr. {SelectedEnvelopeItem.RoomNumber}" : "kein Raum gewählt";
        public string AddToSelectedRoomLabel => SelectedEnvelopeItem != null ? $"Bauteil hinzufügen (Raum-Nr. {SelectedEnvelopeItem.RoomNumber})" : "Bauteil hinzufügen";
        public Visibility PresetActiveVisibility => AnyPresetActive ? Visibility.Visible : Visibility.Collapsed;
        public bool AnyPresetActive => IsElementPresetChecked || IsRoomPresetChecked;
        public bool HasItems => ItemsCount > 0;
        public int ItemsCount => OrderedEnvelopeItems.Count;
        public string ItemsCountString => $"Bereiche/Bauteile: {ItemsCount}";
        public int RoomCount => RoomNumbers.Count;
        public string RoomCountString => $"Räume: {RoomCount}";
        public Visibility NoEntriesVisibility => OrderedEnvelopeItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public bool IsNonResidential => Session.SelectedProject?.BuildingUsage == BuildingUsageType.NonResidential;
        public List<int> RoomNumbers => OrderedEnvelopeItems.Where(e => e.RoomNumber >= 0).Select(e => e.RoomNumber).Distinct().OrderBy(e => e).ToList();

        public IEnumerable<IPropertyItem> TestPropBag => new List<IPropertyItem>()
        {
            new PropertyItem<double>("Nettogrundfläche", Symbol.Area, () => _envelopeCalc.UsableArea) { SymbolSubscriptText = "NGF", Comment = "Als Bezugsfläche wird die Nettogrundfläche verwendet: Die im konditionierten Gebäudevolumen zur Verfügung stehende nutzbare Fläche."},
            new PropertyItem<double>("thermische Hüllfläche", Symbol.Area, () => _envelopeCalc.EnvelopeArea) { SymbolSubscriptText = "", Comment = "Fläche der thermische Gebäudehülle: Summer aller wärmeübertragenden Umfassungsflächen"},
            new PropertyItem<double>("Nettoraumvolumen", Symbol.Volume, () => _envelopeCalc.HeatedRoomVolume) { SymbolSubscriptText = "", Comment = "Volumen einer konditionierten Zone bzw. eines gesamten Gebäudes, das dem Luftaustausch unterliegt. Es wird aus der entsprechenden Nettogrundfläche durch Multiplikation mit der lichten Raumhöhe ermittelt."},
            new PropertyItem<double>(Symbol.PrimaryEnergy, () => _envelopeCalc.AnnualPrimaryEnergy),
            new PropertyItem<double>(Symbol.PrimaryEnergyPerArea, () => _envelopeCalc.AnnualPrimaryEnergyPerArea),
            new PropertyItem<double>("Transmissionswärmeverlust", Symbol.TransmissionHeatTransferCoef, () => _envelopeCalc.AnnualPrimaryEnergy),
            new PropertyItem<double>(Symbol.SpecificHeatTransmissionLoss, () => _envelopeCalc.AnnualPrimaryEnergyPerArea),

            new PropertyItem<int>("Berücksichtigung Wärmebrücken", () => (int)_envelopeCalc.ThermalBridgeSurcharge, value => _envelopeCalc.ThermalBridgeSurcharge = (ThermalBridgeSurchargeType)value)
            {
                PropertyValues = ThermalBridgeSurchargeMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.ThermalBridgeSurcharge, () => _envelopeCalc.ThermalBridgeSurchargeValue, value => _envelopeCalc.ThermalBridgeSurchargeValue = value) { IsReadonly = !_envelopeCalc.IsThermalBridgeSurchargeCustomValue },
            new PropertyItem<int>("Berücksichtigung Fx-Wert", () => (int)_envelopeCalc.Fx, value => _envelopeCalc.Fx = (FxType)value)
            {
                PropertyValues = FxTypeMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double?>("Globaler Fx-Wert", Symbol.TempCorrectionFactor, () => _envelopeCalc.FxGlobalValue, value => _envelopeCalc.FxGlobalValue = value) { IsReadonly = !_envelopeCalc.IsFxCustomValue },
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
            new PropertyItem<string>("Etage", () => _presetEnvelopeItem.FloorLevel, value => _presetEnvelopeItem.FloorLevel = value),
            new PropertyItem<string>("Raumbezeichnung", () => _presetEnvelopeItem.RoomName, value => _presetEnvelopeItem.RoomName = value),
            new PropertyItem<int>("Zone", () => (int)_presetEnvelopeItem.RoomUsageType, value => _presetEnvelopeItem.RoomUsageType = (RoomUsageType)value)
            {
                PropertyValues = RoomUsageTypeMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>("Höhe", Symbol.Length, () => _presetEnvelopeItem.RoomHeightNet, value => _presetEnvelopeItem.RoomHeightNet = value) { SymbolBaseText = "h", SymbolSubscriptText = "netto" },
            new PropertyItem<double>(Symbol.Area, () => _presetEnvelopeItem.RoomAreaNet, value => _presetEnvelopeItem.RoomAreaNet = value) { SymbolSubscriptText = "netto" },
            new PropertyItem<double>(Symbol.Volume, () => _presetEnvelopeItem.RoomVolumeNet, value => _presetEnvelopeItem.RoomVolumeNet = value) { SymbolSubscriptText = "netto" },
            new PropertyItem<double>("Höhe", Symbol.Length, () => _presetEnvelopeItem.RoomHeightGross, value => _presetEnvelopeItem.RoomHeightGross = value)  { SymbolBaseText = "h" },
            new PropertyItem<double>(Symbol.Area, () => _presetEnvelopeItem.RoomAreaGross, value => _presetEnvelopeItem.RoomAreaGross = value),
            new PropertyItem<double>(Symbol.Volume, () => _presetEnvelopeItem.RoomVolumeGross, value => _presetEnvelopeItem.RoomVolumeGross = value),
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
            SetShowRoomDataFirstInGroupOnly(OrderedEnvelopeItems);

            OnPropertyChanged(nameof(OrderedEnvelopeItems));
            OnPropertyChanged(nameof(IsRowSelected));
            OnPropertyChanged(nameof(SelectedRoomName));
            OnPropertyChanged(nameof(AnyPresetActive));
            OnPropertyChanged(nameof(ItemsCountString));
            OnPropertyChanged(nameof(RoomCountString));
            OnPropertyChanged(nameof(NoEntriesVisibility));
        }
        private void UpdatePropertyBags()
        {
            OnPropertyChanged(nameof(ElementPresetProperties));
            OnPropertyChanged(nameof(RoomPresetProperties));
            OnPropertyChanged(nameof(TestPropBag));
        }

        private void UpdateEnvelopeItemDependencies(object? sender, PropertyChangedEventArgs e)
        {
            //SetShowRoomDataFirstInGroupOnly(OrderedEnvelopeItems);
            //OnPropertyChanged(nameof(OrderedEnvelopeItems));
            OnPropertyChanged(nameof(TestPropBag));
        }
        private void UpdateNewProjectAdded()
        {
            // Update InternalIds
            Session.SelectedProject.AssignInternalIdsToEnvelopeItems(true);

            // update UI
            UpdateXamlBindings();
        }

        private void SetEventHandler(EnvelopeItem? item)
        {
            // Unsubscribe from the previous item's PropertyChanged event if it exists
            if (_previousItem != null) _previousItem.PropertyChanged -= UpdateEnvelopeItemDependencies;

            _previousItem = item;

            // Subscribe to the new item's PropertyChanged event if it exists
            if (item != null) item.PropertyChanged += UpdateEnvelopeItemDependencies;
        }

        private void SetShowRoomDataFirstInGroupOnly(IEnumerable<EnvelopeItem> orderedItems)
        {
            // Ensure the items are ordered by RoomNumber so first-in-group is predictable
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
