using BauphysikToolWPF.Services.Application;
using System;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{
    /// <summary>
    /// Thermal boundary conditions needed for calculating thermal values of elements.
    /// </summary>
    public class ThermalValuesCalcConfig
    {
        public double Ti
        {
            get => _userEnvVars[Symbol.TemperatureInterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TemperatureInterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TemperatureInterior] = value;
                    Session.OnEnvVarsChanged(Symbol.TemperatureInterior);
                }
            }
        }
        public double Te
        {
            get => _userEnvVars[Symbol.TemperatureExterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TemperatureExterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TemperatureExterior] = value;
                    Session.OnEnvVarsChanged(Symbol.TemperatureExterior);
                }
            }
        }
        public double Rsi
        {
            get => _userEnvVars[Symbol.TransferResistanceSurfaceInterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TransferResistanceSurfaceInterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TransferResistanceSurfaceInterior] = value;
                    Session.OnEnvVarsChanged(Symbol.TransferResistanceSurfaceInterior);
                }
            }
        }
        public double Rse
        {
            get => _userEnvVars[Symbol.TransferResistanceSurfaceExterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.TransferResistanceSurfaceExterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.TransferResistanceSurfaceExterior] = value;
                    Session.OnEnvVarsChanged(Symbol.TransferResistanceSurfaceExterior);
                }
            }
        }
        public double RelFi
        {
            get => _userEnvVars[Symbol.RelativeHumidityInterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.RelativeHumidityInterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.RelativeHumidityInterior] = value;
                    Session.OnEnvVarsChanged(Symbol.RelativeHumidityInterior);
                }
            }
        }
        public double RelFe
        {
            get => _userEnvVars[Symbol.RelativeHumidityExterior];
            set
            {
                if (Math.Abs(value - _userEnvVars[Symbol.RelativeHumidityExterior]) > 1e-03)
                {
                    _userEnvVars[Symbol.RelativeHumidityExterior] = value;
                    Session.OnEnvVarsChanged(Symbol.RelativeHumidityExterior);
                }
            }
        }

        public ThermalValuesCalcConfig Copy()
        {
            return new ThermalValuesCalcConfig
            {
                Ti = this.Ti,
                Te = this.Te,
                Rsi = this.Rsi,
                Rse = this.Rse,
                RelFi = this.RelFi,
                RelFe = this.RelFe
            };
        }

        // Initialize with default values from the database
        private readonly Dictionary<Symbol, double> _userEnvVars = new Dictionary<Symbol, double>(6)
        {
            {Symbol.TemperatureInterior, DatabaseManager.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior)[0].Value },
            {Symbol.TemperatureExterior, DatabaseManager.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior)[0].Value },
            {Symbol.TransferResistanceSurfaceInterior, DatabaseManager.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior)[0].Value },
            {Symbol.TransferResistanceSurfaceExterior, DatabaseManager.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior)[0].Value },
            {Symbol.RelativeHumidityInterior, DatabaseManager.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior)[0].Value },
            {Symbol.RelativeHumidityExterior, DatabaseManager.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior)[0].Value },
        };
    }
}
