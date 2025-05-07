using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Calculation
{
    public class CheckRequirementsConfig
    {
        public BuildingAgeType BuildingAge { get; set; }
        public BuildingUsageType BuildingUsage { get; set; }
        public double Ti { get; set; }
        public double Te { get; set; }
    }
}
