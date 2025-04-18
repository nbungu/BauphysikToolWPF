using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.UI;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Repositories
{
    public static class DatabaseAccess // publisher of e.g. 'LayersChanged' event
    {
        public static readonly string ConnectionString = DatabaseInstaller.GetInstalledDatabase(forceReplace: false);
        public static readonly SQLiteConnection Database = new SQLiteConnection(ConnectionString);

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
        
        #region Materials

        public static IQueryable<Material> GetMaterialsQuery() => Database.Table<Material>().AsQueryable();

        public static void CreateMaterial(Material material)
        {
            //Only allow adding user defined materials to DB
            if (!material.IsUserDefined) return;
            Database.Insert(material);
        }

        public static void UpdateMaterial(Material material)
        {
            //Only allow updating user defined materials to DB
            if (!material.IsUserDefined) return;
            Database.Update(material);
        }

        public static void DeleteMaterial(Material material)
        {
            Database.Delete(material);
        }

        #endregion

        #region Construction

        public static IQueryable<Construction> GetConstructionsQuery() => Database.Table<Construction>().AsQueryable();

        #endregion

        #region EnvVars

        public static IQueryable<EnvVars> GetEnvVarsQuery() => Database.Table<EnvVars>().AsQueryable();

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

        #region DocumentSource

        public static IQueryable<DocumentSource> GetRequirementSourcesQuery() => Database.Table<DocumentSource>().AsQueryable();

        public static DocumentSource QueryDocumentSourceBySourceType(RequirementSourceType sourceType)
        {
            return GetRequirementSourcesQuery().First(r => (int)r.Source == (int)sourceType);
        }

        #endregion

        #region Requirement

        public static IQueryable<Requirement> GetRequirementsQuery() => Database.Table<Requirement>().AsQueryable();

        public static List<Requirement> GetRequirements()
        {
            return Database.GetAllWithChildren<Requirement>();
        }
        public static List<Requirement> QueryRequirementsBySourceType(RequirementSourceType sourceType)
        {
            var requirementSourceId = QueryDocumentSourceBySourceType(sourceType).Id;
            return GetRequirementsQuery().Where(e => e.DocumentSourceId == requirementSourceId).ToList();
        }

        #endregion
    }
}
