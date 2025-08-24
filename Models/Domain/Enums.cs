using System.Collections.Generic;

namespace BauphysikToolWPF.Models.Domain
{
    public class Enums
    {
        public enum OrientationType
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        }
        public static readonly Dictionary<OrientationType, string> OrientationTypeMapping = new()
        {
            { OrientationType.North, "Norden" },
            { OrientationType.NorthEast, "Nordosten" },
            { OrientationType.East, "Osten" },
            { OrientationType.SouthEast, "Südosten" },
            { OrientationType.South, "Süden" },
            { OrientationType.SouthWest, "Südwesten" },
            { OrientationType.West, "Westen" },
            { OrientationType.NorthWest, "Nordwesten" }
        };
        
        public enum BuildingUsageType
        {
            NonResidential, // 0, Nichtwohngebäude
            Residential     // 1, Wohngebäude
        }
        public static readonly Dictionary<BuildingUsageType, string> BuildingUsageTypeMapping = new()
        {
            { BuildingUsageType.NonResidential, "Nichtwohngebäude" },
            { BuildingUsageType.Residential, "Wohngebäude" }
        };

        public enum BuildingAgeType
        {
            Existing,       // 0, Bestandsgebäude
            New             // 1, Neubau
        }
        public static readonly Dictionary<BuildingAgeType, string> BuildingAgeTypeMapping = new()
        {
            { BuildingAgeType.Existing, "Bestandsgebäude" },
            { BuildingAgeType.New, "Neubau" }
        };

        public enum BuildingTypeResidatial
        {
            EFH, // Einfamilienhaus
            MFH, // Mehrfamilienhaus
            ZFH, // Zweifamilienhaus
            RMH  // Reihenmittelhaus
        }
        public static readonly Dictionary<BuildingTypeResidatial, string> BuildingTypeResidatialMapping = new()
        {
            { BuildingTypeResidatial.EFH, "Einfamilienhaus" },
            { BuildingTypeResidatial.MFH, "Mehrfamilienhaus" },
            { BuildingTypeResidatial.ZFH, "Zweifamilienhaus" },
            { BuildingTypeResidatial.RMH, "Reihenmittelhaus" }
        };

        /// <summary>
        ///
        /// </summary>
        public enum VentilationType
        {
            None,
            Mechanical,
        }
        public static readonly Dictionary<VentilationType, string> VentilationTypeMapping = new()
        {
            { VentilationType.None, "keine Lüftungsanlage" },
            { VentilationType.Mechanical, "mechanische Lüftungsanlage" },
        };
    }
}
