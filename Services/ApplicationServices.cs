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

namespace BauphysikToolWPF.Services
{
    public static class ApplicationServices
    {
        // TODO: Dokument erzeugen

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
                Project project = JsonSerializer.Deserialize<Project>(jsonString, options);
                if (project != null)
                {
                    Logger.LogInfo($"Successfully read project from file: {filePath}");
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
            UserSaved.SelectedProject = new Project()
            {
                Name = "Neues Projekt",
                IsModified = false,
            };
            UserSaved.ProjectFilePath = "";
        }

        public static void WriteToConnectedDatabase(Project project)
        {
            UpdateFullProject(project);
        }

        private static void UpdateFullProject(Project project)
        {
            try
            {
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

        private static void RemoveDeletedSubConstructionsFromDb(Layer layer)
        {
            // Remove all subConstr in DB if Ids of local subConstr is not found in DB
            var localSubConstrIds = layer.SubConstructions.Where(s => s.Id != -1).Select(s => s.Id);

            var subConstrInDB = DatabaseAccess.GetSubConstructionQuery()
                .Where(s => s.LayerId == layer.Id)
                .Select(s => s.Id);

            // Find layers to delete (layers in DB but not in local project)
            var subConstrToDelete = subConstrInDB.Except(localSubConstrIds).Cast<object>().ToList();

            // Delete layers not present in local project
            if (subConstrToDelete.Any()) DatabaseAccess.Database.DeleteAllIds<LayerSubConstruction>(subConstrToDelete);
        }

        private static void RemoveDeletedLayersFromDb(Element element)
        {
            // Remove all Layers in DB if LayerIds of local Layers is not found in DB
            var localLayerIds = element.Layers.Where(e => e.Id != -1).Select(e => e.Id);

            var layersInDB = DatabaseAccess.GetLayersQuery()
                .Where(l => l.ElementId == element.Id)
                .Select(l => l.Id);

            // Find layers to delete (layers in DB but not in local project)
            var layersToDelete = layersInDB.Except(localLayerIds).Cast<object>().ToList();

            // Delete layers not present in local project
            if (layersToDelete.Any()) DatabaseAccess.Database.DeleteAllIds<Layer>(layersToDelete);
        }

        private static void RemoveDeletedElementsFromDb(Project project)
        {
            // Remove all deleted Elements in DB if ElementIds of local Elements is not found in DB
            var localElementIds = project.Elements.Where(e => e.Id != -1).Select(e => e.Id);

            var elementsInDB = DatabaseAccess.GetElementsQuery()
                .Where(e => e.ProjectId == project.Id)
                .Select(e => e.Id);

            // Find elements to delete (elements in DB but not in local project)
            var elementsToDelete = elementsInDB.Except(localElementIds).Cast<object>().ToList();

            // Delete elements not present in local project
            if (elementsToDelete.Any()) DatabaseAccess.Database.DeleteAllIds<Element>(elementsToDelete);
        }
    }
}
