using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace BauphysikToolWPF.Repository
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters

    public static class DatabaseAccess // publisher of e.g. 'LayersChanged' event
    {
        //public static string ConnectionString = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\SQLiteRepo\\DemoDB.db"));
        private static readonly string ConnectionString = DatabaseInstaller.Install();
        private static readonly SQLiteConnection Database = new SQLiteConnection(ConnectionString);

        //The subscriber class must register to LayerAdded event and handle it with the method whose signature matches Notify delegate
        public static event Notify? LayersChanged; // event
        public static event Notify? ElementsChanged;
        public static event Notify? ElementEnvVarsChanged;
        public static event Notify? ProjectsChanged;

        // event handlers - publisher
        private static void OnLayersChanged() //protected virtual method
        {
            LayersChanged?.Invoke(); //if LayersChanged is not null then call delegate
        }
        private static void OnElementsChanged()
        {
            ElementsChanged?.Invoke();
        }
        private static void OnElementEnvVarsChanged()
        {
            ElementEnvVarsChanged?.Invoke();
        }
        private static void OnProjectsChanged()
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
            return Database.GetAllWithChildren<Project>(recursive: true); // Fetch the Projects and all the related entities recursively
        }

        public static void CreateProject(Project project)
        {
            // No need to 'InsertWithChildren', since on 'GetProjects' any Children will be added via FK by SQLiteExtension package
            Database.Insert(project); // Inserts the object in the Database recursively
            OnProjectsChanged(); // raises an event
        }

        public static void UpdateProject(Project project)
        {
            Database.Update(project);
        }

        public static void DeleteProject(Project project)
        {
            Database.Delete(project);
            OnProjectsChanged();
        }
        public static Project QueryProjectById(int projectId)
        {
            projectId = Convert.ToInt32(projectId);
            return Database.GetWithChildren<Project>(projectId, recursive: true); // Fetch the Project by ID and all the related entities recursively
        }

        // Retreive Data from Table "Element"
        public static List<Element> GetElements()
        {
            return Database.GetAllWithChildren<Element>(recursive: true);
        }

        public static void CreateElement(Element element, bool withChildren = false)
        {
            if (withChildren)                           // When copying an Element: Insert with children
                Database.InsertWithChildren(element);
            else Database.Insert(element);               // Default case: Create/Edit a Element: No need to 'InsertWithChildren', since on 'GetElements' any Children will be added via FK by SQLiteExtension package

            OnElementsChanged();
        }

        public static void UpdateElement(Element element)
        {
            Database.Update(element);
            OnElementsChanged();
        }

        public static void DeleteElement(Element element)
        {
            Database.Delete(element);
            OnElementsChanged();
        }

        public static void DeleteElementById(int elementId)
        {
            elementId = Convert.ToInt32(elementId);
            Database.Delete<Element>(elementId);
            OnElementsChanged();
        }
        public static void DeleteAllElements()
        {
            Database.DeleteAll<Element>();
            OnElementsChanged();
        }
        public static Element QueryElementById(int elementId, bool layersSorted = false)
        {
            elementId = Convert.ToInt32(elementId);
            Element element = Database.GetWithChildren<Element>(elementId, recursive: true);

            // default value = false, mostly not needed to return Element with SORTED Layers!
            if (layersSorted)
                element.Layers = QueryLayersByElementId(elementId);

            return element;
        }
        public static List<Element> QueryElementsByProjectId(int projectId, ElementSortingType sortingType = ElementSortingType.DateAscending, bool ascending = true)
        {
            projectId = Convert.ToInt32(projectId);
            List<Element> elements = Database.GetAllWithChildren<Element>(e => e.ProjectId == projectId, recursive: true);

            if (sortingType == ElementSortingType.DateAscending) return elements;

            elements.Sort(new ElementOrganisor(sortingType)); // use of List<T>.Sort(IComparer<T>) method
            return elements;
            
        }

        // Retreive Data from Table "Layer"
        public static void CreateLayer(Layer layer, bool triggerUpdateEvent = true, bool assignEffectiveLayers = true)
        {
            // No need to 'InsertWithChildren', since on 'GetLayers' any Children will be added via FK by SQLiteExtension package
            Database.Insert(layer);

            // True by default: Occurs often when a Layer is deleted
            if (assignEffectiveLayers)
                LayerOrganisor.AssignEffectiveLayers(QueryLayersByElementId(layer.ElementId));

            if (triggerUpdateEvent)
                OnLayersChanged();
        }

        public static void UpdateLayer(Layer layer, bool triggerUpdateEvent = true, bool assignEffectiveLayers = false)
        {
            // No need to 'UpdateWithChildren', since on 'GetLayers' any Children will be added via FK by SQLiteExtension package
            Database.Update(layer);

            // False by default: Occurs rarely when a Layer is updated
            if (assignEffectiveLayers)
                LayerOrganisor.AssignEffectiveLayers(QueryLayersByElementId(layer.ElementId));

            if (triggerUpdateEvent)
                OnLayersChanged();
        }

        public static void DeleteLayer(Layer layer, bool triggerUpdateEvent = true, bool fillLayerGaps = true, bool assignEffectiveLayers = true)
        {
            Database.Delete(layer);

            // True by default: Occurs almost every time a Layer is deleted
            if (fillLayerGaps)
                LayerOrganisor.FillGaps(QueryLayersByElementId(layer.ElementId)); // Remove gaps in the LayerPosition property of current Element

            // True by default: Occurs often when a Layer is deleted
            if (assignEffectiveLayers)
                LayerOrganisor.AssignEffectiveLayers(QueryLayersByElementId(layer.ElementId));

            if (triggerUpdateEvent)
                OnLayersChanged();
        }

        public static void DeleteAllLayers(bool triggerUpdateEvent = true)
        {
            Database.DeleteAll<Layer>();

            if (triggerUpdateEvent)
                OnLayersChanged();
        }
        public static List<Layer> QueryLayersByElementId(int elementId, LayerSortingType sortingType = LayerSortingType.Default)
        {
            elementId = Convert.ToInt32(elementId);
            List<Layer> layers = Database.GetAllWithChildren<Layer>(e => e.ElementId == elementId, recursive: true);

            if (sortingType == LayerSortingType.None)
                return layers;

            layers.Sort(new LayerOrganisor(sortingType)); // use of List<T>.Sort(IComparer<T>) method
            return layers;
        }

        // Retreive Data from Table "Material"
        public static List<Material> GetMaterials()
        {
            return Database.Table<Material>().ToList();
        }

        public static void CreateMaterial(Material material)
        {
            //Only allow adding user defined materials to DB
            if (material.Category != MaterialCategory.UserDefined)
                return;
            Database.Insert(material);
        }

        public static void UpdateMaterial(Material material)
        {
            //Only allow updating user defined materials to DB
            if (material.Category != MaterialCategory.UserDefined)
                return;
            Database.Update(material);
        }

        public static void DeleteMaterial(Material material)
        {
            Database.Delete(material);
        }

        public static List<Material> QueryMaterialByCategory(string category)
        {
            if (category == "*")
                return Database.Query<Material>("SELECT * FROM Material");
            else
                return Database.Query<Material>("SELECT * FROM Material WHERE CategoryName == " + "\"" + category + "\"");
        }
        public static List<Material> QueryMaterialBySearchString(string searchString)
        {
            if (searchString == String.Empty)
                return Database.Query<Material>("SELECT * FROM Material");
            else
                return Database.Query<Material>("SELECT * FROM Material").Where(m => m.Name.Contains(searchString)).ToList();
        }

        // Retreive Data from Table "EnvVars"
        public static List<EnvVars> GetEnvVars()
        {
            return Database.GetAllWithChildren<EnvVars>();
        }

        public static List<EnvVars> QueryEnvVarsBySymbol(string symbol)
        {
            if (symbol == "*")
                return Database.Query<EnvVars>("SELECT * FROM EnvVars");
            else
                return Database.Query<EnvVars>("SELECT * FROM EnvVars WHERE Symbol == " + "\"" + symbol + "\"");
        }

        // Retreive Data from Table "Construction"
        public static List<Construction> GetConstructions()
        {
            return Database.GetAllWithChildren<Construction>();
        }
        public static Construction QueryConstructionById(int constructionId)
        {
            constructionId = Convert.ToInt32(constructionId);
            return Database.GetWithChildren<Construction>(constructionId);
        }

        // Retreive Data from Table "Orientation"
        public static List<Orientation> GetOrientations()
        {
            return Database.Table<Orientation>().ToList();
        }
        public static Orientation QueryOrientationById(int orientationId)
        {
            orientationId = Convert.ToInt32(orientationId);
            return Database.Get<Orientation>(orientationId);
        }

        // Retreive Data from Table "ElementEnvVars"
        public static List<ElementEnvVars> GetElementEnvVars()
        {
            return Database.Table<ElementEnvVars>().ToList();
        }
        public static void CreateElementEnvVars(ElementEnvVars elementEnvVars)
        {
            Database.Insert(elementEnvVars);
            OnElementEnvVarsChanged();
        }

        public static int UpdateElementEnvVars(ElementEnvVars elementEnvVars)
        {
            int i = Database.Update(elementEnvVars);
            OnElementEnvVarsChanged();
            return i;
        }

        public static void DeleteElementEnvVars(ElementEnvVars elementEnvVars)
        {
            Database.Delete(elementEnvVars);
            OnElementEnvVarsChanged();
        }

        public static void DeleteAllElementEnvVars()
        {
            Database.DeleteAll<ElementEnvVars>();
            OnElementEnvVarsChanged();
        }

        // Retreive Data from Table "RequirementSource"
        public static List<RequirementSource> GetRequirementSources()
        {
            return Database.GetAllWithChildren<RequirementSource>();
        }
        public static RequirementSource QueryRequirementSourceById(int requirementSourceId)
        {
            requirementSourceId = Convert.ToInt32(requirementSourceId);
            return Database.Get<RequirementSource>(requirementSourceId);
        }

        // Retreive Data from Table "Requirement"
        public static List<Requirement> GetRequirements()
        {
            return Database.GetAllWithChildren<Requirement>();
        }
        public static Requirement QueryRequirementById(int requirementId)
        {
            requirementId = Convert.ToInt32(requirementId);
            return Database.Get<Requirement>(requirementId);
        }
        public static List<Requirement> QueryRequirementsBySourceId(int requirementSourceId)
        {
            requirementSourceId = Convert.ToInt32(requirementSourceId);
            return Database.GetAllWithChildren<Requirement>(e => e.RequirementSourceId == requirementSourceId);
        }
    }
}
