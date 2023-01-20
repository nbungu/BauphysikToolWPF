using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static KeyValuePair<string, double> Ti { get; set; }
        public static KeyValuePair<string, double> Te { get; set; }
        public static KeyValuePair<string, double> Rsi { get; set; }
        public static KeyValuePair<string, double> Rse { get; set; }
        public static KeyValuePair<string, double> Rel_Fi { get; set; }
        public static KeyValuePair<string, double> Rel_Fe { get; set; }

        //als eine Hastable

        
    }
}
