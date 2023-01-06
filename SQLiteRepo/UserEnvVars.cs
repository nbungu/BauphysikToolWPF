using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class UserEnvVars
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int VarSetId { get; set; }

        [NotNull]
        public double Ti { get; set; }

        [NotNull]
        public double Te { get; set; }

        [NotNull]
        public double Rsi { get; set; }

        [NotNull]
        public double Rse { get; set; }

        [NotNull]
        public double Rel_Fi { get; set; }

        [NotNull]
        public double Rel_Fe { get; set; }

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
