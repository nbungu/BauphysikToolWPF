using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BauphysikToolWPF.Models
{
    public enum Symbol
    {
        None,

        // Geometry
        Length,                             // l
        Distance,                           // a
        Area,                               // A
        Volume,                             // V
        Thickness,                          // d
        Width,                              // b
        Height,                             // h

        // Temperatures
        TemperatureInterior,
        TemperatureExterior,
        TemperatureSurfaceInterior,
        TemperatureSurfaceExterior,

        // R-Values
        RValueLayer,                      // Ri
        RValueElement,                      // R_ges
        RValueTotal,                        // R_Total
        TransferResistanceSurfaceInterior,  // R_si
        TransferResistanceSurfaceExterior,  // R_se
        UValue,

        // Other
        RawDensity,
        AreaMassDensity,                    // m'
        SdThickness,                        
        VapourResistance, // 
        RelativeHumidity,
        RelativeHumidityInterior,
        RelativeHumidityExterior,

        ThermalConductivity, // lambda
        SpecificHeatCapacity // c
    }

    public enum Unit
    {
        None,                   // -

        // Temperature
        Celsius,                // °C
        Kelvin,                 // K

        // Density
        KilogramPerCubicMeter, // kg/m³
        KilogramPerSquareMeter, // kg/m²
        GramPerCubicMeter,     // g/m³
        MilligramPerCubicMeter,// mg/m³

        // Volume Flow
        CubicMeterPerSecond,   // m³/s
        CubicMeterPerHour,     // m³/h
        CubicMeterPerDay,      // m³/day

        // Pressure
        Pascal,                // Pa
        Bar,                   // bar
        Atmosphere,            // atm

        // Length
        Meter,                 // m
        Centimeter,            // cm
        Millimeter,            // mm
        Inch,                  // in
        Foot,                  // ft

        // Area
        SquareMeter,           // m²
        SquareCentimeter,      // cm²
        SquareMillimeter,      // mm²
        SquareInch,            // in²
        SquareFoot,            // ft²

        // Volume
        CubicMeter,            // m³
        CubicCentimeter,       // cm³
        CubicMillimeter,       // mm³
        CubicInch,             // in³
        CubicFoot,             // ft³
        Liter,                 // L
        Milliliter,            // mL
        Gallon,                // gal
        Quart,                 // qt
        Pint,                  // pt
        Cup,                   // cup
        Ounce,                 // oz

        // Mass
        Kilogram,              // kg
        Gram,                  // g
        Milligram,             // mg
        Ton,                   // t
        Pound,                 // lb

        // Energy
        KilowattHour,          // kWh
        Joule,                 // J
        Calorie,               // cal

        // Power
        Watt,                  // W
        Kilowatt,              // kW
        Horsepower,            // hp

        // Force
        Newton,                // N
        Kilonewton,            // kN
        Megapascal,            // MPa
        Gigapascal,            // GPa

        // Time
        Second,                // s
        Minute,                // min
        Hour,                  // h
        Day,                   // day
        Week,                  // week
        Month,                 // month
        Year,                  // year

        // Building Physics
        JoulesPerKilogramKelvin,       // J/(kg·K) - Specific Heat Capacity
        WattsPerMeterKelvin,           // W/(m·K) - Thermal Conductivity
        WattsPerSquareMeterKelvin,     // W/(m²·K) - Thermal Transmittance (U-value)
        WattsPerSquareMeter,           // W/m² - Heat Flux
        KilojoulesPerSquareMeterKelvin,// kJ/(m²·K) - Heat Storage Capacity
        SquareMeterKelvinPerWatt,      // m²·K/W - Thermal Resistance (R-value)
        WattsPerCubicMeterKelvin,      // W/(m³·K) - Volumetric Heat Capacity

        // Other
        Percent                 // %
    }

    public class PropertyItem
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public Symbol Symbol { get; set; }
        [NotNull]
        public object Value { get; set; } = 0.0;
        [NotNull]
        public Unit Unit { get; set; }
        public object Comment { get; set; } = "";

        //------Not part of the Database-----//
        public bool IsReadonly { get; set; } = true;
        
        public object[] PropertyValues { get; set; } = Array.Empty<object>();

        // Properties for base and subscript text
        public string SymbolBaseText { get; set; } = string.Empty;
        public string SymbolSubscriptText { get; set; } = string.Empty;

        // Properties for fraction label of Units
        public string UnitCounterText { get; set; } = string.Empty;
        public string UnitDenominatorText { get; set; } = string.Empty;

        //------Konstruktor-----//

        public PropertyItem() {}

        public PropertyItem(string name, object value)
        {
            Name = name;
            Value = value.ToString() ?? string.Empty;
        }
        public PropertyItem(string name, object value, object[] values)
        {
            Name = name;
            Value = value.ToString() ?? string.Empty;
            PropertyValues = values;
        }

        public PropertyItem(Symbol symbol, object value)
        {
            SetPropertiesBySymbol(symbol);
            Value = value.ToString() ?? string.Empty;
        }

        //------Methoden-----//

        private void SetPropertiesBySymbol(Symbol symbol)
        {
            // Define the mapping of Symbol to properties
            var symbolMappings = new Dictionary<Symbol, (Unit unit, string baseText, string subscriptText, string counterText, string denominatorText, string name)>
            {
                { Symbol.None, (Unit.None, "", "", "", "", "") },
                { Symbol.Length, (Unit.Centimeter, "l", "", "cm", "", "Länge") },
                { Symbol.Distance, (Unit.Centimeter, "a", "", "cm", "", "Abstand") },
                { Symbol.Area, (Unit.SquareMeter, "A", "", "m²", "", "Fläche") },
                { Symbol.Volume, (Unit.CubicMeter, "V", "", "m³", "", "Volumen") },
                { Symbol.Thickness, (Unit.Centimeter, "d", "", "cm", "", "Dicke") },
                { Symbol.Width, (Unit.Centimeter, "b", "", "cm", "", "Breite") },
                { Symbol.Height, (Unit.Centimeter, "h", "", "cm", "", "Höhe") },
                { Symbol.TemperatureInterior, (Unit.Celsius, "θ", "i", "°C", "", "Raumtemperatur") },
                { Symbol.TemperatureExterior, (Unit.Celsius, "θ", "e", "°C", "", "Außentemperatur") },
                { Symbol.TemperatureSurfaceInterior, (Unit.Celsius, "θ", "si", "°C", "", "Raumtemperatur") },
                { Symbol.TemperatureSurfaceExterior, (Unit.Celsius, "θ", "se", "°C", "", "Außentemperatur") },
                { Symbol.RValueLayer, (Unit.SquareMeterKelvinPerWatt, "R", "i", "m²K", "W", "Wärmedurchlasswiderstand") },
                { Symbol.RValueElement, (Unit.SquareMeterKelvinPerWatt, "R", "ges", "m²K", "W", "Wärmedurchlasswiderstand") },
                { Symbol.RValueTotal, (Unit.SquareMeterKelvinPerWatt, "R", "T", "m²K", "W", "Wärmedurchlasswiderstand") },
                { Symbol.TransferResistanceSurfaceInterior, (Unit.SquareMeterKelvinPerWatt, "R", "si", "m²K", "W", "Wärmeübergangswiderstand (innen)") },
                { Symbol.TransferResistanceSurfaceExterior, (Unit.SquareMeterKelvinPerWatt, "R", "se", "m²K", "W", "Wärmeübergangswiderstand (außen)") },
                { Symbol.UValue, (Unit.WattsPerSquareMeterKelvin, "U", "", "W", "m²K", "Wärmedurchgangskoeffizient") },
                { Symbol.RawDensity, (Unit.KilogramPerCubicMeter, "ρ", "", "kg", "m³", "Rohdichte") },
                { Symbol.AreaMassDensity, (Unit.KilogramPerSquareMeter, "m'", "", "kg", "m²", "Flächenbezogene Masse") },
                { Symbol.SdThickness, (Unit.KilogramPerSquareMeter, "s", "d", "m", "", "Wasserdampfdiffusionsäquivalente Luftschichtdicke") },
                { Symbol.RelativeHumidity, (Unit.Percent, "φ", "", "%", "", "Relative Luftfeuchtigkeit") },
                { Symbol.RelativeHumidityInterior, (Unit.Percent, "φ", "i", "%", "", "Relative Luftfeuchtigkeit (innen)") },
                { Symbol.RelativeHumidityExterior, (Unit.Percent, "φ", "e", "%", "", "Relative Luftfeuchtigkeit (außen)") },
                { Symbol.VapourResistance, (Unit.None, "µ", "", "-", "", "Wasserdampfdiffusionswiderstandszahl") },
                { Symbol.ThermalConductivity, (Unit.WattsPerMeterKelvin, "λ", "", "W", "mK", "Wärmeleitfähigkeit") },
                { Symbol.SpecificHeatCapacity, (Unit.JoulesPerKilogramKelvin, "c", "", "J", "kgK", "spezifische Wärmekapazität") },
            };

            // Get the corresponding properties for the given symbol
            if (symbolMappings.TryGetValue(symbol, out var properties))
            {
                Symbol = symbol;
                Unit = properties.unit;
                SymbolBaseText = properties.baseText;
                SymbolSubscriptText = properties.subscriptText;
                UnitCounterText = properties.counterText;
                UnitDenominatorText = properties.denominatorText;
                Name = properties.name;
            }
            else
            {
                throw new ArgumentException($"Unknown symbol: {symbol}", nameof(symbol));
            }
        }
    }
}
/*

 */
