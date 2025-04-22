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

        public enum RoomUsageType
        {
            NotDefined,
            Einzelbuero,
            GrpBueroBis6Ap,
            GrpBueroAb7Ap,
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
            SanitaerWCInNWG,
            SonstigeAufenthalt,
            NebenflaechenOhneAufenthalt,
            Verkehrsflaechen,
            Lager,
            Rechenzentrum,
            GewerbeUndIndustrieHalleSA,
            GewerbeUndIndustrieHalleMSA,
            GewerbeUndIndustrieHalleLA,
            Zuschauerbereich,
            Theater,
            Buehne,
            Messe,
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
            { RoomUsageType.NotDefined, "Nicht definiert" },
            { RoomUsageType.Einzelbuero, "Einzelbüro" },
            { RoomUsageType.GrpBueroBis6Ap, "Gruppenbüro bis 6 AP" },
            { RoomUsageType.GrpBueroAb7Ap, "Gruppenbüro ab 7 AP" },
            { RoomUsageType.Besprechung, "Besprechungsraum" },
            { RoomUsageType.Schalterhalle, "Schalterhalle" },
            { RoomUsageType.Einzelhandel, "Einzelhandel" },
            { RoomUsageType.EinzelhandelMitKühlprodukten, "Einzelhandel mit Kühlprodukten" },
            { RoomUsageType.Klassenzimmer, "Klassenzimmer" },
            { RoomUsageType.Hoersaal, "Hörsaal" },
            { RoomUsageType.Bettenzimmer, "Bettenzimmer" },
            { RoomUsageType.Hotelzimmer, "Hotelzimmer" },
            { RoomUsageType.Kantine, "Kantine" },
            { RoomUsageType.Restaurant, "Restaurant" },
            { RoomUsageType.KuecheInNWG, "Küche in NWG" },
            { RoomUsageType.KuecheLager, "Küche Lager" },
            { RoomUsageType.SanitaerWCInNWG, "Sanitär/WC in NWG" },
            { RoomUsageType.SonstigeAufenthalt, "Sonstige Aufenthaltsräume" },
            { RoomUsageType.NebenflaechenOhneAufenthalt, "Nebenflächen ohne Aufenthalt" },
            { RoomUsageType.Verkehrsflaechen, "Verkehrsflächen" },
            { RoomUsageType.Lager, "Lagerflächen" },
            { RoomUsageType.Rechenzentrum, "Rechenzentrum" },
            { RoomUsageType.GewerbeUndIndustrieHalleSA, "Gewerbe- und Industriehalle SA" },
            { RoomUsageType.GewerbeUndIndustrieHalleMSA, "Gewerbe- und Industriehalle MSA" },
            { RoomUsageType.GewerbeUndIndustrieHalleLA, "Gewerbe- und Industriehalle LA" },
        };
    }
}
