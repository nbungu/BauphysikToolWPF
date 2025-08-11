using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Services.Application;
using System;
using System.Linq;
using BauphysikToolWPF.Services.UI.OpenGL;
using static BauphysikToolWPF.Models.UI.Enums;
using BauphysikToolWPF.Services.UI;

namespace BauphysikToolWPF.Models.Domain.Helper
{
    public static class ProjectExtensions
    {
        public static void AddElement(this Project project, Element newElement)
        {
            newElement.ParentProject = project;
            newElement.InternalId = project.Elements.DefaultIfEmpty().Max(e => e?.InternalId ?? 0) + 1;
            project.Elements.Add(newElement);
        }
        public static void DuplicateElement(this Project project, Element element)
        {
            var copy = element.Copy();
            copy.ParentProject = project;
            project.AddElement(copy);
        }

        public static void RemoveElementById(this Project project, int elementId)
        {
            if (project.Elements.Count == 0) return;
            project.Elements.RemoveAll(e => e.InternalId == elementId);
        }

        private static readonly ElementComparer ElementComparer = new ElementComparer(ElementSortingType.DateDescending);

        public static void SortElements(this Project project, ElementSortingType sortingType)
        {
            if (project.Elements.Count == 0) return;
            ElementComparer.SortingType = sortingType;
            project.Elements.Sort(ElementComparer);
        }

        public static void AssignInternalIdsToElements(this Project project, bool forceOverwrite = false)
        {
            if (project.Elements.Count == 0) return;
            if (forceOverwrite || project.Elements.Any(e => e.InternalId == -1))
            {
                int index = 0;
                project.Elements.ForEach(e => e.InternalId = index++);
            }
        }
        public static void AssignInternalIdsToEnvelopeItems(this Project project, bool forceOverwrite = false)
        {
            if (project.EnvelopeItems.Count == 0) return;
            if (forceOverwrite || project.EnvelopeItems.Any(e => e.InternalId == -1))
            {
                int index = 0;
                project.EnvelopeItems.ForEach(e => e.InternalId = index++);
            }
        }

        public static void RemoveEnvelopeItemById(this Project project, int envelopeItemId)
        {
            if (project.EnvelopeItems.Count == 0) return;
            project.EnvelopeItems.RemoveAll(e => e.InternalId == envelopeItemId);
        }

        public static void AddEnvelopeItem(this Project project, EnvelopeItem newItem)
        {
            newItem.InternalId = project.EnvelopeItems.DefaultIfEmpty().Max(e => e?.InternalId ?? 0) + 1;
            project.EnvelopeItems.Add(newItem);
        }

        public static void DuplicateEnvelopeItemById(this Project project, int envelopeItemId)
        {
            if (project.EnvelopeItems.Count == 0) return;
            var copy = project.EnvelopeItems.First(l => l.InternalId == envelopeItemId).Copy();
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
            var elementScene = new ElementSceneBuilder();
            elementScene.CrossSectionBuilder.DrawingType = DrawingType.CrossSection;

            foreach (var element in elementsWithoutImage)
            {
                if (element.Layers.Count == 0)
                {
                    element.Image = Array.Empty<byte>();
                    continue;
                }

                elementScene.CrossSectionBuilder.Element = element;
                var bmp = OglOffscreenScene.CaptureSceneImage(
                    elementScene,
                    (int)elementScene.CrossSectionBuilder.CanvasSize.Width, // Width
                    (int)elementScene.CrossSectionBuilder.CanvasSize.Height, // Height
                    zoom: 1.0, // Zoom factor
                    dpi: 96 // DPI
                );
                element.Image = bmp.ToByteArray();
            }
        }
        public static void RenderAllElementImages(this Project project)
        {
            var elementScene = new ElementSceneBuilder();
            elementScene.CrossSectionBuilder.DrawingType = DrawingType.CrossSection;
            
            foreach (var element in project.Elements)
            {
                if (element.Layers.Count == 0)
                {
                    element.Image = Array.Empty<byte>();
                    continue;
                }
                    
                elementScene.CrossSectionBuilder.Element = element;
                var bmp = OglOffscreenScene.CaptureSceneImage(
                    elementScene,
                    (int)elementScene.CrossSectionBuilder.CanvasSize.Width, // Width
                    (int)elementScene.CrossSectionBuilder.CanvasSize.Height, // Height
                    zoom: 1.0, // Zoom factor
                    dpi: 96 // DPI
                );
                element.Image = bmp.ToByteArray();
            }
        }

        public static void AssignAsParentToElements(this Project project)
        {
            if (project.Elements.Count == 0) return;
            project.Elements.ForEach(e => e.ParentProject = project);
        }
    }
}
