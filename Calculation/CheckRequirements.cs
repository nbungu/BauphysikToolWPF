using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{
    /*
     * No static Class due to often changing 'Project' and 'Construction'.
     * Therefore the Calculations will always be up to date when calling Class with 'new'.
     * Con: Needs computation time on every call even if variables did not change.
     */

    public class CheckRequirements
    {
        public Element? Element { get; }
        //public BuildingAgeType BuildingAge { get; }
        //public BuildingUsageType BuildingUsage { get; }
        public double Ti { get; }
        public double Te { get; }
        public List<DocumentParameter> RelevantRequirements { get; set; } = new List<DocumentParameter>();
        public double? UMax { get; private set; }
        public string UMaxRequirementSourceName => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null)?.DocumentSource.SourceName ?? "";
        public string UMaxComparisonDescription => RequirementComparisonDescriptionMapping[RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null)?.RequirementComparison ?? RequirementComparison.None];
        public string UMaxCaption => UMaxComparisonDescription != "" && UMaxRequirementSourceName != "" ? $"{UMaxComparisonDescription} nach {UMaxRequirementSourceName}" : "";

        public double? RMin { get; private set; }
        public string? RMinRequirementSourceName => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null)?.DocumentSource.SourceName;
        public double? QMax { get; private set; }

        public bool IsUValueOk => Element?.ThermalResults.UValue <= UMax; // GEG Requirements
        public bool IsRValueOk => Element?.RGesValue >= RMin; // DIN 4108-2 Requirements
        public bool IsQValueOk => Element?.ThermalResults.QValue <= QMax; // Not mandatory as requirement

        public CheckRequirements() { }
        public CheckRequirements(Element? element, CheckRequirementsConfig config)
        {
            Element = element;
            if (Element is null) return;

            //BuildingAge = config.BuildingAge;
            //BuildingUsage = config.BuildingUsage;
            Ti = config.Ti;
            Te = config.Te;

            // Force recalculation to work with latest values
            Element.UpdateResults();

            RelevantRequirements = Element.Construction.Requirements.Where(r => config.RelevantDocumentSources.Contains(r.DocumentSource.DocumentSourceType)).ToList();

            SetUMax();
            SetRMin();
            SetQMax();
        }

        private void SetUMax()
        {
            if (Element is null) return;

            DocumentParameter? specificRequirement = RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null);
            if (specificRequirement is null) return;

            // TODO: Distinct between Ti >= 19 and Ti > 12 && Ti < 19
            UMax = specificRequirement.Value;
        }

        private void SetRMin()
        {
            if (Element is null) return;

            DocumentParameter? specificRequirement = RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null);
            if (specificRequirement is null) return;

            //TODO: Distinct between Element.AreaMassDens >= 100 and Element.AreaMassDens < 100
            RMin = specificRequirement.Value;
        }

        private void SetQMax()
        {
            if (UMax != null && UMax >= 0) QMax = UMax * (Ti - Te);
        }
    }
}
