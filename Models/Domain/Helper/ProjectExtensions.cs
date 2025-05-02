using BauphysikToolWPF.Services.Application;
using System;
using System.Linq;
using BauphysikToolWPF.Models.Database;

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
    }
}
