using System.Collections.Generic;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeCalculationConfig
    {
        public bool IsEFH => IsResidential && BuildingTypeResidatial == BuildingTypeResidatial.EFH;
        public bool IsResidential => BuildingUsageType == BuildingUsageType.Residential;
        public BuildingUsageType BuildingUsageType { get; set; }
        public BuildingTypeResidatial BuildingTypeResidatial { get; set; }
        public bool WithAirLeakTest { get; set; }
    }
}
