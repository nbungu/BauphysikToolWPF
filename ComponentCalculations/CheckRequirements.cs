using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BauphysikToolWPF.ComponentCalculations.CheckRequirements;

namespace BauphysikToolWPF.ComponentCalculations
{
    /*
     * No static Class due to often changing 'Project' and 'Construction'.
     * Therefore the Calculations will always be up to date when calling Class with 'new'.
     * Con: Needs computation time on every call even if variables did not change.
     */

    public class CheckRequirements
    {
        // Always fetch current Project on calling this Class. No need for Notifier or Updater when Project changes
        private Project project = FO0_LandingPage.Project;

        // Always fetch current Construction on calling this Class. No need for Notifier or Updater when Construction changes
        private Construction currentConstruction = FO0_LandingPage.SelectedElement.Construction;

        public double U_max;

        public double R_min;

        public bool IsUValueOK = false;

        public bool IsRValueOK = false;

        public CheckRequirements(double u_value, double r_value)
        {
            this.U_max = GetUMax();
            this.R_min = GetRMin();
            this.IsUValueOK = u_value <= U_max;
            this.IsRValueOK = r_value >= R_min;
        }
        
        public enum BuildingUsage
        {
            NonResidential, // 0, Nichtwohngebäude
            Residential     // 1, Wohngebäude
        }
        public enum BuildingAge
        {
            Existing,       // 0, Bestandsgebäude
            New             // 1, Neubau
        }
        public enum EnvCondition
        {
            TiGreater19,    // 0, ValueA
            TiBetween12And19// 1, ValueB
        }
        public enum RequirementSource
        {
            GEG_Anlage1 = 1,
            GEG_Anlage2 = 2,
            GEG_Anlage7 = 3,
            DIN_4108_2_Tabelle3 = 4
        }
        private double GetUMax()
        {
            // default (irregular) values
            double u_max_requirement = -1;
            int requirementSourceId = -1; 

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = currentConstruction.Requirements;

            // catch constructions with no requirements
            if (allRequirements == null || allRequirements.Count == 0)
                return u_max_requirement;

            // b) Select relevant Source based off Building Age and Usage
            if (project.IsNewConstruction)
            {
                if (project.IsResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSource.GEG_Anlage1;
                }
                if (project.IsNonResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSource.GEG_Anlage2;
                }
            }
            if (project.IsExistingConstruction)
            {
                requirementSourceId = (int)RequirementSource.GEG_Anlage7;
            }

            // c) Get specific Requirement from selected RequirementSource
            Requirement specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);

            // Check if conditions have to be met
            if (UserSaved.Ti >= 19)
            {
                return specificRequirement.ValueA;
            }
            else if (UserSaved.Ti > 12 && UserSaved.Ti < 19)
            {
                return specificRequirement.ValueB ?? specificRequirement.ValueA;
            }
            else
            {
                //TODO
                // If Room Temperature (inside) is lower than 12 °C it does not specify as 'heated' room. No requirement has to be met!
                // To be safe: Use lowest requirement from specified source (means high U-Value)
                return 5.0;
            }
        }

        private double GetRMin()
        {
            // default (irregular) values
            double r_min_requirement = -1;

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = currentConstruction.Requirements;

            // catch constructions with no requirements
            if (allRequirements == null || allRequirements.Count == 0)
                return r_min_requirement;

            // b) Select relevant Source
            int requirementSourceId = (int)RequirementSource.DIN_4108_2_Tabelle3;

            // c) Get specific Requirement from selected RequirementSource
            Requirement specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
            //TODO: Can be null

            // Check if conditions have to be met
            // TODO m'
            return specificRequirement.ValueA;

        }
    }
}
