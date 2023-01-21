using BauphysikToolWPF.SQLiteRepo;
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

        // Save all as unordered collection. Key must be unique!
        public static Dictionary<string, double> UserEnvVars = new Dictionary<string, double>();

        //------Eigenschaften-----//
        private static KeyValuePair<string, double> ti;
        public static KeyValuePair<string, double> Ti
        {
            get { return ti; }
            set
            {
                ti = value;
                UserEnvVars.Add(ti.Key, ti.Value);
            }
        }
        private static KeyValuePair<string, double> te;
        public static KeyValuePair<string, double> Te
        {
            get { return te; }
            set
            {
                te = value;
                UserEnvVars.Add(te.Key, te.Value);
            }
        }
        private static KeyValuePair<string, double> rsi;
        public static KeyValuePair<string, double> Rsi
        {
            get { return rsi; }
            set
            {
                rsi = value;
                UserEnvVars.Add(rsi.Key, rsi.Value);
            }
        }
        private static KeyValuePair<string, double> rse;
        public static KeyValuePair<string, double> Rse
        {
            get { return rse; }
            set
            {
                rse = value;
                UserEnvVars.Add(rse.Key, rse.Value);
            }
        }
        private static KeyValuePair<string, double> rel_Fi;
        public static KeyValuePair<string, double> Rel_Fi
        {
            get { return rel_Fi; }
            set
            {
                rel_Fi = value;
                UserEnvVars.Add(rel_Fi.Key, rel_Fi.Value);
            }
        }
        private static KeyValuePair<string, double> rel_Fe;
        public static KeyValuePair<string, double> Rel_Fe
        {
            get { return rel_Fe; }
            set
            {
                rel_Fe = value;
                UserEnvVars.Add(rel_Fe.Key, rel_Fe.Value);
            }
        }
    }
}
