using BauphysikToolWPF.Models.Application;
using BT.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BauphysikToolWPF.Services.Application
{
    public static class RecentProjectsManager
    {
        public static void AddRecentProject(string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            var recentProjects = GetRecentProjects();
            var existingEntry = recentProjects.FirstOrDefault(p => p?.FilePath == projectFilePath, null);
            if (existingEntry != null)
            {
                existingEntry.LastOpened = TimeStamp.GetCurrentUnixTimestamp();
            }
            else
            {
                recentProjects.Insert(0, new RecentProjectItem { FileName = projectFileName, FilePath = projectFilePath, LastOpened = TimeStamp.GetCurrentUnixTimestamp() });
            }
            if (recentProjects.Count > 8) recentProjects.RemoveAt(5);
            SaveRecentProjects(recentProjects);
        }

        public static List<RecentProjectItem> GetRecentProjects()
        {
            SetupFile();

            if (File.Exists(PathService.UserRecentProjectsFilePath))
            {
                string jsonString = File.ReadAllText(PathService.UserRecentProjectsFilePath);
                List<RecentProjectItem>? recentEntries = JsonSerializer.Deserialize<List<RecentProjectItem>>(jsonString);
                if (recentEntries != null && recentEntries.All(e => e.IsValid)) return recentEntries.OrderByDescending(e => e.LastOpened).ToList();
            }
            return new List<RecentProjectItem>(0);
        }


        #region private methods
        
        private static void SaveRecentProjects(List<RecentProjectItem> recentProjects)
        {
            string jsonString = JsonSerializer.Serialize(recentProjects.ToList());
            File.WriteAllText(PathService.UserRecentProjectsFilePath, jsonString);
        }

        private static void SetupFile(bool forceReplace = false)
        {
            // file under user-specific AppData folder
            string userRecentProjectsFilePath = PathService.UserRecentProjectsFilePath;
            // file under installation directory folder
            string sourceRecentProjectFilePath = PathService.BuildDirRecentProjectsFilePath;

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(PathService.UserProgramDataPath))
                {
                    Directory.CreateDirectory(PathService.UserProgramDataPath);
                }
                // Example: Copy the file from the output folder to ProgramData if it doesn't already exist
                if (!File.Exists(userRecentProjectsFilePath))
                {
                    Logger.LogInfo($"Copying recent_project.json from: {sourceRecentProjectFilePath} to {userRecentProjectsFilePath}");
                    File.Copy(sourceRecentProjectFilePath, userRecentProjectsFilePath);
                }
                if (forceReplace)
                {
                    Logger.LogInfo($"Force replacing recent_projects.json from: {sourceRecentProjectFilePath} to {userRecentProjectsFilePath}");
                    File.Delete(userRecentProjectsFilePath);
                    File.Copy(sourceRecentProjectFilePath, userRecentProjectsFilePath);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not get installed recent_projects.json: {e.Message}");
            }
        }

        #endregion
    }
}
