using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.Repository
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters

    public static class DatabaseAccess // publisher of e.g. 'LayersChanged' event
    {
        private static readonly string ConnectionString = DatabaseInstaller.GetInitialDatabase();
        public static readonly SQLiteConnection Database = new SQLiteConnection(ConnectionString);

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

        #region IQueryable
        
        public static IQueryable<Project> GetProjectsQuery()
        {
            return Database.Table<Project>().AsQueryable();
        }
        public static IQueryable<Element> GetElementsQuery()
        {
            return Database.Table<Element>().AsQueryable();
        }
        public static IQueryable<Layer> GetLayersQuery()
        {
            return Database.Table<Layer>().AsQueryable();
        }
        public static IQueryable<LayerSubConstruction> GetSubConstructionQuery()
        {
            return Database.Table<LayerSubConstruction>().AsQueryable();
        }
        public static IQueryable<Material> GetMaterialsQuery()
        {
            return Database.Table<Material>().AsQueryable();
        }
        public static IQueryable<EnvVars> GetEnvVarsQuery()
        {
            return Database.Table<EnvVars>().AsQueryable();
        }
        public static IQueryable<Requirement> GetRequirementsQuery()
        {
            return Database.Table<Requirement>().AsQueryable();
        }
        public static IQueryable<RequirementSource> GetRequirementSourcesQuery()
        {
            return Database.Table<RequirementSource>().AsQueryable();
        }
        #endregion

        #region Projects

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
            OnProjectsChanged();
        }

        public static void DeleteProject(Project project)
        {
            Database.Delete(project);
            OnProjectsChanged();
        }
        public static Project QueryProjectById(int projectId)
        {
            return Database.GetWithChildren<Project>(projectId, recursive: true); // Fetch the Project by ID and all the related entities recursively
        }

        #endregion

        #region Materials

        public static List<Material> GetMaterials()
        {
            return Database.Table<Material>().ToList();
        }

        public static void CreateMaterial(Material material)
        {
            //Only allow adding user defined materials to DB
            if (material.Category != MaterialCategory.UserDefined) return;
            Database.Insert(material);
        }

        public static void UpdateMaterial(Material material)
        {
            //Only allow updating user defined materials to DB
            if (material.Category != MaterialCategory.UserDefined) return;
            Database.Update(material);
        }

        public static void DeleteMaterial(Material material)
        {
            Database.Delete(material);
        }

        public static List<Material> QueryMaterialByCategory(MaterialCategory category)
        {
            if (category == MaterialCategory.None) return GetMaterials();
            return GetMaterialsQuery().Where(m => (int)m.Category == (int)category).ToList();
        }

        #endregion

        #region EnvVars

        public static List<EnvVars> GetEnvVars()
        {
            return Database.GetAllWithChildren<EnvVars>();
        }

        public static List<EnvVars> QueryEnvVarsBySymbol(Symbol symbol)
        {
            if (symbol == Symbol.None) return GetEnvVars();
            return GetEnvVarsQuery().Where(e => (int)e.Symbol == (int)symbol).ToList();
        }

        #endregion

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

        // Retreive Data from Table "RequirementSource"
        public static List<RequirementSource> GetRequirementSources()
        {
            return Database.GetAllWithChildren<RequirementSource>();
        }
        public static RequirementSource QueryRequirementSourceBySourceType(RequirementSourceType sourceType)
        {
            return GetRequirementSourcesQuery().First(r => (int)r.Source == (int)sourceType);
        }

        // Retreive Data from Table "Requirement"
        public static List<Requirement> GetRequirements()
        {
            return Database.GetAllWithChildren<Requirement>();
        }
        public static List<Requirement> QueryRequirementsBySourceType(RequirementSourceType sourceType)
        {
            var requirementSourceId = QueryRequirementSourceBySourceType(sourceType).Id;
            return GetRequirementsQuery().Where(e => e.RequirementSourceId == requirementSourceId).ToList();
        }
    }
}
