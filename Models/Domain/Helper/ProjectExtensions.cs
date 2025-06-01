using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Services.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Repositories;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain.Helper
{
    public static class ProjectExtensions
    {
        public static void AssignInternalIdsToElements(this Project project)
        {
            if (project.Elements.Count == 0) return;
            int index = 0; // Start at 0
            project.Elements.ForEach(e => e.InternalId = index++);
        }
        public static void AssignInternalIdsToEnvelopeItems(this Project project)
        {
            if (project.EnvelopeItems.Count == 0) return;
            int index = 0; // Start at 0
            project.EnvelopeItems.ForEach(e => e.InternalId = index++);
        }

        public static void RemoveEnvelopeItem(this Project project, int targetItemId)
        {
            if (project.EnvelopeItems.Count == 0) return;
            var targetItem = project.EnvelopeItems.First(l => l.InternalId == targetItemId);
            project.EnvelopeItems.Remove(targetItem);
            project.AssignInternalIdsToEnvelopeItems();
        }

        public static void AddEnvelopeItem(this Project project, EnvelopeItem newItem)
        {
            project.EnvelopeItems.Add(newItem);
            project.AssignInternalIdsToEnvelopeItems();
        }
        public static void DuplicateEnvelopeItem(this Project project, int targetItemId)
        {
            if (project.EnvelopeItems.Count == 0) return;
            var copy = project.EnvelopeItems.First(l => l.InternalId == targetItemId).Copy();
            copy.InternalId = project.EnvelopeItems.Count;
            project.AddEnvelopeItem(copy);
        }
        public static bool IsMaterialInUse(this Project project, Material? material)
        {
            if (project.Elements.Count == 0 || material is null) return false;
            return project.Elements.Any(e =>
                e.Layers.Any(l =>
                    Equals(l.Material, material) || Equals(l.SubConstruction?.Material, material)
                )
            );
        }

        public static void RenderMissingElementImages(this Project project)
        {
            var elementsWithoutImage = project.Elements.Where(e => e.Image == Array.Empty<byte>()).ToList();
            elementsWithoutImage.ForEach(ImageCreator.RenderElementPreviewImage);
        }
        public static void RenderAllElementImages(this Project project)
        {
            project.Elements.ForEach(ImageCreator.RenderElementPreviewImage);
        }

        /// <summary>
        /// For filtering Element construction requirements
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static List<DocumentSourceType> GetProjectRelatedDocumentSources(this Project project)
        {
            // Add all document sources that are always available
            var documentSourceTypes = new List<DocumentSourceType>()
            {
                DocumentSourceType.DIN_4108_2_Tabelle_3,
                DocumentSourceType.DIN_V_18599_10_Tabelle_E1,
                DocumentSourceType.DIN_V_18599_2_Tabelle_5,
                DocumentSourceType.DIN_4108_3_AnhangA,
                DocumentSourceType.DIN_EN_ISO_6946_Tabelle_7,
                DocumentSourceType.DIN_EN_ISO_6946_Tabelle_8,
            };
            // Add document sources based on project properties
            if (project.BuildingAge == BuildingAgeType.New && project.BuildingUsage == BuildingUsageType.Residential)
            {
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage1);
                documentSourceTypes.Add(DocumentSourceType.DIN_V_18599_10_Tabelle_4);
            }
            else if (project.BuildingAge == BuildingAgeType.New && project.BuildingUsage == BuildingUsageType.NonResidential)
            {
                documentSourceTypes.Add(DocumentSourceType.GEG_Anlage2);
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
            return documentSourceTypes;
        }

        public static List<int> GetProjectRelatedDocumentSourceIds(this Project project)
        {
            return GetProjectRelatedDocumentSources(project)
                .Select(d => DatabaseAccess.QueryDocumentSourceBySourceType(d).Id)
                .ToList();
        }
    }
}
