using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using BauphysikToolWPF.Models.UI;

namespace BauphysikToolWPF.Services.Application
{
    // Top-level type. Defined outside of class. Part of namespace BauphysikToolWPF. Accessible from whole application
    public enum NavigationPage
    {
        // see in MainWindow.xaml the List of ItemsSource for indices of the ListBoxItems (Pages)
        ProjectData = 0,
        ElementCatalogue = 10,
        LayerSetup = 11,
        Summary = 12,
        TemperatureCurve = 13,
        GlaserCurve = 14,
        DynamicHeatCalc = 15,
        BuildingEnvelope = 20,
        ThermalBridges = 21,
        EnvelopeSummary = 22
    }

    public static class NavigationManager
    {
        // If true, only one page can be selected at a time.
        // If false, multiple pages can be shown as selected (e.g. current parent page is always shown as selected + the current child page).
        public static bool SingleStateSelection { get; set; } = true;
        public static Dictionary<NavigationPage, NavigationContent> ParentPageDictionary => ParentPages.ToDictionary(p => p.Page, p => p);

        public static List<NavigationContent> ParentPages = new List<NavigationContent>()
        {
            new NavigationContent(NavigationPage.ProjectData, false),
            new NavigationContent(NavigationPage.ElementCatalogue, false)
            {
                PageGroups = new List<NavigationGroupContent>()
                {
                    new NavigationGroupContent("KONSTRUKTION", new List<NavigationPage>(2) { NavigationPage.LayerSetup, NavigationPage.Summary }),
                    new NavigationGroupContent("ERGEBNISSE", new List<NavigationPage>(3) { NavigationPage.TemperatureCurve, NavigationPage.GlaserCurve, NavigationPage.DynamicHeatCalc }),
                }
            },
            new NavigationContent(NavigationPage.BuildingEnvelope, false)
            {
                PageGroups = new List<NavigationGroupContent>()
                {
                    new NavigationGroupContent("", new List<NavigationPage>(2) { NavigationPage.ThermalBridges, NavigationPage.EnvelopeSummary }),
                }
            }
        };
        public static readonly Dictionary<NavigationPage, string> PageNameMapping = new()
        {
            { NavigationPage.ProjectData, "Projektdaten"},
            { NavigationPage.ElementCatalogue, "Bauteilkatalog"},
            { NavigationPage.LayerSetup, "Bearbeiten"},
            { NavigationPage.Summary, "Zusammenfassung"},
            { NavigationPage.TemperatureCurve, "Temperaturverlauf"},
            { NavigationPage.GlaserCurve, "Glaser-Diagramm"},
            { NavigationPage.DynamicHeatCalc, "Dynamisch"},
            { NavigationPage.BuildingEnvelope, "Gebäudehülle"},
            { NavigationPage.ThermalBridges, "Wärmebrücken"},
            { NavigationPage.EnvelopeSummary, "Zusammenfassung"},
        };
        public static readonly Dictionary<NavigationPage, string> PageTooltipMapping = new()
        {
            { NavigationPage.ProjectData, "Übersicht und Eingabe der Projektdaten."},
            { NavigationPage.ElementCatalogue, "Übersicht aller projektbezogenen Konstruktionen."},
            { NavigationPage.LayerSetup, "Erstellen der Konstuktion mit beliebigen Schichten und Materialien; Eingabe der Umgebungsrandbedingungen."},
            { NavigationPage.Summary, "Vertikal- und Querschnitt der Konstruktion; Zusammenfassung der bauphysikalischen Kennwerte."},
            { NavigationPage.TemperatureCurve, "Zeigt stationären Temperaturverlauf im Bauteilquerschnitt."},
            { NavigationPage.GlaserCurve, "Zeigt Glaser-Diagramm im Bauteilquerschnitt."},
            { NavigationPage.DynamicHeatCalc, "Dynamische Wärmeberechnung"},
            { NavigationPage.BuildingEnvelope, "Eingabe der Gebäudehülle"},
            { NavigationPage.ThermalBridges, "Eingabe der Wärmebrücken"},
            { NavigationPage.EnvelopeSummary, "Zusammenfassung der Hüllflächenergebnisse und Anpassung der Berechnungs-Randbedingungen."},
        };
        public static readonly Dictionary<NavigationPage, string> PageIconMapping = new()
        {
            { NavigationPage.ProjectData, "ButtonIcon_Info_B" },
            { NavigationPage.ElementCatalogue, "ButtonIcon_Grid_B" },
            { NavigationPage.LayerSetup, "ButtonIcon_Layers_Flat" },
            { NavigationPage.Summary, "ButtonIcon_Summary_Flat" },
            { NavigationPage.TemperatureCurve, "ButtonIcon_LayerTemps_Flat" },
            { NavigationPage.GlaserCurve, "ButtonIcon_Glaser_Flat" },
            { NavigationPage.DynamicHeatCalc, "ButtonIcon_Dynamic_Flat" },
            { NavigationPage.BuildingEnvelope, "Favicon" },
            { NavigationPage.ThermalBridges, "ButtonIcon_Summary_Flat" },
            { NavigationPage.EnvelopeSummary, "ButtonIcon_Summary_Flat" },
        };

        public static void UpdateParentSelectedState(this NavigationPage parentPage)
        {
            // When targetPage is NOT a parent page, return
            if (!ParentPageDictionary.ContainsKey(parentPage)) return;

            ParentPages.ForEach(p => p.IsSelected = p.Page == parentPage);

            if (SingleStateSelection) UnselectChildPages();
        }

        public static void UpdateChildSelectedState(this NavigationPage childPage, List<NavigationGroupContent> navigationGroup)
        {
            // When targetPage is a parent page, return
            if (ParentPageDictionary.ContainsKey(childPage)) return;
            
            foreach (var group in navigationGroup)
            {
                foreach (var child in group.ChildPages)
                {
                    child.IsSelected = child.Page == childPage;
                }
            }

            if (SingleStateSelection) UnselectParentPages();
        }
        public static void UpdateParentEnabledStates()
        {
            bool isProjectLoaded = !Session.SelectedProject?.IsNewEmptyProject ?? false;
            ParentPages.ForEach(p => p.IsEnabled = isProjectLoaded);
        }

        public static BitmapImage? GetBitmapImageFromAppResources(string resourceKey) => System.Windows.Application.Current.Resources[resourceKey] as BitmapImage;
        public static DataTemplate? GetPageFromAppResources(string resourceKey) => System.Windows.Application.Current.Resources[resourceKey] as DataTemplate;

        #region private

        private static void UnselectChildPages()
        {
            ParentPages.ForEach(p => p.PageGroups?.ForEach(g => g.ChildPages.ForEach(c => c.IsSelected = false)));
        }

        private static void UnselectParentPages()
        {
            ParentPages.ForEach(p => p.IsSelected = false);
        }

        #endregion
    }
}
