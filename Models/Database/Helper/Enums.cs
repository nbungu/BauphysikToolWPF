using System.Collections.Generic;

namespace BauphysikToolWPF.Models.Database.Helper
{
    public class Enums
    {
        public enum ConstructionType
        {
            NotDefined,
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


        public enum RequirementSourceType
        {
            NotDefined,
            GEG_Anlage1,
            GEG_Anlage2,
            GEG_Anlage7,
            DIN_4108_2_Tabelle3,
            DIN_V_18599_10_AnhangA,
        }
        public static readonly Dictionary<RequirementSourceType, string> RequirementSourceTypeMapping = new()
        {
            { RequirementSourceType.NotDefined, "Nicht definiert" },
            { RequirementSourceType.GEG_Anlage1, "GEG Anlage 1" },
            { RequirementSourceType.GEG_Anlage2, "GEG Anlage 2" },
            { RequirementSourceType.GEG_Anlage7, "GEG Anlage 7" },
            { RequirementSourceType.DIN_4108_2_Tabelle3, "DIN 4108-2 Tabelle 3" },
            { RequirementSourceType.DIN_V_18599_10_AnhangA, "DIN V 18599-10 Anhang A" }
        };

        public enum MaterialCategory
        {
            NotDefined,
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
            Wohnen,
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
    }
}
