using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{
    
    public class CheckRequirements
    {
        public CheckRequirementsConfig Config { get; } = new CheckRequirementsConfig();
        public Element? Element { get; }
        public List<DocumentParameter> RelevantRequirements { get; private set; } = new List<DocumentParameter>();

        public double? UMax { get; private set; }
        public string UMaxRequirementSourceName => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null)?.DocumentSource.SourceName ?? "";
        public RequirementComparison UMaxComparisonRequirement => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null)?.RequirementComparison ?? RequirementComparison.None;
        public string UMaxComparisonDescription => RequirementComparisonDescriptionMapping[UMaxComparisonRequirement];
        public string UMaxCaption => UMaxComparisonDescription != "" && UMaxRequirementSourceName != "" ? $"{UMaxComparisonDescription} nach {UMaxRequirementSourceName}" : "";
        public bool IsUValueOk => Element?.ThermalResults.UValue <= UMax;



        public double? RMin { get; private set; }
        public string? RMinRequirementSourceName => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null)?.DocumentSource.SourceName;
        public RequirementComparison RMinComparisonRequirement => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null)?.RequirementComparison ?? RequirementComparison.None;
        public string RMinComparisonDescription => RequirementComparisonDescriptionMapping[RMinComparisonRequirement];
        public string RMinCaption => RMinComparisonDescription != "" && RMinRequirementSourceName != null ? $"{RMinComparisonDescription} nach {RMinRequirementSourceName}" : "";
        public bool IsRValueOk => Element?.RGesValue >= RMin;



        public double? QMax { get; private set; }
        public bool IsQValueOk => Element?.ThermalResults.QValue <= QMax;

        public CheckRequirements() {}
        public CheckRequirements(Element? element, CheckRequirementsConfig config)
        {
            Element = element;
            Config = config;

            Update();
        }

        public void Update()
        {
            if (Element is null) return;

            if (Element.Recalculate == false) Element.RefreshResults();

            RelevantRequirements = Element.Construction.Requirements.Where(r => Config.RelevantDocumentSources.Contains(r.DocumentSource.DocumentSourceType)).ToList();

            UMax = GetUMax();
            RMin = GetRMin();
            QMax = GetQMax();
        }

        private double? GetUMax()
        {
            if (Element is null) return null;

            DocumentParameter? specificRequirement = RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null);
            if (specificRequirement is null) return null;

            // TODO: Distinct between Ti >= 19 and Ti > 12 && Ti < 19
            return specificRequirement.Value;
        }

        private double? GetRMin()
        {
            if (Element is null) return null;

            DocumentParameter? specificRequirement = RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null);
            if (specificRequirement is null) return null;

            //TODO: Distinct between Element.AreaMassDens >= 100 and Element.AreaMassDens < 100
            return specificRequirement.Value;
        }

        private double? GetQMax()
        {
            if (UMax != null && UMax >= 0) return UMax * (Config.Ti - Config.Te);
            return null;
        }
    }
}
