using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using BT.Logging;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{

    public class CheckRequirements
    {
        public CheckRequirementsConfig Config { get; } = new CheckRequirementsConfig();
        public Element? Element { get; }
        public List<DocumentParameter> RelevantRequirements { get; private set; } = new List<DocumentParameter>();

        public List<DocumentSourceType> RelevantDocumentSources { get; set; } = new List<DocumentSourceType>(0);

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

            // Update sources: Element specific sources can vary
            RelevantDocumentSources = GetRelatedDocumentSources(Element);

            RelevantRequirements = Element.Construction.Requirements.Where(r => RelevantDocumentSources.Contains(r.DocumentSource.DocumentSourceType)).ToList();

            UMax = GetUMax();
            RMin = GetRMin();
            QMax = GetQMax();

            Logger.LogInfo($"Updated requirements check for element: {Element}.");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public List<DocumentSourceType> GetRelatedDocumentSources(Element element)
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

            #region Element Related

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
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage2_Spalte1);
                // TODO: auf element ebene -> beide GetSourcesMethoden zusammenlegen
                //if (Ti > 19) documentSourceTypes.Add(DocumentSourceType.GEG_Anlage2);
                // else documentSourceTypes.Add(DocumentSourceType.GEG_Anlage2_Spalte2);

                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_AnhangA);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_5);
            }
            else if (project.BuildingAge == BuildingAgeType.Existing && project.BuildingUsage == BuildingUsageType.Residential)
            {
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage7);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_4);
            }
            else if (project.BuildingAge == BuildingAgeType.Existing && project.BuildingUsage == BuildingUsageType.NonResidential)
            {
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage7);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_AnhangA);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_5);
            }

            #endregion
            
            return documentSourceTypes;
        }
    }
}
