using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.SQLiteRepo
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)

    public static class DatabaseAccess // publisher of 'LayerAdded' event
    {
        private static string dbPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\SQLiteRepo\\DemoDB.db"));
        private static SQLiteConnection sqlConn = new SQLiteConnection(dbPath);

        //The subscriber class must register to LayerAdded event and handle it with the method whose signature matches Notify delegate
        public static event Notify LayerAdded;      // event
        public static event Notify LayerDeleted;    // event

        // (Instanzen-) Konstruktor nicht möglich bei statischen Klassen
        /*public DatabaseAccess()
        {
        }*/

        public static void OnLayerAdded() //protected virtual method
        {
            LayerAdded?.Invoke(); //if LayerAdded is not null then call delegate
        }
        public static void OnLayerDeleted() //protected virtual method
        {
            LayerDeleted?.Invoke(); //if LayerDeleted is not null then call delegate
        }

        // Retreive Data from Table "Layer"
        public static List<Layer> GetLayers()
        {
            List<Layer> layers = sqlConn.Table<Layer>().ToList();
            return layers;
        }

        public static int CreateLayer(Layer layer) // returns int, which represents the number of rows that were inserted into the table
        {
            int i = sqlConn.Insert(layer);
            OnLayerAdded(); //raises an event
            return i;
        }

        public static int UpdateLayer(Layer layer)
        {
            return sqlConn.Update(layer);
        }

        public static int DeleteLayer(Layer layer)
        {
            int i = sqlConn.Delete(layer);
            OnLayerDeleted();
            return i;
        }

        public static int DeleteAllLayers()
        {
            int i = sqlConn.DeleteAll<Layer>();
            // TODO:
            // Reset AutoIncrement status from 'sqlite_sequence' table
            // delete from sqlite_sequence where name='your_table';
            OnLayerDeleted();
            return i;
        }

        // Retreive Data from Table "Material"
        public static List<Material> GetMaterials()
        {
            List<Material> materials = sqlConn.Table<Material>().ToList();
            return materials;
        }

        public static int CreateMaterial(Material material) // returns int, which represents the number of rows that were inserted into the table
        {
            return sqlConn.Insert(material);
        }

        public static int UpdateMaterial(Material material)
        {
            return sqlConn.Update(material);
        }

        public static int DeleteMaterial(Material material)
        {
            return sqlConn.Delete(material);
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

        /*public static List<Material> LinqZeroBalance()
        {
            return sqlConn.Table<Material>().Where(a => a.BulkDensity == 0).ToList();
        }*/
    }
}
