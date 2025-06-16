using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Services.UI;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Services.Application
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)

    public delegate void NotifyPageChanged(NavigationPage targetPage, NavigationPage? originPage = null);
    
    public static class Session // publisher of 'EnvVarsChanged' event
    {
        #region EventHandlers

        // The subscriber class must register to these event and handle it with the method whose signature matches Notify delegate
        public static event Notify? SelectedProjectChanged; // event
        public static event Notify? NewProjectAdded; // event
        public static event Notify? SelectedElementChanged;
        public static event Notify? NewElementAdded;
        public static event Notify? ElementRemoved;
        public static event Notify? SelectedLayerChanged;
        public static event Notify? EnvVarsChanged;
        public static event Notify? EnvelopeItemsChanged;


        public static event NotifyPageChanged? PageChanged;

        // event handlers - publisher
        public static void OnSelectedProjectChanged(bool updateIsModified = true) //protected virtual method
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedProjectChanged?.Invoke(); //if SelectedProjectChanged is not null then call delegate
        }
        public static void OnNewProjectAdded(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            NewProjectAdded?.Invoke();
        }
        public static void OnSelectedElementChanged(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedElementChanged?.Invoke();
        }
        public static void OnNewElementAdded(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            NewElementAdded?.Invoke();
        }
        public static void OnElementRemoved(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            ElementRemoved?.Invoke();
        }
        public static void OnSelectedLayerChanged(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedLayerChanged?.Invoke();
        }
        public static void OnEnvVarsChanged()
        {
            EnvVarsChanged?.Invoke();
        }
        public static void OnEnvelopeItemsChanged(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            EnvelopeItemsChanged?.Invoke();
        }
        public static void OnPageChanged(NavigationPage targetPage, NavigationPage? originPage = null)
        {
            if (SelectedProject == null) return;
            PageChanged?.Invoke(targetPage, originPage);
        }

        #endregion

        public static string ProjectFilePath { get; set; } = string.Empty;

        public static Project? SelectedProject { get; set; }

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        public static int SelectedElementId { get; set; } = -1;

        /// <summary>
        /// Zeigt auf das entsprechende Element aus dem aktuellen Projekt auf Basis der InternalID von 'SelectedElementId'
        /// </summary>
        public static Element? SelectedElement => SelectedProject?.Elements.FirstOrDefault(e => e?.InternalId == SelectedElementId, null);

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        public static int SelectedLayerId { get; set; } = -1;

        /// <summary>
        /// Zeigt auf den entsprechenden Layer aus dem aktuellen Element auf Basis der LayerPosition von 'SelectedLayerPosition'
        /// </summary>
        public static Layer? SelectedLayer => SelectedElement?.Layers.FirstOrDefault(e => e?.InternalId == SelectedLayerId, null);

        public static ThermalValuesCalcConfig ThermalValuesCalcConfig => new ThermalValuesCalcConfig()
        {
            Ti = Ti,
            Te = Te,
            Rsi = Rsi,
            Rse = Rse,
            RelFi = RelFi,
            RelFe = RelFe,
        };

        public static EnvelopeCalculationConfig EnvelopeCalcConfig => new EnvelopeCalculationConfig()
        {
            BuildingUsageType = SelectedProject?.BuildingUsage ?? BuildingUsageType.Residential,
            BuildingTypeResidatial = SelectedProject?.BuildingTypeResidatial ?? BuildingTypeResidatial.EFH,
            // TODO: ... Add other properties as needed
        };

        public static CheckRequirementsConfig CheckRequirementsConfig => new CheckRequirementsConfig()
        {
            //BuildingAge = SelectedProject?.BuildingAge ?? BuildingAgeType.New,
            //BuildingUsage = SelectedProject?.BuildingUsage ?? BuildingUsageType.Residential,
            Ti = Ti,
            Te = Te,
        };
        
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
        public static double RelFi
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
        public static double RelFe
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

        #region private 

        // Initialize with default values from the database
        private static readonly Dictionary<Symbol, double> _userEnvVars = new Dictionary<Symbol, double>(6)
        {
            {Symbol.TemperatureInterior, DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior)[0].Value },
            {Symbol.TemperatureExterior, DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior)[0].Value },
            {Symbol.TransferResistanceSurfaceInterior, DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior)[0].Value },
            {Symbol.TransferResistanceSurfaceExterior, DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior)[0].Value },
            {Symbol.RelativeHumidityInterior, DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior)[0].Value },
            {Symbol.RelativeHumidityExterior, DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior)[0].Value },
        };

        #endregion
    }
}
