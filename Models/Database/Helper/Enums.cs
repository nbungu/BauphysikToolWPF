using System.Collections.Generic;

namespace BauphysikToolWPF.Models.Database.Helper
{
    public class Enums
    {
        public enum ConstructionType
        {
            NotDefined = 0,
            Aussenwand,
            AussenwandGegErdreich,
            Innenwand,
            WhgTrennwand,
            GeschossdeckeGegAussenluft,
            DeckeGegUnbeheizt,
            InnenwandGegUnbeheizt,
            Flachdach,
            Schraegdach,
            Umkehrdach,
            Bodenplatte,
            Kellerdecke,
        }
        public static readonly Dictionary<ConstructionType, string> ConstructionTypeMapping = new()
        {
            { ConstructionType.NotDefined, "Nicht definiert" },
            { ConstructionType.Aussenwand, "Außenwand" },
            { ConstructionType.AussenwandGegErdreich, "Außenwand gegen Erdreich" },
            { ConstructionType.Innenwand, "Innenwand" },
            { ConstructionType.WhgTrennwand, "Wohnungstrennwand" },
            { ConstructionType.GeschossdeckeGegAussenluft, "Geschossdecke gegen Außenluft" },
            { ConstructionType.DeckeGegUnbeheizt, "Decke gegen unbeheizten Raum" },
            { ConstructionType.InnenwandGegUnbeheizt, "Innenwand gegen unbeheizten Raum" },
            { ConstructionType.Flachdach, "Flachdach" },
            { ConstructionType.Schraegdach, "Schrägdach" },
            { ConstructionType.Umkehrdach, "Umkehrdach" },
            { ConstructionType.Bodenplatte, "Bodenplatte" },
            { ConstructionType.Kellerdecke, "Kellerdecke" }
        };

        public enum ConstructionDirection
        {
            Vertical,
            Horizontal,
        }

        public static readonly Dictionary<ConstructionDirection, string> SubConstructionDirectionMapping = new()
        {
            { ConstructionDirection.Vertical, "Vertikal" },
            { ConstructionDirection.Horizontal, "Horizontal" }
        };


        public enum DocumentSourceType
        {
            NotDefined = 0,
            GEG_Anlage1,
            GEG_Anlage2,
            GEG_Anlage7,
            DIN_4108_2_Tabelle3,
            DIN_V_18599_10_AnhangA,
            DIN_V_18599_10_Tabelle_E1,
        }
        public static readonly Dictionary<DocumentSourceType, string> RequirementSourceTypeMapping = new()
        {
            { DocumentSourceType.NotDefined, "Nicht definiert" },
            { DocumentSourceType.GEG_Anlage1, "GEG Anlage 1" },
            { DocumentSourceType.GEG_Anlage2, "GEG Anlage 2" },
            { DocumentSourceType.GEG_Anlage7, "GEG Anlage 7" },
            { DocumentSourceType.DIN_4108_2_Tabelle3, "DIN 4108-2 Tabelle 3" },
            { DocumentSourceType.DIN_V_18599_10_AnhangA, "DIN V 18599-10 Anhang A" }
        };

        public enum MaterialCategory
        {
            NotDefined = 0,
            Insulation,
            Concrete,
            Wood,
            Masonry,
            Plasters,
            Sealant,
            Air
        }
        public static readonly Dictionary<MaterialCategory, string> MaterialCategoryMapping = new()
        {
            { MaterialCategory.NotDefined, "Alle" },
            { MaterialCategory.Insulation, "Wärmedämmung" },
            { MaterialCategory.Concrete, "Beton" },
            { MaterialCategory.Wood, "Holz" },
            { MaterialCategory.Masonry, "Mauerwerk" },
            { MaterialCategory.Plasters, "Mörtel und Putze" },
            { MaterialCategory.Sealant, "Dichtbahnen, Folien" },
            { MaterialCategory.Air, "Luftschicht" },
        };

        public static readonly Dictionary<MaterialCategory, double> DefaultLayerWidthMapping = new()
        {
            { MaterialCategory.NotDefined, 10.0 },
            { MaterialCategory.Insulation, 16.0 },
            { MaterialCategory.Concrete, 24.0 },
            { MaterialCategory.Wood, 4.8 },
            { MaterialCategory.Masonry, 24.0 },
            { MaterialCategory.Plasters, 1.0 },
            { MaterialCategory.Sealant, 0.01 },
            { MaterialCategory.Air, 4.0 }
        };

