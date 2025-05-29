using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Repositories;

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
            Fenster,
            Tuer,
            InnenTuer,
            Glasvorbau,
            Tiefgaragendecke
        }

        public static readonly Dictionary<ConstructionType, string> ConstructionTypeMapping =
            Enum.GetValues(typeof(ConstructionType))
                .Cast<ConstructionType>()
                .ToDictionary(
                    type => type,
                    DatabaseAccess.QueryConstructionNameByConstructionType
                );

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
            DIN_4108_2_Tabelle_3,
            DIN_V_18599_10_AnhangA,
            DIN_V_18599_10_Tabelle_E1,
            DIN_V_18599_10_Tabelle_4,
            DIN_V_18599_2_Tabelle_5,
            DIN_V_18599_10_Tabelle_5,
            DIN_4108_3_AnhangA,
            DIN_EN_ISO_6946_Tabelle_7,
            DIN_EN_ISO_6946_Tabelle_8,
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
