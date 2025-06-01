using System.Collections.Generic;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Calculation
{
    public class CheckRequirementsConfig
    {
        //public BuildingAgeType BuildingAge { get; set; }
        //public BuildingUsageType BuildingUsage { get; set; }
        public double Ti { get; set; }
        public double Te { get; set; }
        public List<DocumentSourceType> RelevantDocumentSources { get; set; } = new List<DocumentSourceType>(0);
    }
}
