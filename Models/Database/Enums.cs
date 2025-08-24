using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Repositories;

namespace BauphysikToolWPF.Models.Database
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
            Fenster,
            Tuer,
            InnenTuer,
            Glasvorbau,
            Tiefgaragendecke, 
            DeckeGegUnbeheiztenDachraum,
        }

        public static readonly Dictionary<ConstructionType, string> ConstructionTypeMapping =
            Enum.GetValues(typeof(ConstructionType))
                .Cast<ConstructionType>()
                .ToDictionary(
                    type => type,
                    DatabaseAccess.QueryConstructionNameByConstructionType
                );

        public static readonly Dictionary<ConstructionType, string> ConstructionTypeShortNameMapping = new()
        {
            { ConstructionType.NotDefined, "-" },
            { ConstructionType.Aussenwand, "AW" },
            { ConstructionType.AussenwandGegErdreich, "AWE" },
            { ConstructionType.Innenwand, "IW" },
            { ConstructionType.WhgTrennwand, "TW" },
            { ConstructionType.GeschossdeckeGegAussenluft, "DA" },
            { ConstructionType.DeckeGegUnbeheizt, "DU" },
            { ConstructionType.InnenwandGegUnbeheizt, "IWU" },
            { ConstructionType.Flachdach, "FD" },
            { ConstructionType.Schraegdach, "SD" },
            { ConstructionType.Umkehrdach, "FD" },
            { ConstructionType.Bodenplatte, "BP" },
            { ConstructionType.Kellerdecke, "DK" },
            { ConstructionType.Fenster, "FE" },
            { ConstructionType.Tuer, "AT" },
            { ConstructionType.InnenTuer, "IT" },
            { ConstructionType.Glasvorbau, "FE" },
            { ConstructionType.Tiefgaragendecke, "DTG" },
            { ConstructionType.DeckeGegUnbeheiztenDachraum, "DUD" },
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

        public enum ConstructionGroup
        {
            AussenliegendeWand = 1,
            InnenliegendeWand = 2,
            Decke = 3,
            Boden = 4,
            Dach = 5,
            Fenster = 6,
            NotDefined = 10
        }

        public static readonly Dictionary<ConstructionGroup, string> ConstructionGroupMapping = new()
        {
            { ConstructionGroup.AussenliegendeWand, "Außenliegende Wand" },
            { ConstructionGroup.InnenliegendeWand, "Innenliegende Wand" },
            { ConstructionGroup.Decke, "Decke" },
            { ConstructionGroup.Boden, "Boden" },
            { ConstructionGroup.Dach, "Dachaufbauten" },
            { ConstructionGroup.Fenster, "Fenster" },
            { ConstructionGroup.NotDefined, "Sonstige" },
        };

        public enum DocumentSourceType
        {
            NotDefined = 0,
            GEG_Anlage1,
            GEG_Anlage2_Spalte1,
            GEG_Anlage7_Spalte1,
            DIN_4108_2_Tabelle_3,
            DIN_V_18599_10_AnhangA,
            DIN_V_18599_10_Tabelle_E1,
            DIN_V_18599_10_Tabelle_4,
            DIN_V_18599_2_Tabelle_5,
            DIN_V_18599_10_Tabelle_5,
            DIN_4108_3_AnhangA,
            DIN_EN_ISO_6946_Tabelle_7,
            DIN_EN_ISO_6946_Tabelle_8,
            DIN_4108_2_5p1p2p2,
            DIN_4108_2_5p1p3,
            GEG_Anlage2_Spalte2,
            GEG_Anlage7_Spalte2,
        }

        // TODO: TEST
        // method calls are executed once per entry when the static class containing the dictionary
        // is first loaded into memory (i.e., on first access to any member of the class or when explicitly triggered)
        public static readonly Dictionary<DocumentSourceType, string> DocumentSourceTypeMapping =
            Enum.GetValues(typeof(DocumentSourceType))
                .Cast<DocumentSourceType>()
                .ToDictionary(
                    type => type,
                    DatabaseAccess.QueryDocumentSourceNameBySourceType
                );

        public enum MaterialCategory
        {
            NotDefined = 0,
            Insulation,
            Concrete,
            Wood,
            Masonry,
            Plasters,
            Sealant,
            Air,
            Glass
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
            { MaterialCategory.Glass, "Glas" },
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
            { MaterialCategory.Air, 4.0 },
            { MaterialCategory.Glass, 0.4 },
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

        public static readonly Dictionary<RoomUsageType, string> RoomUsageTypeMapping =
            Enum.GetValues(typeof(RoomUsageType))
                .Cast<RoomUsageType>()
                .ToDictionary(
                    type => type,
                    DatabaseAccess.QueryRoomUsageProfileNameByRoomUsageType
                );

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

        public enum RequirementComparison
        {
            None = 0,
            Equal,
            LessThan,
            GreaterThan,
            LessThanOrEqual,
            GreaterThanOrEqual,
        }
        public static readonly Dictionary<RequirementComparison, string> RequirementComparisonMapping = new()
        {
            { RequirementComparison.None, "kein Anforderungswert" },
            { RequirementComparison.Equal, "=" },
            { RequirementComparison.LessThan, "<" },
            { RequirementComparison.GreaterThan, ">" },
            { RequirementComparison.LessThanOrEqual, "<=" },
            { RequirementComparison.GreaterThanOrEqual, ">=" }
        };
        public static readonly Dictionary<RequirementComparison, string> RequirementComparisonDescriptionMapping = new()
        {
            { RequirementComparison.None, "kein Anforderungswert" },
            { RequirementComparison.Equal, "Vorgabe" },
            { RequirementComparison.LessThan, "Maximalwert" },
            { RequirementComparison.GreaterThan, "Minimalwert" },
            { RequirementComparison.LessThanOrEqual, "Maximalwert" },
            { RequirementComparison.GreaterThanOrEqual, "Minimalwert" }
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