        // DIN V 18599-10 Anhang A
        public enum RoomUsageType
        {
            Wohnen = 0,
            Einzelbuero,
            GruppenBueroBis6Ap,
            GrossRaumBueroAb7Ap,
            Besprechung,
            Schalterhalle,
            Einzelhandel,
            EinzelhandelMitKühlprodukten,
            Klassenzimmer,
            Hoersaal,
            Bettenzimmer,
            Hotelzimmer,
            Kantine,
            Restaurant,
            KuecheInNWG,
            KuecheLager,
            WCSanitaerInNWG,
            SonstigeAufenthalt,
            NebenflaechenOhneAufenthalt,
            Verkehrsflaechen,
            LagerTechnikArchiv,
            Rechenzentrum,
            GewerbeUndIndustrieHalleSA,
            GewerbeUndIndustrieHalleMSA,
            GewerbeUndIndustrieHalleLA,
            Zuschauerbereich,
            FoyerTheater,
            BuehneTheater,
            MesseKongress,
            AusstellungMuseum,
            BibLesesaal,
            BibFreihandbereich,
            BibMagazinDepot,
            Turnhalle,
            ParkhausBueroPrivatnutzung,
            ParkhausOeffentlicheNutzung,
            Saunabereich,
            Fitnessraum,
            Labor,
            UntersuchungsBehandlungsraeume,
            Spezialpflegebereiche,
            FlurAllgemeinerPflegebereich,
            ArztpraxenTherapeutischePraxen,
            LagerLogistikhalle,
        }

        public static readonly Dictionary<RoomUsageType, string> RoomUsageTypeMapping = new()
        {
            { RoomUsageType.Wohnen, "Wohnen" },
            { RoomUsageType.Einzelbuero, "Einzelbüro" },
            { RoomUsageType.GruppenBueroBis6Ap, "Gruppenbüro (zwei bis sechs Arbeitsplätze)" },
            { RoomUsageType.GrossRaumBueroAb7Ap, "Großraumbüro (ab sieben Arbeitsplätze)" },
            { RoomUsageType.Besprechung, "Besprechung, Sitzung, Seminar" },
            { RoomUsageType.Schalterhalle, "Schalterhalle" },
            { RoomUsageType.Einzelhandel, "Einzelhandel/Kaufhaus" },
            { RoomUsageType.EinzelhandelMitKühlprodukten, "Einzelhandel/Kaufhaus (Lebensmittelabteilung mit Kühlprodukten)" },
            { RoomUsageType.Klassenzimmer, "Klassenzimmer (Schule), Gruppenraum (Kindergarten)" },
            { RoomUsageType.Hoersaal, "Hörsaal, Auditorium" },
            { RoomUsageType.Bettenzimmer, "Bettenzimmer" },
            { RoomUsageType.Hotelzimmer, "Hotelzimmer" },
            { RoomUsageType.Kantine, "Kantine" },
            { RoomUsageType.Restaurant, "Restaurant" },
            { RoomUsageType.KuecheInNWG, "Küchen in Nichtwohngebäuden" },
            { RoomUsageType.KuecheLager, "Küche – Vorbereitung, Lager" },
            { RoomUsageType.WCSanitaerInNWG, "WC und Sanitärräume in Nichtwohngebäuden" },
            { RoomUsageType.SonstigeAufenthalt, "Sonstige Aufenthaltsräume" },
            { RoomUsageType.NebenflaechenOhneAufenthalt, "Nebenflächen (ohne Aufenthaltsräume)" },
            { RoomUsageType.Verkehrsflaechen, "Verkehrsflächen" },
            { RoomUsageType.LagerTechnikArchiv, "Lager, Technik, Archiv" },
            { RoomUsageType.Rechenzentrum, "Rechenzentrum" },
            { RoomUsageType.GewerbeUndIndustrieHalleSA, "Gewerbliche und industrielle Hallen – schwere Arbeit, stehende Tätigkeit" },
            { RoomUsageType.GewerbeUndIndustrieHalleMSA, "Gewerbliche und industrielle Hallen – mittelschwere Arbeit, überwiegend stehende Tätigkeit" },
            { RoomUsageType.GewerbeUndIndustrieHalleLA, "Gewerbliche und industrielle Hallen – leichte Arbeit, überwiegend sitzende Tätigkeit" },
            { RoomUsageType.Zuschauerbereich, "Zuschauerbereich (Theater und Veranstaltungsbauten)" },
            { RoomUsageType.FoyerTheater, "Foyer (Theater und Veranstaltungsbauten)" },
            { RoomUsageType.BuehneTheater, "Bühne (Theater und Veranstaltungsbauten)" },
            { RoomUsageType.MesseKongress, "Messe/Kongress" },
            { RoomUsageType.AusstellungMuseum, "Ausstellungsräume und Museum mit konservatorischen Anforderungen" },
            { RoomUsageType.BibLesesaal, "Bibliothek – Lesesaal" },
            { RoomUsageType.BibFreihandbereich, "Bibliothek – Freihandbereich" },
            { RoomUsageType.BibMagazinDepot, "Bibliothek – Magazin und Depot" },
            { RoomUsageType.Turnhalle, "Turnhalle (ohne Zuschauerbereich)" },
            { RoomUsageType.ParkhausBueroPrivatnutzung, "Parkhäuser (Büro- und Privatnutzung)" },
            { RoomUsageType.ParkhausOeffentlicheNutzung, "Parkhäuser (öffentliche Nutzung)" },
            { RoomUsageType.Saunabereich, "Saunabereich" },
            { RoomUsageType.Fitnessraum, "Fitnessraum" },
            { RoomUsageType.Labor, "Labor" },
            { RoomUsageType.UntersuchungsBehandlungsraeume, "Untersuchungs- und Behandlungsräume" },
            { RoomUsageType.Spezialpflegebereiche, "Spezialpflegebereiche" },
            { RoomUsageType.FlurAllgemeinerPflegebereich, "Flure des allgemeinen Pflegebereichs" },
            { RoomUsageType.ArztpraxenTherapeutischePraxen, "Arztpraxen und Therapeutische Praxen" },
            { RoomUsageType.LagerLogistikhalle, "Lagerhallen, Logistikhallen" },
        };

