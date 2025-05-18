using BauphysikToolWPF.Models.Database;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Repositories
{
    public static class DatabaseAccess // publisher of e.g. 'LayersChanged' event
    {
        public static readonly string ConnectionString = DatabaseInstaller.GetInstalledDatabase(forceReplace: true);
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

        public static Material QueryMaterialById(int materialId)
        {
            return GetMaterialsQuery().FirstOrDefault(m => m.Id == materialId, Material.Empty);
        }

        #endregion

        #region Construction

        public static IQueryable<Construction> GetConstructionsQuery() => Database.Table<Construction>().AsQueryable();

        public static Construction QueryConstructionById(int constructionId)
        {
            return GetConstructionsQuery().FirstOrDefault(c => c.Id == constructionId, new Construction());
        }
        public static string QueryConstructionNameByConstructionType(ConstructionType constructionType)
        {
            return GetConstructionsQuery().FirstOrDefault(c => c.ConstructionType == constructionType, new Construction()).TypeName;
        }

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

        public static IQueryable<DocumentSource> GetDocumentSourcesQuery() => Database.Table<DocumentSource>().AsQueryable();

        public static DocumentSource QueryDocumentSourceBySourceType(DocumentSourceType sourceType)
        {
            return GetDocumentSourcesQuery().FirstOrDefault(r => (int)r.DocumentSourceType == (int)sourceType, new DocumentSource());
        }
        public static DocumentSource QueryDocumentSourceById(int designDocumentId)
        {
            return GetDocumentSourcesQuery().FirstOrDefault(r => r.Id == designDocumentId, new DocumentSource());
        }
        public static string QueryDocumentSourceNameBySourceType(DocumentSourceType sourceType)
        {
            return GetDocumentSourcesQuery().FirstOrDefault(r => r.DocumentSourceType == sourceType, new DocumentSource()).SourceName;
        }
        public static string QueryDocumentSourceDescrBySourceType(DocumentSourceType sourceType)
        {
            return GetDocumentSourcesQuery().FirstOrDefault(r => r.DocumentSourceType == sourceType, new DocumentSource()).SourceDescription;
        }

        #endregion

        #region Requirement

        public static IQueryable<Requirement> GetRequirementsQuery() => Database.Table<Requirement>().AsQueryable();

        public static List<Requirement> QueryRequirementsBySourceType(DocumentSourceType sourceType)
        {
            var desgnDocumentId = QueryDocumentSourceBySourceType(sourceType).Id;
            return GetRequirementsQuery().Where(e => e.DocumentSourceId == desgnDocumentId).ToList();
        }
        public static List<Requirement> QueryRequirementsByDesignDocumentId(int desgnDocumentId)
        {
            return GetRequirementsQuery().Where(e => e.DocumentSourceId == desgnDocumentId).ToList();
        }

        #endregion

        #region ClimateData

        public static IQueryable<ClimateData> GetClimateDataQuery() => Database.Table<ClimateData>().AsQueryable();

        #endregion

        #region RoomUsageProfile

        public static IQueryable<RoomUsageProfile> GetRoomUsageProfileQuery() => Database.Table<RoomUsageProfile>().AsQueryable();

        public static string QueryRoomUsageProfileNameByRoomUsageType(RoomUsageType usageType)
        {
            return GetRoomUsageProfileQuery().FirstOrDefault(r => r.RoomUsageType == usageType, new RoomUsageProfile()).UsageName;
        }

        #endregion
    }
}
