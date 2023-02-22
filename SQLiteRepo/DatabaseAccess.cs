using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BauphysikToolWPF.SQLiteRepo
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters
    public class DatabaseAccess // publisher of e.g. 'LayersChanged' event
    {
        // TODO: no absolute Path
        private static string dbPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\SQLiteRepo\\DemoDB.db"));
        private static SQLiteConnection sqlConn = new SQLiteConnection(dbPath);

        //The subscriber class must register to LayerAdded event and handle it with the method whose signature matches Notify delegate
        public static event Notify? LayersChanged; // event
        public static event Notify? ElementsChanged;
        public static event Notify? ElementEnvVarsChanged;
        public static event Notify? ProjectsChanged;

        // event handlers - publisher
        public static void OnLayersChanged() //protected virtual method
        {
            
            LayersChanged?.Invoke(); //if LayersChanged is not null then call delegate
        }
        public static void OnElementsChanged()
        {
            ElementsChanged?.Invoke();
        }
        public static void OnElementEnvVarsChanged()
        {
            ElementEnvVarsChanged?.Invoke();
        }
        public static void OnProjectsChanged()
        {
            ProjectsChanged?.Invoke();
        }

        /*
         *  SQLiteNetExtensions allows direct Access to the related children of a Class (e.g. from 1:n relation).
         *  
         *  To Access the Related Children of a SQL Class directly, fetch only the parent Class which Holds the Child!
         *  Inheritance of the Child property over more than one hierarchy levels dont't work! Otherwise the Child will be null.
         *  
         *  Construction (child property) can be retrieved from parent 'Element' but then 'Construction' will not hold its Child property 'Requirements'.
         *  Must be fetched directly by id so that its children are fetched aswell.
         *  
         *  For an Object to contain the complete entity tree starting from 'Project', specifiy the recursive operations
         */

        // Retreive Data from Table "Project"
        public static List<Project> GetProjects()
        {
            return sqlConn.GetAllWithChildren<Project>(recursive: true); // Fetch the Projects and all the related entities recursively
        }

        public static void CreateProject(Project project)
        {
            // No need to 'InsertWithChildren', since on 'GetProjects' any Children will be added via FK by SQLiteExtension package
            sqlConn.Insert(project); // Inserts the object in the database recursively
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
            return sqlConn.GetWithChildren<Project>(projectId, recursive: true); // Fetch the Project by ID and all the related entities recursively
        }

        // Retreive Data from Table "Element"
        public static List<Element> GetElements()
        {
            return sqlConn.GetAllWithChildren<Element>(recursive: true);
        }

        public static void CreateElement(Element element)
        {
            // No need to 'InsertWithChildren', since on 'GetElements' any Children will be added via FK by SQLiteExtension package
            sqlConn.Insert(element);
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
        }
        public static void DeleteAllElements()
        {
            sqlConn.DeleteAll<Element>();
            OnElementsChanged();
        }
        public static Element QueryElementById(int elementId)
        {
            return sqlConn.GetWithChildren<Element>(elementId, recursive: true);
        }
        public static List<Element> QueryElementsByProjectId(int projectId)
        {
            return sqlConn.GetAllWithChildren<Element>(e => e.ProjectId == projectId, recursive: true);
        }

        // Retreive Data from Table "Layer"
        public static List<Layer> GetLayers()
        {
            return sqlConn.GetAllWithChildren<Layer>(recursive: true);
        }

        public static void CreateLayer(Layer layer)
        {
            // No need to 'InsertWithChildren', since on 'GetLayers' any Children will be added via FK by SQLiteExtension package
            sqlConn.Insert(layer);
            OnLayersChanged();
        }

        public static void UpdateLayer(Layer layer, bool triggerUpdateEvent = true)
        {
            sqlConn.UpdateWithChildren(layer);

            if (triggerUpdateEvent == false)
                return;

            OnLayersChanged();
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
            return sqlConn.GetAllWithChildren<Layer>(e => e.ElementId == elementId, recursive: true);
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
