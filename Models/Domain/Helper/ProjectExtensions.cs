using BauphysikToolWPF.Services.Application;
using System;
using System.Linq;

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