        public enum ReferenceLocation
        {
            Bremerhaven = 1,
            Rostock,
            Hamburg,
            Potsdam,
            Essen,
            BadMarienberg,
            Kassel,
            Braunlage,
            Chemnitz,
            Hof,
            Fichtelberg,
            Mannheim,
            Passau,
            Stoetten,
            GarmischPartenkirchen,
        }
        public static readonly Dictionary<ReferenceLocation, string> ReferenceLocationMapping = new()
        {
            { ReferenceLocation.Bremerhaven, "Bremerhaven" },
            { ReferenceLocation.Rostock, "Rostock" },
            { ReferenceLocation.Hamburg, "Hamburg" },
            { ReferenceLocation.Potsdam, "Potsdam" },
            { ReferenceLocation.Essen, "Essen" },
            { ReferenceLocation.BadMarienberg, "Bad Marienberg" },
            { ReferenceLocation.Kassel, "Kassel" },
            { ReferenceLocation.Braunlage, "Braunlage" },
            { ReferenceLocation.Chemnitz, "Chemnitz" },
            { ReferenceLocation.Hof, "Hof" },
            { ReferenceLocation.Fichtelberg, "Fichtelberg" },
            { ReferenceLocation.Mannheim, "Mannheim" },
            { ReferenceLocation.Passau, "Passau" },
            { ReferenceLocation.Stoetten, "Stötten" },
            { ReferenceLocation.GarmischPartenkirchen, "Garmisch-Partenkirchen" }
        };

        public enum Month
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }
        public static readonly Dictionary<Month, string> MonthMapping = new()
        {
            { Month.January, "Januar" },
            { Month.February, "Februar" },
            { Month.March, "März" },
            { Month.April, "April" },
            { Month.May, "Mai" },
            { Month.June, "Juni" },
            { Month.July, "Juli" },
            { Month.August, "August" },
            { Month.September, "September" },
            { Month.October, "Oktober" },
            { Month.November, "November" },
            { Month.December, "Dezember" }
        };
    }
}
