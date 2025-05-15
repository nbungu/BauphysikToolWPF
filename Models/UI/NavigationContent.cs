using System.Collections.Generic;
using System.ComponentModel;

namespace BauphysikToolWPF.Models.UI
{
    public class NavigationContent : INotifyPropertyChanged // To reflect changes being made in a collection -> OnPropertyChanged(nameof(ParentPages)) not needed
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public NavigationPage Page { get; set; } // parent page of the NavigationMenu ListBoxItem
        public string PageName => NavigationNameMapping.TryGetValue(Page, out var name) ? name : Page.ToString();
        
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public List<NavigationGroupContent>? PageGroups { get; set; }

        public NavigationContent() { } // default constructor for XAML binding
        
        public NavigationContent(NavigationPage page, bool isEnabled = true)
        {
            Page = page;
            IsEnabled = isEnabled;
        }

        public static readonly Dictionary<NavigationPage, string> NavigationNameMapping = new()
        {
            { NavigationPage.ProjectPage, "Projektdaten"},
            { NavigationPage.ElementCatalogue, "Bauteilkatalog"},
            { NavigationPage.LayerSetup, "Schichtenaufbau"},
            { NavigationPage.Summary, "Zusammenfassung"},
            { NavigationPage.TemperatureCurve, "Temperaturverlauf"},
            { NavigationPage.GlaserCurve, "Glaser-Diagramm"},
            { NavigationPage.DynamicHeatCalc, "Dynamische Wärmeberechnung"},
            { NavigationPage.BuildingEnvelope, "Gebäudehülle"},
        };
        public static readonly Dictionary<NavigationPage, string> NavigationIconMapping = new()
        {
            { NavigationPage.ProjectPage, "ButtonIcon_Project_Flat" },
            { NavigationPage.ElementCatalogue, "ButtonIcon_Catalogue_Flat" },
            { NavigationPage.LayerSetup, "ButtonIcon_Layers_Flat" },
            { NavigationPage.Summary, "ButtonIcon_Summary_Flat" },
            { NavigationPage.TemperatureCurve, "ButtonIcon_LayerTemps_Flat" },
            { NavigationPage.GlaserCurve, "ButtonIcon_Glaser_Flat" },
            { NavigationPage.DynamicHeatCalc, "ButtonIcon_Dynamic_Flat" },
            { NavigationPage.BuildingEnvelope, "ButtonIcon_Envelope_Flat" },
        };
    }

    public class NavigationGroupContent : INotifyPropertyChanged // To reflect changes being made in a collection -> OnPropertyChanged(nameof(AvailableChildGroups)) not needed
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
        public string GroupHeader { get; set; } = string.Empty; // defines the header name of a NavigationMenu ListBoxItem
        public List<NavigationContent> ChildPages { get; set; } = new();
        
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public NavigationGroupContent() { } // default constructor for XAML binding

        public NavigationGroupContent(string groupHeader, List<NavigationPage> pages, bool isGroupEnabled = true)
        {
            GroupHeader = groupHeader;
            IsEnabled = isGroupEnabled;
            foreach (var page in pages)
            {
                ChildPages.Add(new NavigationContent(page, IsEnabled));
            }
        }
    }
}
