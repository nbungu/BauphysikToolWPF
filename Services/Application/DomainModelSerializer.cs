using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Services.Application
{
    public static class DomainModelSerializer
    {
        public static void SaveProjectToFile(Project? project, string filePath)
        {
            if (project is null || string.IsNullOrEmpty(filePath))
            {
                Logger.LogError("Project or file path is null or empty. Cannot save project.");
                return;
            }
            try
            {
                Logger.LogInfo($"Start saving project to file: {filePath}");
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };

                string jsonString = JsonSerializer.Serialize(project, options);
                File.WriteAllText(filePath, jsonString);
                Logger.LogInfo($"Successfully saved project to file: {filePath}");
                MainWindow.ShowToast($"Projekt '{Session.SelectedProject?.Name ?? string.Empty}' gespeichert unter {filePath}.", ToastType.Success);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving project to file: {filePath}, {e.Message}");
                MainWindow.ShowToast($"Projekt '{Session.SelectedProject?.Name ?? string.Empty}' konnte nicht gespeichert werden: {e.Message}.", ToastType.Error);
            }
        }

        public static Project GetProjectFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Logger.LogError($"File path is null or empty or file does not exist: {filePath}");
                return Project.Empty;
            }
            try
            {
                Logger.LogInfo($"Start reading project from file: {filePath}");
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };
                Project? project = JsonSerializer.Deserialize<Project>(jsonString, options);
                if (project != null)
                {
                    Logger.LogInfo($"Successfully read project from file: {filePath}");
                    project.CreatedByUser = true;
                    project.IsModified = false;
                    return project;
                }
                Logger.LogWarning($"Could not read from project file: {filePath}");
                return new Project();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error reading project from file: {filePath}, {e.Message}");
                return new Project();
            }
        }
    }
}
