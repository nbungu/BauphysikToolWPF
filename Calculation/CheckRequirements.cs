using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using System;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using Enums = BauphysikToolWPF.Models.Database.Helper.Enums;

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
        public BuildingAgeType BuildingAge { get; }
        public BuildingUsageType BuildingUsage { get; }
        public double Ti { get; }
        public double Te { get; }
        public double UMax { get; private set; } = -1;
        public double RMin { get; private set; } = -1;
        public double QMax { get; private set; } = -1;
        public bool IsUValueOk { get; } // GEG Requirements
        public bool IsRValueOk { get; } // DIN 4108-2 Requirements
        public bool IsQValueOk { get; } // Not mandatory as requirement

        public CheckRequirements() { }
        public CheckRequirements(Element? element, CheckRequirementsConfig config)
        {
            Element = element;
            if (Element is null) return;

            BuildingAge = config.BuildingAge;
            BuildingUsage = config.BuildingUsage;
            Ti = config.Ti;
            Te = config.Te;

            Element.UpdateResults();
            SetUMax();
            SetRMin();
            SetQMax();
            IsUValueOk = UMax == -1 || Element.ThermalResults.UValue <= UMax;
            IsRValueOk = RMin == -1 || Element.RGesValue >= RMin;
            IsQValueOk = QMax == -1 || Element.ThermalResults.QValue <= QMax;
        }

        private void SetUMax()
        {
            if (Element is null) return;

            // default (irregular) values
            int requirementSourceId = -1;

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            var constructionRequirements = Element.Construction.Requirements;

            // catch constructions with no requirements (e.g. Innenwand)
            if (constructionRequirements.Count == 0) return;

            // b) Select relevant Source based off Building Age and Usage
            if (BuildingAge == BuildingAgeType.New)
            {
                if (BuildingUsage == BuildingUsageType.Residential)
                {
                    requirementSourceId = (int)Enums.DocumentSourceType.GEG_Anlage1;
                }
                else if (BuildingUsage == BuildingUsageType.NonResidential)
                {
                    requirementSourceId = (int)Enums.DocumentSourceType.GEG_Anlage2;
                }
            }
            else if (BuildingAge == BuildingAgeType.Existing)
            {
                requirementSourceId = (int)Enums.DocumentSourceType.GEG_Anlage7;
            }

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = constructionRequirements.Find(r => r.DocumentSourceId == requirementSourceId);
            if (specificRequirement is null) return;

            // Check if conditions have to be met
            if (Ti >= 19)
            {
                UMax = specificRequirement.ValueA;
            }
            else if (Ti > 12 && Ti < 19)
            {
                UMax = specificRequirement.ValueB ?? specificRequirement.ValueA;
            }
            else
            {
                //TODO
                // If Room Temperature (inside) is lower than 12 °C it does not specify as 'heated' room. No requirement has to be met!
            }
        }

        private void SetRMin()
        {
            if (Element is null) return;
            
            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = Element.Construction.Requirements;

            // catch constructions with no requirements
            if (allRequirements.Count == 0) return;

            // b) Select relevant Source
            int requirementSourceId = (int)Enums.DocumentSourceType.DIN_4108_2_Tabelle3;

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = allRequirements.Find(r => r.DocumentSourceId == requirementSourceId);
            if (specificRequirement is null) return;

            // Check if conditions have to be met
            if (Element.AreaMassDens >= 100) RMin = specificRequirement.ValueA;
            
            RMin = specificRequirement.ValueB ?? specificRequirement.ValueA;
        }
        private void SetQMax()
        {
            if (UMax >= 0) QMax = Math.Round(UMax * (Ti - Te), 4);
        }
    }
}
