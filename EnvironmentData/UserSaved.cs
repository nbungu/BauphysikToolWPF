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
        public static Dictionary<string, double> UserEnvVars = new Dictionary<string, double>()
        {
            {"Ti", 0 },
            {"Te", 0 },
            {"Rsi", 0 },
            {"Rse", 0 },
            {"Rel_Fi", 0 },
            {"Rel_Fe", 0 },
        };

        //------Eigenschaften-----//
        // Kapselung von UserEnvVars
        public static double Ti
        {
            get { return UserEnvVars["Ti"]; }
            set
            {
                UserEnvVars["Ti"] = value;
            }
        }
        public static double Te
        {
            get { return UserEnvVars["Te"]; }
            set
            {
                UserEnvVars["Te"] = value;
            }
        }
        public static double Rsi
        {
            get { return UserEnvVars["Rsi"]; }
            set
            {
                UserEnvVars["Rsi"] = value;
            }
        }
        public static double Rse
        {
            get { return UserEnvVars["Rse"]; }
            set
            {
                UserEnvVars["Rse"] = value;
            }
        }
        public static double Rel_Fi
        {
            get { return UserEnvVars["Rel_Fi"]; }
            set
            {
                UserEnvVars["Rel_Fi"] = value;
            }
        }
        public static double Rel_Fe
        {
            get { return UserEnvVars["Rel_Fe"]; }
            set
            {
                UserEnvVars["Rel_Fe"] = value;
            }
        }
    }
}
