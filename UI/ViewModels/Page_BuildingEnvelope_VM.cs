using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BauphysikToolWPF.Models.Domain.Helper;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_BuildingEnvelope_VM : ObservableObject
    {
        public Page_BuildingEnvelope_VM()
        {
            Session.EnvelopeItemsChanged += UpdateXamlBindings;
        }
        
        [RelayCommand]
        private void AddEnvelopeItem()
        {
            Session.SelectedProject?.AddEnvelopeItem(new EnvelopeItem());
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

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsItemSelected))]
        private EnvelopeItem? _selectedEnvelopeItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        private static bool _isInfoPresetChecked = false;

        [ObservableProperty]
        private static bool _isInfoPresetEmpty = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        private static bool _isElementPresetChecked = false;

        [ObservableProperty]
        private static bool _isElementPresetEmpty = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnyPresetActive))]
        private static bool _isRoomPresetChecked = true;

        [ObservableProperty]
        private static bool _isRoomPresetEmpty = false;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public ObservableCollection<EnvelopeItem> EnvelopeItems => new ObservableCollection<EnvelopeItem>(Session.SelectedProject?.EnvelopeItems ?? new List<EnvelopeItem>());
        public bool IsItemSelected => SelectedEnvelopeItem != null;
        public Visibility AnyPresetActive => IsInfoPresetChecked || IsElementPresetChecked || IsRoomPresetChecked ? Visibility.Visible : Visibility.Collapsed;
        public string ItemsCount => $"Bereiche angelegt: {EnvelopeItems.Count}";
        public Visibility NoEntriesVisibility => EnvelopeItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

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
            OnPropertyChanged(nameof(IsItemSelected));
            OnPropertyChanged(nameof(ItemsCount));
            OnPropertyChanged(nameof(NoEntriesVisibility));
        }
    }
}
