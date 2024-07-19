using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Models.Helper;

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

        public static Project SelectedProject;

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        public static int SelectedElementId { get; set; } = -1;

        /// <summary>
        /// Zeigt auf das entsprechende Element aus dem aktuellen Projekt auf Basis der InternalID von 'SelectedElementId'
        /// </summary>
        public static Element SelectedElement => SelectedProject.Elements.FirstOrDefault(e => e.InternalId == SelectedElementId, new Element());

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        public static int SelectedLayerId { get; set; } = -1;

        /// <summary>
        /// Zeigt auf den entsprechenden Layer aus dem aktuellen Element auf Basis der LayerPosition von 'SelectedLayerPosition'
        /// </summary>
        public static Layer SelectedLayer => SelectedElement.Layers.FirstOrDefault(e => e.InternalId == SelectedLayerId, new Layer());

        // Unordered Collection. Key must be unique!
        private static readonly Dictionary<Symbol, double> _userEnvVars = new Dictionary<Symbol, double>(6)
        {
            {Symbol.TemperatureInterior, DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureInterior)[0].Value },
            {Symbol.TemperatureExterior, DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureExterior)[0].Value },
            {Symbol.TransferResistanceSurfaceInterior, DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceInterior)[0].Value },
            {Symbol.TransferResistanceSurfaceExterior, DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceExterior)[0].Value },
            {Symbol.RelativeHumidityInterior, DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityInterior)[0].Value },
            {Symbol.RelativeHumidityExterior, DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityExterior)[0].Value },
        };

        //------Eigenschaften-----//
        // Kapselung von _userEnvVars
        public static double Ti
        {
            get => _userEnvVars[Symbol.TemperatureInterior];
            set
            {
                if (value != _userEnvVars[Symbol.TemperatureInterior])
                {
                    _userEnvVars[Symbol.TemperatureInterior] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Te
        {
            get => _userEnvVars[Symbol.TemperatureExterior];
            set
            {
                if (value != _userEnvVars[Symbol.TemperatureExterior])
                {
                    _userEnvVars[Symbol.TemperatureExterior] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rsi
        {
            get => _userEnvVars[Symbol.TransferResistanceSurfaceInterior];
            set
            {
                if (value != _userEnvVars[Symbol.TransferResistanceSurfaceInterior])
                {
                    _userEnvVars[Symbol.TransferResistanceSurfaceInterior] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rse
        {
            get => _userEnvVars[Symbol.TransferResistanceSurfaceExterior];
            set
            {
                if (value != _userEnvVars[Symbol.TransferResistanceSurfaceExterior])
                {
                    _userEnvVars[Symbol.TransferResistanceSurfaceExterior] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rel_Fi
        {
            get => _userEnvVars[Symbol.RelativeHumidityInterior];
            set
            {
                if (value != _userEnvVars[Symbol.RelativeHumidityInterior])
                {
                    _userEnvVars[Symbol.RelativeHumidityInterior] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
        public static double Rel_Fe
        {
            get => _userEnvVars[Symbol.RelativeHumidityExterior];
            set
            {
                if (value != _userEnvVars[Symbol.RelativeHumidityExterior])
                {
                    _userEnvVars[Symbol.RelativeHumidityExterior] = value;
                    OnEnvVarsChanged(); // raises an event
                }
            }
        }
    }
}
