using System;
using System.IO;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using System.Linq;
using SQLiteNetExtensions.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using BauphysikToolWPF.Models.JsonDependencies;

namespace BauphysikToolWPF.Services
{
    public static class ApplicationServices
    {
        public static string RecentProjectsFilePath = GetRecentProjectsFilePath();
        public static string DownloadsFolderPath = GetDownloadsFolderPath();
        public static string LocalAppDataPath = GetLocalAppDataPath();
        public static string LocalProgramDataPath = GetLocalProgramDataPath();

        public static void SaveProjectToFile(Project project, string filePath)
        {
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
                MainWindow.ShowToast($"Projekt '{UserSaved.SelectedProject.Name}' gespeichert unter {filePath}.", ToastType.Success);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving project to file: {filePath}, {e.Message}");
                MainWindow.ShowToast($"Projekt '{UserSaved.SelectedProject.Name}' konnte nicht gespeichert werden: {e.Message}.", ToastType.Error);
            }
        }

        public static Project LoadProjectFromFile(string filePath)
        {
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

        public static void CreateNewProject()
        {
            UserSaved.SelectedProject = new Project();
            UserSaved.SelectedProject.CreatedByUser = true;
            UserSaved.ProjectFilePath = "";
            MainWindow.ShowToast("Neues Projekt erstellt!", ToastType.Success);
        }

        public static void WriteToConnectedDatabase(Project project)
        {
            UpdateFullProject(project);
        }

        private static void UpdateFullProject(Project project)
        {
            try
            {
                Logger.LogInfo($"Start saving project to Database: {DatabaseAccess.ConnectionString}");
                project.UpdateTimestamp();
                DatabaseAccess.Database.Update(project);

                // Remove all deleted Elements in DB if ElementIds of local Elements is not found in DB
                RemoveDeletedElementsFromDb(project);

                // Update existing elements
                foreach (var element in project.Elements.Where(e => e.Id != -1))
                {
                    element.ProjectId = project.Id; // Important to update to the new FK (ProjectId)
                    DatabaseAccess.Database.Update(element);

                    // Remove all Layers in DB if LayerIds of local Layers is not found in DB
                    RemoveDeletedLayersFromDb(element);

                    // Update exisiting layers
                    foreach (var layer in element.Layers.Where(l => l.Id != -1))
                    {
                        layer.ElementId = element.Id; // Important to update to the new FK (ElementId), since Element was created new
                        DatabaseAccess.Database.Update(layer);

                        // Remove all subConstr in DB if Ids of local subConstr is not found in DB
                        RemoveDeletedSubConstructionsFromDb(layer);

                        // Update exisiting SubConstructions
                        foreach (var subConstr in layer.SubConstructions.Where(s => s.Id != -1))
                        {
                            subConstr.LayerId = layer.Id;
                            DatabaseAccess.Database.Update(subConstr);
                        }

                        // Insert new SubConstructions
                        foreach (var subConstr in layer.SubConstructions.Where(s => s.Id == -1))
                        {
                            subConstr.LayerId = layer.Id;
                            DatabaseAccess.Database.Insert(subConstr);
                        }
                    }

                    // Insert new layers
                    foreach (var layer in element.Layers.Where(l => l.Id == -1))
                    {
                        layer.ElementId = element.Id; // Important to update to the new FK (ElementId), since Element was created new
                        DatabaseAccess.Database.Insert(layer);

                        // Insert new SubConstructions
                        foreach (var subConstr in layer.SubConstructions)
                        {
                            subConstr.LayerId = layer.Id;
                            DatabaseAccess.Database.Insert(subConstr);
                        }
                    }
                }

                // Insert new elements
                foreach (var element in project.Elements.Where(e => e.Id == -1))
                {
                    element.ProjectId = project.Id; // Important to update to the new FK (ProjectId)
                    DatabaseAccess.Database.Insert(element);

                    // Insert new layers
                    foreach (var layer in element.Layers)
                    {
                        layer.ElementId = element.Id; // Important to update to the new FK (ElementId), since Element was created new
                        DatabaseAccess.Database.Insert(layer);

                        // Insert new SubConstructions
                        foreach (var subConstr in layer.SubConstructions)
                        {
                            subConstr.LayerId = layer.Id;
                            DatabaseAccess.Database.Insert(subConstr);
                        }
                    }
                }
                Logger.LogInfo($"Successfully saved project to internal database");
            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving project to database: {e.Message}");
            }
        }

        // TODO: Move 
        private static void RemoveDeletedSubConstructionsFromDb(Layer layer)
        {
            // Remove all subConstr in DB if Ids of local subConstr is not found in DB
            var localSubConstrIds = layer.SubConstructions.Where(s => s.Id != -1).Select(s => s.Id);

            var subConstrInDb = DatabaseAccess.GetSubConstructionQuery()
                .Where(s => s.LayerId == layer.Id)
                .Select(s => s.Id);

            // Find layers to delete (layers in DB but not in local project)
            var subConstrToDelete = subConstrInDb.Except(localSubConstrIds).Cast<object>().ToList();

            // Delete layers not present in local project
            if (subConstrToDelete.Any()) DatabaseAccess.Database.DeleteAllIds<LayerSubConstruction>(subConstrToDelete);
        }
        // TODO: Move 
        private static void RemoveDeletedLayersFromDb(Element element)
        {
            // Remove all Layers in DB if LayerIds of local Layers is not found in DB
            var localLayerIds = element.Layers.Where(e => e.Id != -1).Select(e => e.Id);

            var layersInDb = DatabaseAccess.GetLayersQuery()
                .Where(l => l.ElementId == element.Id)
                .Select(l => l.Id);

            // Find layers to delete (layers in DB but not in local project)
            var layersToDelete = layersInDb.Except(localLayerIds).Cast<object>().ToList();

            // Delete layers not present in local project
            if (layersToDelete.Any()) DatabaseAccess.Database.DeleteAllIds<Layer>(layersToDelete);
        }
        // TODO: Move 
        private static void RemoveDeletedElementsFromDb(Project project)
        {
            // Remove all deleted Elements in DB if ElementIds of local Elements is not found in DB
            var localElementIds = project.Elements.Where(e => e.Id != -1).Select(e => e.Id);

            var elementsInDb = DatabaseAccess.GetElementsQuery()
                .Where(e => e.ProjectId == project.Id)
                .Select(e => e.Id);

            // Find elements to delete (elements in DB but not in local project)
            var elementsToDelete = elementsInDb.Except(localElementIds).Cast<object>().ToList();

            // Delete elements not present in local project
            if (elementsToDelete.Any()) DatabaseAccess.Database.DeleteAllIds<Element>(elementsToDelete);
        }

        // TODO:
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
                recentProjects.Insert(0, new RecentProjectJson { FileName = fileName, FilePath = filePath, LastOpened = TimeStamp.GetCurrentUnixTimestamp() });
            }

