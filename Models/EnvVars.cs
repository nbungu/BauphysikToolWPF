using System.Collections.Generic;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.CustomControls;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    public class EnvVars
    {
        //------Variablen-----//

        // Hinweis auf Normierungsfehler

        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int EnvVarId { get; set; }
        [NotNull]
        public string Symbol { get; set; } = string.Empty;
        [NotNull]
        public double Value { get; set; }
        [NotNull]
        public string Comment { get; set; } = string.Empty;
        public Unit Unit { get; set; }


        //------Not part of the Database-----//

        // m:n relationship with Element
        [ManyToMany(typeof(ElementEnvVars), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Element> Elements { get; set; } = new List<Element>();
        
        // SQLite doesnt allow other types than TEXT, INT, REAL or BLOB 
        //List<Layer> has to be converted into BLOB -> byte[]
        /*public byte[] ConvertListToBLOB()
        {
            byte[] layersByteArray = new byte[layers.Count * sizeof(Layer)];
            Buffer.BlockCopy(layers, 0, layersByteArray, 0, layersByteArray.Length);

            return;
        }*/

        /*public Material correspondingMaterial()
        {
            return DatabaseAccess.GetMaterials().Find(m => m.Id == this.Id);
        }*/

        /*public byte[] ConvertBLOBToList()
        {
            double[] scores;
            byte[] scoresByteArray = person.Scores;
            scores = new double[scoresByteArray.Length / sizeof(double)];
            Buffer.BlockCopy(scoresByteArray, 0, scores, 0, scoresByteArray.Length);
        }*/
    }
}
