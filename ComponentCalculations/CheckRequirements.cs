using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class CheckRequirements
    {
        private Project project;

        private Element currentElement;

        public double U_calculated;

        public double R_calculated;

        public double U_max;

        public double R_min;

        public bool IsUValueOK = false;

        public bool IsRValueOK = false;

        public CheckRequirements(double u_value, double r_value)
        {
            this.project = FO0_LandingPage.Project;
            this.currentElement = FO0_LandingPage.SelectedElement;
            this.U_calculated = u_value;
            this.R_calculated = r_value;
            this.U_max = GetUMax();
            this.R_min = GetRMin();
            this.IsUValueOK = U_calculated <= U_max;
            this.IsRValueOK = R_calculated >= R_min;
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
            if (project.IsNewConstruction)
            {
                if(project.IsResidentialUsage)
                {
                    int sourceId = (int)RequirementSource.GEG_Anlage1;
                    int constructionId = currentElement.ConstructionId;

                    //Query ConstructionRequirements: Relevant Requirements only for current constructionId
                    

                    //Only use RequirementId which is related to GEG Anlage 1 (Id = 1)
                    

                }
                if (project.IsNonResidentialUsage)
                {
                    int sourceId = (int)RequirementSource.GEG_Anlage2;
                    List<Requirement> requirements = DatabaseAccess.QueryRequirementsBySourceId(sourceId);
                }
            }
            if (project.IsExistingConstruction)
            {
                int sourceId = (int)RequirementSource.GEG_Anlage7;
                List<Requirement> requirements = DatabaseAccess.QueryRequirementsBySourceId(sourceId);
            }
        }
        private double GetRMin()
        {
            return 1.0;
        }
    }
}
