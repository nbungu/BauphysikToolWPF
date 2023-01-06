using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace BauphysikToolWPF.EnvironmentData
{
    // TODO: use Class UserEnvVars in SQLite instead
    public static class UserSaved
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        public static int VarSetId { get; set; }
        public static string Ti { get; set; }
        public static string Te { get; set; }
        public static string Rsi { get; set; }
        public static string Rse { get; set; }
        public static string Rel_Fi { get; set; }
        public static string Rel_Fe { get; set; }
        public static double Ti_Value { get; set; }
        public static double Te_Value { get; set; }
        public static double Rsi_Value { get; set; }
        public static double Rse_Value { get; set; }
        public static double Rel_Fi_Value { get; set; }
        public static double Rel_Fe_Value { get; set; }

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
            string[] scores;
            byte[] scoresByteArray = person.Scores;
            scores = new string[scoresByteArray.Length / sizeof(string)];
            Buffer.BlockCopy(scoresByteArray, 0, scores, 0, scoresByteArray.Length);
        }*/
    }
}
