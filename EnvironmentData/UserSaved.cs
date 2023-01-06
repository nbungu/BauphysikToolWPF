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
    }
}
