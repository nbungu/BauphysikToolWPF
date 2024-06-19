using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using System.Collections.Generic;

namespace BauphysikToolWPF.SessionData
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)
    public static class UserSaved // publisher of 'EnvVarsChanged' event
    {
        // The subscriber class must register to these event and handle it with the method whose signature matches Notify delegate
        public static event Notify? SelectedProjectChanged; // event
        public static event Notify? SelectedElementChanged; // event
        public static event Notify? SelectedLayerChanged; // event
        public static event Notify? EnvVarsChanged; // event
        public static Project? SelectedProject;

        // event handlers - publisher
        public static void OnSelectedProjectChanged() //protected virtual method
        {
            SelectedProjectChanged?.Invoke(); //if SelectedProjectChanged is not null then call delegate
        }
        public static void OnSelectedElementChanged() //protected virtual method
        {
            SelectedElementChanged?.Invoke(); //if LayersChanged is not null then call delegate
        }
        public static void OnSelectedLayerChanged() //protected virtual method
        {
            SelectedLayerChanged?.Invoke(); //if LayersChanged is not null then call delegate
        }
        public static void OnEnvVarsChanged() //protected virtual method
        {
            EnvVarsChanged?.Invoke(); // if EnvVarsChanged is not null then call delegate
        }

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        private static int _selectedElementId = -1;
        public static int SelectedElementId
        {
            get => _selectedElementId;
            set
            {
                _selectedElementId = value;
                //OnSelectedElementChanged();
            }
        }

        /// <summary>
        /// Zeigt auf das entsprechende Element aus dem aktuellen Projekt auf Basis der InternalID von 'SelectedElementId'
        /// </summary>
        public static Element? SelectedElement => SelectedProject?.Elements.Find(e => e.InternalId == SelectedElementId);

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        private static int _selectedLayerId = -1;

        public static int SelectedLayerId
        {
            get => _selectedLayerId;
            set
            {
                _selectedLayerId = value;
                //OnSelectedLayerChanged();
            }
        }

        /// <summary>
        /// Zeigt auf den entsprechenden Layer aus dem aktuellen Element auf Basis der LayerPosition von 'SelectedLayerPosition'
        /// </summary>
        public static Layer? SelectedLayer => SelectedElement?.Layers.Find(e => e.InternalId == SelectedLayerId);

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
