using System.Collections.Generic;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;

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

        // TODO: remove hardcoded value
        public static Project CurrentProject { get; set; } = DatabaseAccess.QueryProjectById(1);

        // Unordered Collection. Key must be unique!
        private static readonly Dictionary<string, double> _userEnvVars = new Dictionary<string, double>(6)
        {
            {"Ti", DatabaseAccess.QueryEnvVarsBySymbol("Ti")[0].Value },
            {"Te", DatabaseAccess.QueryEnvVarsBySymbol("Te")[0].Value },
            {"Rsi", DatabaseAccess.QueryEnvVarsBySymbol("Rsi")[0].Value },
            {"Rse", DatabaseAccess.QueryEnvVarsBySymbol("Rse")[0].Value },
            {"Rel_Fi", DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi")[0].Value },
            {"Rel_Fe", DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe")[0].Value },
        };

        //------Eigenschaften-----//
        // Kapselung von _userEnvVars
        public static double Ti
        {
            get => _userEnvVars["Ti"];
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
            get => _userEnvVars["Te"];
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
            get => _userEnvVars["Rsi"];
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
            get => _userEnvVars["Rse"];
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
            get => _userEnvVars["Rel_Fi"];
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
            get => _userEnvVars["Rel_Fe"];
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
