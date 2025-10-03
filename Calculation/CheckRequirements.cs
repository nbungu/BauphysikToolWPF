using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BT.Logging;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.Domain.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{

    public class CheckRequirements
    {
        private readonly Element _element = new Element();

        public List<DocumentParameter> RelevantRequirements { get; private set; } = new List<DocumentParameter>();
        public List<DocumentSourceType> RelevantDocumentSources { get; set; } = new List<DocumentSourceType>(0);

        public double? UMax { get; private set; }
        public string UMaxRequirementSourceName => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null)?.DocumentSource.SourceName ?? "";
        public RequirementComparison UMaxComparisonRequirement => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null)?.RequirementComparison ?? RequirementComparison.None;
        public string UMaxComparisonDescription => RequirementComparisonDescriptionMapping[UMaxComparisonRequirement];
        public string UMaxCaption => UMaxComparisonDescription != "" && UMaxRequirementSourceName != "" ? $"{UMaxComparisonDescription} nach {UMaxRequirementSourceName}" : "";
        public bool IsUValueOk => _element.ThermalResults.UValue <= UMax;



        public double? RMin { get; private set; }
        public string? RMinRequirementSourceName => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null)?.DocumentSource.SourceName;
        public RequirementComparison RMinComparisonRequirement => RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null)?.RequirementComparison ?? RequirementComparison.None;
        public string RMinComparisonDescription => RequirementComparisonDescriptionMapping[RMinComparisonRequirement];
        public string RMinCaption => RMinComparisonDescription != "" && RMinRequirementSourceName != null ? $"{RMinComparisonDescription} nach {RMinRequirementSourceName}" : "";
        public bool IsRValueOk => _element.RGesValue >= RMin;


        public double? QMax { get; private set; }
        public bool IsQValueOk => _element.ThermalResults.QValue <= QMax;
        public bool HasRequirements => RelevantRequirements.Count > 0;

        public CheckRequirements() {}
        public CheckRequirements(Element element)
        {
            _element = element;

            Update();
        }

        public void Update()
        {
            _element.RefreshResults();

            // Update sources: _element specific sources can vary
            RelevantDocumentSources = GetRelatedDocumentSources(_element);

            RelevantRequirements = _element.Construction.Requirements.Where(r => RelevantDocumentSources.Contains(r.DocumentSource.DocumentSourceType)).ToList();

            UMax = GetUMax();
            RMin = GetRMin();
            QMax = GetQMax();

            Logger.LogInfo($"Updated requirements check for element: {_element}.");
        }

        private double? GetUMax()
        {
            DocumentParameter? specificRequirement = RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.UValue, null);
            if (specificRequirement is null) return null;

            // TODO: Distinct between Ti >= 19 and Ti > 12 && Ti < 19
            return specificRequirement.Value;
        }

        private double? GetRMin()
        {
            DocumentParameter? specificRequirement = RelevantRequirements.FirstOrDefault(r => r?.Symbol == Symbol.RValueElement, null);
            if (specificRequirement is null) return null;

            //TODO: Distinct between _element.AreaMassDens >= 100 and _element.AreaMassDens < 100
            return specificRequirement.Value;
        }

        private double? GetQMax()
        {
            if (UMax != null && UMax >= 0) return UMax * (_element.ThermalCalcConfig.Ti - _element.ThermalCalcConfig.Te);
            return null;
        }

        /// <summary>
        /// Determines a list of relevant document sources (standards, tables, etc.) 
        /// that apply based on the properties of the given element and its associated project. 
        /// This includes general sources as well as those specific to the element and the project context.
        /// </summary>
        /// <param name="element">The element for which the related document sources should be determined. 
        /// It is expected that the <c>ParentProject</c> property is correctly assigned.</param>
        /// <returns>A list of <see cref="DocumentSourceType"/> values applicable to the given element.</returns>
        public static List<DocumentSourceType> GetRelatedDocumentSources(Element element)
        {
            var project = element.ParentProject;
            if (project is null) return new List<DocumentSourceType>(0);

            #region General
            
            // Add document sources that are always available
            var documentSourceTypes = new List<DocumentSourceType>()
            {
                DocumentSourceType.DIN_V_18599_10_Tabelle_E1,
                DocumentSourceType.DIN_V_18599_2_Tabelle_5,
                DocumentSourceType.DIN_4108_3_AnhangA,
                DocumentSourceType.DIN_EN_ISO_6946_Tabelle_7,
                DocumentSourceType.DIN_EN_ISO_6946_Tabelle_8,
            };
            #endregion

            #region _element Related

            // Add document sources based on element properties
            if (element.IsInhomogeneous) documentSourceTypes.Add(DocumentSourceType.DIN_4108_2_5p1p3); // Mindestwerte für Wärmedurchlasswiderstände inhomogener, opaker Bauteile
            else
            {
                if (element.AreaMassDens >= 100) documentSourceTypes.Add(DocumentSourceType.DIN_4108_2_Tabelle_3); // Mindestwerte für Wärmedurchlasswiderstände homogener Bauteile mit m' ≥ 100 kg/m²
                else documentSourceTypes.Add(DocumentSourceType.DIN_4108_2_5p1p2p2); // Mindestwerte für Wärmedurchlasswiderstände homogener Bauteile mit m' < 100 kg/m²
            }

            #endregion
            
            #region Project Related

            // Add document sources based on project properties
            if (project.BuildingAge == BuildingAgeType.New && project.BuildingUsage == BuildingUsageType.Residential)
            {
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage1);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_4);
            }
            else if (project.BuildingAge == BuildingAgeType.New && project.BuildingUsage == BuildingUsageType.NonResidential)
            {
                if (element.ThermalCalcConfig.Ti >= 19) documentSourceTypes.Add(DocumentSourceType.GEG_Anlage2_Spalte1);
                else if (element.ThermalCalcConfig.Ti >= 12 && element.ThermalCalcConfig.Ti < 19) documentSourceTypes.Add(DocumentSourceType.GEG_Anlage2_Spalte2);

                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_AnhangA);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_5);
            }
            else if (project.BuildingAge == BuildingAgeType.Existing && project.BuildingUsage == BuildingUsageType.Residential)
            {
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage7_Spalte1);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_4);
            }
            else if (project.BuildingAge == BuildingAgeType.Existing && project.BuildingUsage == BuildingUsageType.NonResidential)
            {
                if (element.ThermalCalcConfig.Ti >= 19) documentSourceTypes.Add(DocumentSourceType.GEG_Anlage7_Spalte1);
                else if (element.ThermalCalcConfig.Ti >= 12 && element.ThermalCalcConfig.Ti < 19) documentSourceTypes.Add(DocumentSourceType.GEG_Anlage7_Spalte2);
                
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_AnhangA);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_5);
            }

            #endregion
            
            return documentSourceTypes;
        }
    }
}
