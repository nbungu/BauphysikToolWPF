using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BauphysikToolWPF.Models.Application;
using BauphysikToolWPF.Services.Application;
using BT.Logging;

namespace BauphysikToolWPF.Repositories
{
    public static class RecentProjectsManager
    {
        public static readonly string RecentProjectsFilePath = GetRecentProjectsFilePath();

        public static void AddRecentProject(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var recentProjects = LoadRecentProjects();
            var existingEntry = recentProjects.FirstOrDefault(p => p.FilePath == filePath);
            if (existingEntry != null)
            {
                existingEntry.LastOpened = TimeStamp.GetCurrentUnixTimestamp();
            }
            else
            {
                recentProjects.Insert(0, new RecentProjectItem { FileName = fileName, FilePath = filePath, LastOpened = TimeStamp.GetCurrentUnixTimestamp() });
            }

            if (recentProjects.Count > 8) recentProjects.RemoveAt(5);
            SaveRecentProjects(recentProjects);
        }

        public static List<RecentProjectItem> LoadRecentProjects()
        {
            if (File.Exists(RecentProjectsFilePath))
            {
                string json = File.ReadAllText(RecentProjectsFilePath);
                var recentEntries = JsonSerializer.Deserialize<List<RecentProjectItem>>(json);
                if (recentEntries != null && recentEntries.All(e => e.IsValid)) return recentEntries;
            }
            return new List<RecentProjectItem>(0);
        }

        public static void SaveRecentProjects(List<RecentProjectItem> recentProjects)
        {
            string json = JsonSerializer.Serialize(recentProjects.ToList());
            File.WriteAllText(RecentProjectsFilePath, json);
        }

        private static string GetRecentProjectsFilePath(bool forceReplace = false)
        {
            try
            {
                // User-specific AppData folder
                string appFolder = PathService.LocalAppDataPath;
                string recentProjectsFilePath = Path.Combine(appFolder, "recent_projects.json");

                // Ensure the directory exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Example: Copy the file from the output folder to ProgramData if it doesn't already exist
                string sourceRecentProjectFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\recent_projects.json"));

                if (!File.Exists(recentProjectsFilePath))
                {
                    File.Copy(sourceRecentProjectFile, recentProjectsFilePath);
                }
                if (forceReplace)
                {
                    File.Delete(recentProjectsFilePath);
                    File.Copy(sourceRecentProjectFile, recentProjectsFilePath);
                }
                return recentProjectsFilePath;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not get recently used projects file: {e.Message}");
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\recent_projects.json"));
            }
        }
    }
}
