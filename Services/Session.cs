﻿using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.UI.Models;
using BT.Logging;

namespace BauphysikToolWPF.Services
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)
    public static class Session // publisher of 'EnvVarsChanged' event
    {
        // The subscriber class must register to these event and handle it with the method whose signature matches Notify delegate
        public static event Notify? SelectedProjectChanged; // event
        public static event Notify? NewProjectAdded; // event
        public static event Notify? SelectedElementChanged;
        public static event Notify? NewElementAdded;
        public static event Notify? ElementRemoved;
        public static event Notify? SelectedLayerChanged;
        public static event Notify? EnvVarsChanged;

        // event handlers - publisher
        public static void OnSelectedProjectChanged(bool updateIsModified = true) //protected virtual method
        {
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedProjectChanged?.Invoke(); //if SelectedProjectChanged is not null then call delegate
        }
        public static void OnNewProjectAdded(bool updateIsModified = true)
        {
            if (updateIsModified) SelectedProject.IsModified = true;
            NewProjectAdded?.Invoke();
        }
        public static void OnSelectedElementChanged(bool updateIsModified = true)
        {
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedElementChanged?.Invoke();
        }
        public static void OnNewElementAdded(bool updateIsModified = true)
        {
            if (updateIsModified) SelectedProject.IsModified = true;
            NewElementAdded?.Invoke();
        }
        public static void OnElementRemoved(bool updateIsModified = true)
        {
            if (updateIsModified) SelectedProject.IsModified = true;
            ElementRemoved?.Invoke();
        }
        public static void OnSelectedLayerChanged(bool updateIsModified = true)
        {
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedLayerChanged?.Invoke();
        }
        public static void OnEnvVarsChanged()
        {
            EnvVarsChanged?.Invoke();
        }

        public static string ProjectFilePath { get; set; } = string.Empty;

        public static Project SelectedProject = new Project();

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


        // Use GlaserCalc as Collection for Results due to Polymorphism;
        // You can use GlaserCalc objects wherever ThermalValuesCalc and TemperatureCurveCalc objects are expected.
        private static ThermalValuesCalc _calcResults = new ThermalValuesCalc();
        public static ThermalValuesCalc CalcResults
        {
            get
            {
                if (Recalculate)
                {
                    _calcResults = new ThermalValuesCalc(SelectedElement, Rsi, Rse, Ti, Te);
                    Recalculate = false;
                }
                return _calcResults;
            }
        }

        private static bool _recalculate = true;
        /// <summary>
        /// Recalculate Flag only gets set by LayerSetup Page: All Changes to the Layers and EnvVars,
        /// which would require a re-calculation, are made there.
        /// </summary>
        public static bool Recalculate
        {
            get => _recalculate;
            set
            {
                if (value == _recalculate) return;
                _recalculate = value;
                Logger.LogInfo($"Set Recalculate flag to: {value}");
            }
        }

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
                if (Math.Abs(value - _userEnvVars[Symbol.TemperatureInterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TemperatureInterior] = value;
                    OnEnvVarsChanged();
                }
            }
        }
        public static double Te
        {
            get => _userEnvVars[Symbol.TemperatureExterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TemperatureExterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TemperatureExterior] = value;
                    OnEnvVarsChanged();
                }
            }
        }
        public static double Rsi
        {
            get => _userEnvVars[Symbol.TransferResistanceSurfaceInterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TransferResistanceSurfaceInterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TransferResistanceSurfaceInterior] = value;
                    OnEnvVarsChanged();
                }
            }
        }
        public static double Rse
        {
            get => _userEnvVars[Symbol.TransferResistanceSurfaceExterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TransferResistanceSurfaceExterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TransferResistanceSurfaceExterior] = value;
                    OnEnvVarsChanged();
                }
            }
        }
        public static double Rel_Fi
        {
            get => _userEnvVars[Symbol.RelativeHumidityInterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.RelativeHumidityInterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.RelativeHumidityInterior] = value;
                    OnEnvVarsChanged();
                }
            }
        }
        public static double Rel_Fe
        {
            get => _userEnvVars[Symbol.RelativeHumidityExterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.RelativeHumidityExterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.RelativeHumidityExterior] = value;
                    OnEnvVarsChanged();
                }
            }
        }
    }
}
