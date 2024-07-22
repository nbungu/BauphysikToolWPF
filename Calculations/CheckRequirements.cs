using BauphysikToolWPF.Models;
using BauphysikToolWPF.SessionData;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.Calculations
{
    /*
     * No static Class due to often changing 'Project' and 'Construction'.
     * Therefore the Calculations will always be up to date when calling Class with 'new'.
     * Con: Needs computation time on every call even if variables did not change.
     */

    public class CheckRequirements
    {
        // Always fetch current Project on calling this Class. No need for Notifier or Updater when Project changes
        private readonly Project _currentProject = UserSaved.SelectedProject;

        public Element Element { get; }
        public double U_max { get; }
        public double R_min { get; }
        public double Q_max { get; }
        public bool IsUValueOk { get; } // GEG Requirements
        public bool IsRValueOk { get; } // DIN 4108-2 Requirements
        public bool IsQValueOk { get; } // Not mandatory as requirement

        public CheckRequirements(Element? element, double uValue, double qValue)
        {
            if (element == null)
            {
                Element = new Element();
                return;
            }

            Element = element;
            U_max = GetUMax();
            R_min = GetRMin();
            Q_max = GetQMax();
            IsUValueOk = (U_max == -1) || uValue <= U_max;
            IsRValueOk = (R_min == -1) || Element.RGesValue >= R_min;
            IsQValueOk = (Q_max == -1) || qValue <= Q_max;
        }

        private double GetUMax()
        {
            // default (irregular) values
            int requirementSourceId = -1;

            // a) Get all Requirements linked to current type of construction. Without any relation to a specific RequirementSource!
            // via m:n relation of Construction and Requirement.
            var constructionRequirements = Element.Construction.Requirements;

            // catch constructions with no requirements (e.g. Innenwand)
            if (constructionRequirements is null || constructionRequirements.Count == 0) return -1;

            // b) Select relevant Source based off Building Age and Usage
            if (_currentProject.IsNewConstruction)
            {
                if (_currentProject.IsResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSourceType.GEG_Anlage1;
                }
                else if (_currentProject.IsNonResidentialUsage)
                {
                    requirementSourceId = (int)RequirementSourceType.GEG_Anlage2;
                }
            }
            else if (_currentProject.IsExistingConstruction)
            {
                requirementSourceId = (int)RequirementSourceType.GEG_Anlage7;
            }

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = constructionRequirements.Find(r => r.DocumentSourceId == requirementSourceId);
            if (specificRequirement is null) return -1;

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
            if (allRequirements is null || allRequirements.Count == 0) return -1;

            // b) Select relevant Source
            int requirementSourceId = (int)RequirementSourceType.DIN_4108_2_Tabelle3;

            // c) Get specific Requirement from selected RequirementSource
            Requirement? specificRequirement = allRequirements.Find(r => r.DocumentSourceId == requirementSourceId);
            if (specificRequirement is null) return -1;

            // Check if conditions have to be met
            if (Element.AreaMassDens >= 100) return specificRequirement.ValueA;
            
            return specificRequirement.ValueB ?? specificRequirement.ValueA;
        }
        private double GetQMax()
        {
            if (U_max == -1) return -1;

            return Math.Round(U_max * (UserSaved.Ti - UserSaved.Te), 3);
        }
    }
}