            if (recentProjects.Count > 8) recentProjects.RemoveAt(5);
            SaveRecentProjects(recentProjects);
        }
        // TODO:
        public static List<RecentProjectJson> LoadRecentProjects()
        {
            if (File.Exists(RecentProjectsFilePath))
            {
                string json = File.ReadAllText(RecentProjectsFilePath);
                var recentEntries = JsonSerializer.Deserialize<List<RecentProjectJson>>(json);
                if (recentEntries != null) return recentEntries;
            }
            return new List<RecentProjectJson>(0);
        }
        // TODO:
        public static void SaveRecentProjects(List<RecentProjectJson> recentProjects)
        {
            string json = JsonSerializer.Serialize(recentProjects.ToList());
            File.WriteAllText(RecentProjectsFilePath, json);
        }

        #region Paths

        /// <summary>
        /// Path to: %appdata%
        /// </summary>
        /// <returns></returns>
        private static string GetLocalProgramDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Path to: %appdata%/BauphysikTool
        /// </summary>
        /// <returns></returns>
        private static string GetLocalAppDataPath()
        {
            string programDataPath = GetLocalProgramDataPath();
            string appFolder = Path.Combine(programDataPath, "BauphysikTool");
            return appFolder;
        }

        /// <summary>
        /// Retrieves the path to the user's Downloads folder.
        /// </summary>
        /// <returns>
        /// The full path to the Downloads folder. If the folder cannot be retrieved using the Windows API, 
        /// the method falls back to the default path: UserProfile + "Downloads".
        /// </returns>
        /// <remarks>
        /// This method first attempts to retrieve the Downloads folder using the Windows Shell API function
        /// SHGetKnownFolderPath, which accounts for custom folder locations. If this fails (e.g., due to
        /// relocation or API errors), it falls back to constructing the path manually based on the user's
        /// profile directory.
        /// </remarks>
        /// <exception cref="ExternalException">
        /// Thrown if the Windows API fails to retrieve the Downloads folder path.
        /// </exception>
        private static string GetDownloadsFolderPath()
        {
            try
            {
                // Attempt to retrieve the Downloads folder path
                Guid downloadsFolderGuid = new Guid("374DE290-123F-4565-9164-39C4925E467B");
                IntPtr outPath;

                int result = SHGetKnownFolderPath(downloadsFolderGuid, 0, IntPtr.Zero, out outPath);

                if (result >= 0) // Success
                {
                    string path = Marshal.PtrToStringUni(outPath) ?? String.Empty;
                    Marshal.FreeCoTaskMem(outPath);
                    return path;
                }
                else
                {
                    throw new ExternalException("Failed to retrieve Downloads folder path.", result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving Downloads folder: {ex.Message}");
                // Fallback to UserProfile + "Downloads"
                string fallbackPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                Console.WriteLine("Falling back to default path:");
                return fallbackPath;
            }
        }

        private static string GetRecentProjectsFilePath(bool forceReplace = false)
        {
            try
            {
                // User-specific AppData folder
                string appFolder = GetLocalAppDataPath();
                string recentProjectsFilePath = Path.Combine(appFolder, "recently_used.json");

                // Ensure the directory exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Example: Copy the file from the output folder to ProgramData if it doesn't already exist
                string sourceRecentProjectFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Services\\recently_used.json")); ;

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
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Services\\recently_used.json")); ;
            }
        }
        
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr pszPath);

        #endregion
    }
}
