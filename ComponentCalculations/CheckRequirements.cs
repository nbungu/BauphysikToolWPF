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
    public class CheckRequirements
    {
        private Project project = FO0_LandingPage.Project;

        private Construction currentConstruction;

        private double U_calculated;

        //private double R_calculated;

        public double U_max;

        //public double R_min;

        public bool IsUValueOK = false;

        //public bool IsRValueOK = false;

        public CheckRequirements(double u_value, Construction construction)
        {
            this.currentConstruction = construction;
            this.U_calculated = u_value;
            //this.R_calculated = r_value;
            this.U_max = GetUMax();
            //this.R_min = GetRMin();
            this.IsUValueOK = U_calculated <= U_max;
            //this.IsRValueOK = R_calculated >= R_min;
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
            double u_max_requirement = 0.0;
            Requirement specificRequirement = new Requirement();

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = currentConstruction.Requirements; // TODO: is null

            // b) Select relevant Source based off Building Age and Usage
            if (project.IsNewConstruction)
            {
                if(project.IsResidentialUsage)
                {
                    int requirementSourceId = (int)RequirementSource.GEG_Anlage1;
                    // c) Get specific Requirement from selected RequirementSource
                    specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
                    // TODO break;
                }
                if (project.IsNonResidentialUsage)
                {
                    int requirementSourceId = (int)RequirementSource.GEG_Anlage2;
                    // c) Get specific Requirement from selected RequirementSource
                    specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
                }
            }
            if (project.IsExistingConstruction)
            {
                int requirementSourceId = (int)RequirementSource.GEG_Anlage7;
                // c) Get specific Requirement from selected RequirementSource
                specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
            }

            //TODO: Check for Conditions
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
            return 2.0;
        }
    }
}
