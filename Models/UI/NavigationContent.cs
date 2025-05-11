using System.Collections.Generic;

namespace BauphysikToolWPF.Models.UI
{
    public class NavigationContent
    {
        public NavigationPage Page { get; set; } // parent page of the NavigationMenu ListBoxItem
        public string PageName => NavigationNameMapping.TryGetValue(Page, out var name) ? name : Page.ToString();
        public bool IsEnabled { get; set; } = true; // defines if the NavigationMenu ListBoxItem is enabled or not
        public List<NavigationGroupContent>? GroupContent { get; set; }

        public NavigationContent() { } // default constructor for XAML binding
        
        public NavigationContent(NavigationPage page)
        {
            Page = page;
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

    public class NavigationGroupContent
    {
        public string GroupHeader { get; set; } = string.Empty; // defines the header name of a NavigationMenu ListBoxItem
        public List<NavigationContent> ChildPages { get; set; } = new();

        public NavigationGroupContent() { } // default constructor for XAML binding

        public NavigationGroupContent(string groupHeader, List<NavigationPage> pages)
        {
            GroupHeader = groupHeader;
            foreach (var page in pages)
            {
                ChildPages.Add(new NavigationContent(page));
            }
        }
    }
}
