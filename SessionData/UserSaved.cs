using System.Collections.Generic;

namespace BauphysikToolWPF.SessionData
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)
    public static class UserSaved // publisher of 'EnvVarsChanged' event
    {
        public static event Notify EnvVarsChanged; // event

        // event handlers - publisher
        public static void OnEnvVarsChanged() //protected virtual method
        {
            EnvVarsChanged?.Invoke(); // if EnvVarsChanged is not null then call delegate
        }

        // Unordered Collection. Key must be unique!
        private static Dictionary<string, double> userEnvVars = new Dictionary<string, double>()
        {
            {"Ti", 0 },
            {"Te", 0 },
            {"Rsi", 0 },
            {"Rse", 0 },
            {"Rel_Fi", 0 },
            {"Rel_Fe", 0 },
        };
        public static Dictionary<string, double> UserEnvVars
        {
            get { return userEnvVars; }
        }

        // Saves the last SelectedIndex of the combo boxes
        private static Dictionary<string, int> comboBoxSelection = new Dictionary<string, int>()
        {
            {"Ti_ComboBox", -1 },
            {"Te_ComboBox", -1 },
            {"Rsi_ComboBox", -1 },
            {"Rse_ComboBox", -1 },
            {"Rel_Fi_ComboBox", -1 },
            {"Rel_Fe_ComboBox", -1 },
        };
        public static Dictionary<string, int> ComboBoxSelection
        {
            get { return comboBoxSelection; }
        }

        //------Eigenschaften-----//
        // Kapselung von userEnvVars
        public static double Ti
        {
            get { return userEnvVars["Ti"]; }
            set
            {
                if (value != userEnvVars["Ti"])
                {
                    userEnvVars["Ti"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Te
        {
            get { return userEnvVars["Te"]; }
            set
            {
                if (value != userEnvVars["Te"])
                {
                    userEnvVars["Te"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rsi
        {
            get { return userEnvVars["Rsi"]; }
            set
            {
                if (value != userEnvVars["Rsi"])
                {
                    userEnvVars["Rsi"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rse
        {
            get { return userEnvVars["Rse"]; }
            set
            {
                if (value != userEnvVars["Rse"])
                {
                    userEnvVars["Rse"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rel_Fi
        {
            get { return userEnvVars["Rel_Fi"]; }
            set
            {
                if (value != userEnvVars["Rel_Fi"])
                {
                    userEnvVars["Rel_Fi"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rel_Fe
        {
            get { return userEnvVars["Rel_Fe"]; }
            set
            {
                if (value != userEnvVars["Rel_Fe"])
                {
                    userEnvVars["Rel_Fe"] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
    }
}
