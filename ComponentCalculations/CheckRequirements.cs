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
        private Project _currentProject = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId);

        public Element Element { get; private set; }
        public double U_max { get; private set; }
        public double R_min { get; private set; }
        public double Q_max { get; private set; }
        public bool IsUValueOK { get; private set; } = false; // GEG Requirements
        public bool IsRValueOK { get; private set; } = false; // DIN 4108-2 Requirements
        public bool IsQValueOK { get; private set; } = false; // Not mandatory as requirement

        public CheckRequirements(Element element, double uValue, double qValue)
        {
            if (element is null)
                return;

            Element = element;            
            U_max = GetUMax();
            R_min = GetRMin();
            Q_max = GetQMax();
            IsUValueOK = (U_max == -1) ? true : uValue <= U_max;
            IsRValueOK = (R_min == -1) ? true : Element.RValue >= R_min;
            IsQValueOK = (Q_max == -1) ? true : qValue <= Q_max;
        }
       
        private double GetUMax()
        {
            // default (irregular) values
            int requirementSourceId = -1; 

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            List<Requirement> allRequirements = Element.Construction.Requirements;

            // catch constructions with no requirements (e.g. Innenwand)
            if (allRequirements is null || allRequirements.Count == 0)
                return -1;

            // b) Select relevant Source based off Building Age and Usage
            if (_currentProject.IsNewConstruction)
            {
                if (_currentProject.IsResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSourceType.GEG_Anlage1;
                }
                if (_currentProject.IsNonResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSourceType.GEG_Anlage2;
                }
            }
            if (_currentProject.IsExistingConstruction)
            {
                requirementSourceId = (int)RequirementSourceType.GEG_Anlage7;
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
            List<Requirement> allRequirements = Element.Construction.Requirements;

            // catch constructions with no requirements
            if (allRequirements is null || allRequirements.Count == 0)
                return -1;

            // b) Select relevant Source
            int requirementSourceId = (int)RequirementSourceType.DIN_4108_2_Tabelle3;

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = allRequirements.Find(r => r.RequirementSourceId == requirementSourceId);
            if (specificRequirement is null)
                return -1;

            // Check if conditions have to be met
            if (Element.AreaMassDens >= 100)
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
