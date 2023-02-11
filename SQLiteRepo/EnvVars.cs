using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class EnvVars
    {
        //------Variablen-----//

        // Hinweis auf Normierungsfehler

        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int EnvVarId { get; set; }

        [NotNull]
        public string Symbol { get; set; }

        [NotNull]
        public double Value { get; set; }

        public string? Comment { get; set; }

        public string? Unit { get; set; }

        //------Not part of the Database-----//

        // m:n relationship with Element
        [ManyToMany(typeof(ElementEnvVars), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Element> Elements { get; set; }


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
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId);
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
