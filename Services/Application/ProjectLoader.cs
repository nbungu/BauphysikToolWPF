using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BT.Logging;
using System;
using System.IO;

namespace BauphysikToolWPF.Services.Application
{
    internal static class ProjectLoader
    {
        public static void TryOpenProject(string rawFilePath, bool isModifiedState = false)
        {
            try
            {
                if (PathService.IsUncPath(rawFilePath))
                {
                    Logger.LogWarning("Rejected UNC/network path provided.");
                    return;
                }

                string fullPath = Path.GetFullPath(rawFilePath);

                if (!PathService.IsAllowedExtension(fullPath, new[] { ".btk", ".btkproj" }))
                {
                    Logger.LogWarning($"Rejected disallowed extension: {PathService.MaskPath(fullPath)}");
                    return;
                }

                if (!File.Exists(fullPath))
                {
                    Logger.LogWarning($"File not found: {PathService.MaskPath(fullPath)}");
                    return;
                }

                if (!PathService.IsReasonableFileSize(fullPath, 10 * 1024 * 1024))
                {
                    Logger.LogWarning($"Rejected due to size: {PathService.MaskPath(fullPath)}");
                    return;
                }

                Project loadedProject = ProjectSerializer.GetProjectFromFile(fullPath);
                Session.SelectedProject = loadedProject;
                Session.SelectedProject.IsModified = isModifiedState;
                Session.ProjectFilePath = fullPath;
                RecentProjectsManager.AddRecentProject(fullPath);

                Logger.LogInfo($"Loaded Project: '{Session.SelectedProject}'");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading project: {ex.Message}");
            }
        }

    }
}
