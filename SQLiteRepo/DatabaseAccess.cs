﻿using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BauphysikToolWPF.SQLiteRepo
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)
    public class DatabaseAccess // publisher of 'LayersChanged' event
    {
        // TODO: no absolute Path
        private static string dbPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\SQLiteRepo\\DemoDB.db"));
        private static SQLiteConnection sqlConn = new SQLiteConnection(dbPath);

        //The subscriber class must register to LayerAdded event and handle it with the method whose signature matches Notify delegate
        public static event Notify? LayersChanged; // event
        public static event Notify? ElementsChanged; // event
        public static event Notify? ElementEnvVarsChanged; //event
        public static event Notify? ProjectsChanged; //event

        // event handlers - publisher
        public static void OnLayersChanged() //protected virtual method
        {
            LayersChanged?.Invoke(); //if LayersChanged is not null then call delegate
        }
        public static void OnElementsChanged() //protected virtual method
        {
            ElementsChanged?.Invoke(); //if ElementsChanged is not null then call delegate
        }
        public static void OnElementEnvVarsChanged() //protected virtual method
        {
            ElementEnvVarsChanged?.Invoke(); //if ElementEnvVarsChanged is not null then call delegate
        }
        public static void OnProjectsChanged() //protected virtual method
        {
            ProjectsChanged?.Invoke(); //if ElementEnvVarsChanged is not null then call delegate
        }

        // Retreive Data from Table "Project"
        public static List<Project> GetProjects()
        {
            return sqlConn.GetAllWithChildren<Project>();
        }

        public static void CreateProject(Project project)
        {
            sqlConn.InsertWithChildren(project);
            OnProjectsChanged(); // raises an event
        }

        public static void UpdateProject(Project project)
        {
            sqlConn.Update(project);
        }

        public static void DeleteProject(Project project)
        {
            sqlConn.Delete(project);
            OnProjectsChanged();
        }
        public static Project QueryProjectById(int projectId)
        {
            return sqlConn.GetWithChildren<Project>(projectId);
        }

        // Retreive Data from Table "Layer"
        public static List<Layer> GetLayers()
        {
            return sqlConn.GetAllWithChildren<Layer>(); // old Method: List<Layer> layers = sqlConn.Table<Layer>().ToList();
        }

        public static void CreateLayer(Layer layer)
        {
            sqlConn.InsertWithChildren(layer); //Method from SQLiteExt -> adds a relationship to a child object ('Material') 
            OnLayersChanged(); // raises an event
        }

        public static void UpdateLayer(Layer layer)
        {
            sqlConn.UpdateWithChildren(layer);
        }

        public static void DeleteLayer(Layer layer)
        {
            sqlConn.Delete(layer);
            OnLayersChanged();
        }

        public static void DeleteAllLayers()
        {
            sqlConn.DeleteAll<Layer>();
            OnLayersChanged();
        }
        public static List<Layer> QueryLayersByElementId(int elementId)
        {
            return sqlConn.GetAllWithChildren<Layer>(e => e.ElementId == elementId);
        }

        // Retreive Data from Table "Material"
        public static List<Material> GetMaterials()
        {
            return sqlConn.Table<Material>().ToList();
        }

        public static void CreateMaterial(Material material)
        {
            sqlConn.Insert(material);
        }

        public static void UpdateMaterial(Material material)
        {
            sqlConn.Update(material);
        }

        public static void DeleteMaterial(Material material)
        {
            sqlConn.Delete(material);
        }

        public static List<Material> QueryMaterialByCategory(string category)
        {
            if (category == "*")
                return sqlConn.Query<Material>("SELECT * FROM Material");
            else
                return sqlConn.Query<Material>("SELECT * FROM Material WHERE Category == " + "\"" + category + "\"");
        }
        public static List<Material> QueryMaterialBySearchString(string searchString)
        {
            if (searchString == String.Empty)
                return sqlConn.Query<Material>("SELECT * FROM Material");
            else
                return sqlConn.Query<Material>("SELECT * FROM Material").Where(m => m.Name.Contains(searchString)).ToList();
        }

        // Retreive Data from Table "EnvVars"
        public static List<EnvVars> GetEnvVars()
        {
            return sqlConn.GetAllWithChildren<EnvVars>();
        }

        public static List<EnvVars> QueryEnvVarsBySymbol(string symbol)
        {
            if (symbol == "*")
                return sqlConn.Query<EnvVars>("SELECT * FROM EnvVars");
            else
                return sqlConn.Query<EnvVars>("SELECT * FROM EnvVars WHERE Symbol == " + "\"" + symbol + "\"");
        }

        // Retreive Data from Table "Element"
        public static List<Element> GetElements()
        {
            return sqlConn.GetAllWithChildren<Element>();
        }

        public static void CreateElement(Element element)
        {
            sqlConn.InsertWithChildren(element); //Method from SQLiteExt -> adds a relationship to a child object ('Layers') 
            OnElementsChanged();
        }

        public static void UpdateElement(Element element)
        {
            sqlConn.Update(element);
            OnElementsChanged();
        }

        public static void DeleteElement(Element element)
        {
            sqlConn.Delete(element);
            OnElementsChanged();
        }

        public static void DeleteElementById(int elementId)
        {
            sqlConn.Delete<Element>(elementId);
            OnElementsChanged();
            // ON DELETE CASCADE -> deletes corresp. Layers and ElementEnvVars via the foreignkey constraint
        }
        public static void DeleteAllElements()
        {
            sqlConn.DeleteAll<Element>();
            OnElementsChanged();
        }
        public static Element QueryElementById(int elementId)
        {
            return sqlConn.GetWithChildren<Element>(elementId);
        }
        public static List<Element> QueryElementsByProjectId(int projectId)
        {
            return sqlConn.GetAllWithChildren<Element>(e => e.ProjectId == projectId);
        }

        // Retreive Data from Table "Construction"
        public static List<Construction> GetConstructions()
        {
            return sqlConn.GetAllWithChildren<Construction>();
        }
        public static Construction QueryConstructionById(int constructionId)
        {
            return sqlConn.GetWithChildren<Construction>(constructionId);
        }

        // Retreive Data from Table "ElementEnvVars"
        public static List<ElementEnvVars> GetElementEnvVars()
        {
            return sqlConn.Table<ElementEnvVars>().ToList();
        }
        public static void CreateElementEnvVars(ElementEnvVars elementEnvVars)
        {
            sqlConn.Insert(elementEnvVars);
            OnElementEnvVarsChanged();
        }

        public static int UpdateElementEnvVars(ElementEnvVars elementEnvVars)
        {
            int i = sqlConn.Update(elementEnvVars);
            OnElementEnvVarsChanged();
            return i;
        }

        public static void DeleteElementEnvVars(ElementEnvVars elementEnvVars)
        {
            sqlConn.Delete(elementEnvVars);
            OnElementEnvVarsChanged();
        }

        public static void DeleteAllElementEnvVars()
        {
            sqlConn.DeleteAll<ElementEnvVars>();
            OnElementEnvVarsChanged();
        }

        // Retreive Data from Table "RequirementSource"
        public static List<RequirementSource> GetRequirementSources()
        {
            return sqlConn.GetAllWithChildren<RequirementSource>();
        }
        public static RequirementSource QueryRequirementSourceById(int requirementSourceId)
        {
            return sqlConn.Get<RequirementSource>(requirementSourceId);
        }

        // Retreive Data from Table "Requirement"
        public static List<Requirement> GetRequirements()
        {
            return sqlConn.GetAllWithChildren<Requirement>();
        }
        public static Requirement QueryRequirementById(int requirementId)
        {
            return sqlConn.Get<Requirement>(requirementId);
        }
        public static List<Requirement> QueryRequirementsBySourceId(int requirementSourceId)
        {
            return sqlConn.GetAllWithChildren<Requirement>(e => e.RequirementSourceId == requirementSourceId);
        }
    }
}
