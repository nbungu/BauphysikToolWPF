using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;

namespace BauphysikToolWPF.SessionData
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)
    public static class UserSaved // publisher of 'EnvVarsChanged' event
    {
        public static event Notify? EnvVarsChanged; // event

        // event handlers - publisher
        public static void OnEnvVarsChanged() //protected virtual method
        {
            EnvVarsChanged?.Invoke(); // if EnvVarsChanged is not null then call delegate
        }

        // Unordered Collection. Key must be unique!
        private static readonly Dictionary<string, double> _userEnvVars = new Dictionary<string, double>()
        {
            {"Ti", DatabaseAccess.QueryEnvVarsBySymbol("Ti")[0].Value },
            {"Te", DatabaseAccess.QueryEnvVarsBySymbol("Te")[0].Value },
            {"Rsi", DatabaseAccess.QueryEnvVarsBySymbol("Rsi")[0].Value },
            {"Rse", DatabaseAccess.QueryEnvVarsBySymbol("Rse")[0].Value },
            {"Rel_Fi", DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi")[0].Value },
            {"Rel_Fe", DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe")[0].Value },
        };
        public static Dictionary<string, double> UserEnvVars => _userEnvVars;

        //------Eigenschaften-----//
        // Kapselung von _userEnvVars
        public static double Ti
        {
            get { return _userEnvVars["Ti"]; }
            set
            {
                if (value != _userEnvVars["Ti"])
                {
                    _userEnvVars["Ti"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Te
        {
            get { return _userEnvVars["Te"]; }
            set
            {
                if (value != _userEnvVars["Te"])
                {
                    _userEnvVars["Te"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rsi
        {
            get { return _userEnvVars["Rsi"]; }
            set
            {
                if (value != _userEnvVars["Rsi"])
                {
                    _userEnvVars["Rsi"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rse
        {
            get { return _userEnvVars["Rse"]; }
            set
            {
                if (value != _userEnvVars["Rse"])
                {
                    _userEnvVars["Rse"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rel_Fi
        {
            get { return _userEnvVars["Rel_Fi"]; }
            set
            {
                if (value != _userEnvVars["Rel_Fi"])
                {
                    _userEnvVars["Rel_Fi"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rel_Fe
        {
            get { return _userEnvVars["Rel_Fe"]; }
            set
            {
                if (value != _userEnvVars["Rel_Fe"])
                {
                    _userEnvVars["Rel_Fe"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
    }
}
