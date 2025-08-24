using static BauphysikToolWPF.Models.Domain.Enums;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeCalculationConfig
    {
        public bool IsEFH => IsResidential && BuildingTypeResidatial == BuildingTypeResidatial.EFH;
        public bool IsResidential => BuildingUsageType == BuildingUsageType.Residential;
        public BuildingUsageType BuildingUsageType { get; set; }
        public BuildingTypeResidatial BuildingTypeResidatial { get; set; }
        public bool WithAirLeakTest { get; set; }

        public EnvelopeCalculationConfig Copy()
        {
            return new EnvelopeCalculationConfig()
            {
                // TODO:
            };
        }
    }
}
