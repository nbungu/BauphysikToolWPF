using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.Services.UI
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
        EnvelopeSummary = 21
    }

    public static class NavigationManager
    {
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
                    new NavigationGroupContent("", new List<NavigationPage>(2) { NavigationPage.EnvelopeSummary }),
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
            { NavigationPage.EnvelopeSummary, "Zusammenfassung der Hüllflächenergebnisse und Anpassung der Berechnungs-Randbedingungen."},
        };
        public static readonly Dictionary<NavigationPage, string> PageIconMapping = new()
        {
            { NavigationPage.ProjectData, "ButtonIcon_House_B" },
            { NavigationPage.ElementCatalogue, "ButtonIcon_Elements_B" },
            { NavigationPage.LayerSetup, "ButtonIcon_Layers_Flat" },
            { NavigationPage.Summary, "ButtonIcon_Summary_Flat" },
            { NavigationPage.TemperatureCurve, "ButtonIcon_LayerTemps_Flat" },
            { NavigationPage.GlaserCurve, "ButtonIcon_Glaser_Flat" },
            { NavigationPage.DynamicHeatCalc, "ButtonIcon_Dynamic_Flat" },
            { NavigationPage.BuildingEnvelope, "Favicon" },
            { NavigationPage.EnvelopeSummary, "ButtonIcon_Summary_Flat" },
        };

        public static void UpdateParentSelectedState(this NavigationPage parentPage)
        {
            // When targetPage is NOT a parent page, return
            if (!ParentPageDictionary.ContainsKey(parentPage)) return;
                
            ParentPages.ForEach(p => p.IsSelected = p.Page == parentPage);
        }
        public static void UpdateChildSelectedState(this NavigationPage childPage)
        {
            // When targetPage is a parent page, return
            if (ParentPageDictionary.ContainsKey(childPage)) return;

            foreach (var parent in ParentPages)
            {
                if (parent.PageGroups == null) continue;

                foreach (var group in parent.PageGroups)
                {
                    foreach (var child in group.ChildPages)
                    {
                        child.IsSelected = child.Page == childPage;
                    }
                }
            }
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
        }
        public static void UpdateParentEnabledStates()
        {
            bool isProjectLoaded = !Session.SelectedProject?.IsNewEmptyProject ?? false;
            ParentPages.ForEach(p => p.IsEnabled = isProjectLoaded);
        }

        public static BitmapImage? GetBitmapImageFromAppResources(string resourceKey) => System.Windows.Application.Current.Resources[resourceKey] as BitmapImage;
        public static DataTemplate? GetPageFromAppResources(string resourceKey) => System.Windows.Application.Current.Resources[resourceKey] as DataTemplate;
    }
}
