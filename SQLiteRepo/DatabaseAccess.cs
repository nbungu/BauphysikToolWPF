using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteNetExtensions.Extensions;
using System.Windows.Media.Media3D;

namespace BauphysikToolWPF.SQLiteRepo
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)

    public static class DatabaseAccess // publisher of 'LayersChanged' event
    {
        private static string dbPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\SQLiteRepo\\DemoDB.db"));
        private static SQLiteConnection sqlConn = new SQLiteConnection(dbPath);

        //The subscriber class must register to LayerAdded event and handle it with the method whose signature matches Notify delegate
        public static event Notify LayersChanged; // event
        public static event Notify ElementsChanged; // event

        // (Instanzen-) Konstruktor nicht möglich bei statischen Klassen
        /*public DatabaseAccess()
        {
        }*/

        public static void OnLayersChanged() //protected virtual method
        {
            LayersChanged?.Invoke(); //if LayerAdded is not null then call delegate
        }
        public static void OnElementsChanged() //protected virtual method
        {
            ElementsChanged?.Invoke(); //if LayerAdded is not null then call delegate
        }

        // Retreive Data from Table "Layer"
        public static List<Layer> GetLayers()
        {
            List<Layer> layers = sqlConn.GetAllWithChildren<Layer>(); // old Method: List<Layer> layers = sqlConn.Table<Layer>().ToList();
            return layers;
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

        public static int DeleteLayer(Layer layer)
        {
            int i = sqlConn.Delete(layer);
            OnLayersChanged();
            return i;
        }

        public static int DeleteAllLayers()
        {
            int i = sqlConn.DeleteAll<Layer>();
            OnLayersChanged();
            return i;
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

        // Retreive Data from Table "EnvVars"
        public static List<EnvVars> GetEnvVars()
        {
            List<EnvVars> envVars = sqlConn.Table<EnvVars>().ToList();
            return envVars;
        }

        public static int CreateEnvVars(EnvVars envVars)
        {
            return sqlConn.Insert(envVars);
        }

        public static int UpdateEnvVars(EnvVars envVars)
        {
            return sqlConn.Update(envVars);
        }

        public static int DeleteEnvVars(EnvVars envVars)
        {
            return sqlConn.Delete(envVars);
        }

        public static int DeleteAllEnvVars()
        {
            return sqlConn.DeleteAll<EnvVars>();
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
            sqlConn.UpdateWithChildren(element);
        }

        public static int DeleteElement(Element element)
        {
            return sqlConn.Delete(element);
        }

        public static int DeleteElementById(int elementId)
        {
            return sqlConn.Delete(elementId); // ON DELETE CASCADE -> deletes corresp. Layers via the foreignkey constraint
        }

        public static int DeleteAllElements()
        {
            return sqlConn.DeleteAll<Element>();
        }
        public static Element QueryElementsById(int id)
        {
            //return sqlConn.Query<Element>("SELECT * FROM Element WHERE ElementId == " + id).First();

            return sqlConn.GetWithChildren<Element>(id);
        }
    }
}
