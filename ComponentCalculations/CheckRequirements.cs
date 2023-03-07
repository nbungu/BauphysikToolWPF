using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using System;
using System.Collections.Generic;

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
        private Project currentProject = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId);

        // Always fetch current Construction on calling this Class. No need for Notifier or Updater when Construction changes
        private Element currentElement = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId);

        public double U_max;

        public double R_min;

        public double Q_max;

        public bool IsUValueOK = false; // GEG Requirements

        public bool IsRValueOK = false; // DIN 4108-2 Requirements

        public bool IsQValueOK = false; // Not mandatory as requirement

        public CheckRequirements(double u_value, double r_value)
        {
            this.U_max = GetUMax();
            this.R_min = GetRMin();
            this.Q_max = GetQMax();
            this.IsUValueOK = u_value <= U_max;
            this.IsRValueOK = r_value >= R_min;
            this.IsQValueOK = Math.Round(u_value * (UserSaved.Ti - UserSaved.Te), 2) <= Q_max;
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
            int requirementSourceId = -1; 

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = currentElement.Construction.Requirements;

            // catch constructions with no requirements
            if (allRequirements is null || allRequirements.Count == 0)
                return -1;

            // b) Select relevant Source based off Building Age and Usage
            if (currentProject.IsNewConstruction)
            {
                if (currentProject.IsResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSource.GEG_Anlage1;
                }
                if (currentProject.IsNonResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSource.GEG_Anlage2;
                }
            }
            if (currentProject.IsExistingConstruction)
            {
                requirementSourceId = (int)RequirementSource.GEG_Anlage7;
            }

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
            if (specificRequirement is null)
                return -1;

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
                return -1;
            }
        }

        private double GetRMin()
        {
            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = currentElement.Construction.Requirements;

            // catch constructions with no requirements
            if (allRequirements is null || allRequirements.Count == 0)
                return -1;

            // b) Select relevant Source
            int requirementSourceId = (int)RequirementSource.DIN_4108_2_Tabelle3;

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
            if (specificRequirement is null)
                return -1;

            // Check if conditions have to be met
            if (currentElement.AreaMassDens >= 100)
            {
                return specificRequirement.ValueA;
            }
            else
            {
                return specificRequirement.ValueB ?? specificRequirement.ValueA;
            }
        }
        private double GetQMax()
        {
            if (U_max == -1)
                return -1;

            return Math.Round(U_max * (UserSaved.Ti - UserSaved.Te), 3);
        }       
    }
}
