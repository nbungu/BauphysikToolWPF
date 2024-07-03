using System;
using System.Collections.Generic;
using SQLite;

namespace BauphysikToolWPF.Models.Helper
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
        VapourDiffusionResistance, // 
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

    /*
     * To ensure that changing PropertyItem.Value reflects on the corresponding SelectedLayer,
     * you can use a delegate to dynamically get and set the Thickness property of SelectedLayer
     *
     *
     * usage with getter and setter:
     *
     * var thicknessProperty = new PropertyItem<double>(Symbol.Thickness, () => SelectedLayer.Thickness, value => SelectedLayer.Thickness = value);
     *
     * usage with getter only:
     *
     * var thicknessProperty = new PropertyItem<double>(Symbol.Thickness, () => SelectedLayer.Thickness);
     */


    public class PropertyItem<T> : IPropertyItem
    {
        //------Events-----//

        public static event Action? PropertyChanged; // event

        public static void OnPropertyChanged() //protected virtual method
        {
            PropertyChanged?.Invoke(); // if PropertyChanged is not null then call delegate
        }

        //------Variablen-----//

        // _getter and _setter are delegates used to get and set the Value.
        private Func<T>? _getter;
        private Action<T>? _setter;
        private T _value;

        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public object Value
        {
            get => _getter != null ? (object)_getter() : _value;
            set
            {
                if (_setter != null)
                {
                    _setter((T)value);
                }
                else
                {
                    _value = (T)value;
                }
            }
        }
        [NotNull]
        public string Name { get; set; }
        [NotNull]
        public Symbol Symbol { get; set; }
        [NotNull]
        public Unit Unit { get; set; }
        public string Comment { get; set; }

        //------Not part of the Database-----//
        public bool IsReadonly { get; set; } = true;
        public object[] PropertyValues { get; set; } = Array.Empty<object>();
        public string SymbolBaseText { get; set; } = string.Empty;
        public string SymbolSubscriptText { get; set; } = string.Empty;
        public string UnitCounterText { get; set; } = string.Empty;
        public string UnitDenominatorText { get; set; } = string.Empty;

        //------Konstruktor-----//

        public PropertyItem() { }

        public PropertyItem(string name, Func<T> getter, Action<T> setter)
        {
            Name = name;
            IsReadonly = false;
            _getter = getter;
            _setter = setter;
        }

        public PropertyItem(Symbol symbol, Func<T> getter, Action<T> setter)
        {
            SetPropertiesBySymbol(symbol);
            IsReadonly = false;
            _getter = getter;
            _setter = setter;
        }
        public PropertyItem(string name, Func<T> getter)
        {
            Name = name;
            IsReadonly = true;
            _getter = getter;
        }

        public PropertyItem(Symbol symbol, Func<T> getter)
        {
            SetPropertiesBySymbol(symbol);
            IsReadonly = true;
            _getter = getter;
        }

        //------Methoden-----//

        private void SetPropertiesBySymbol(Symbol symbol)
        {
            // Define the mapping of Symbol to properties
            var symbolMappings = new Dictionary<Symbol, (Unit unit, string baseText, string subscriptText, string counterText, string denominatorText, string name, string comment)>
            {
                { Symbol.None, (Unit.None, "", "", "", "", "", "") },
                { Symbol.Length, (Unit.Centimeter, "l", "", "cm", "", "Länge", "") },
                { Symbol.Distance, (Unit.Centimeter, "a", "", "cm", "", "Abstand", "") },
                { Symbol.Area, (Unit.SquareMeter, "A", "", "m²", "", "Fläche", "") },
                { Symbol.Volume, (Unit.CubicMeter, "V", "", "m³", "", "Volumen", "") },
                { Symbol.Thickness, (Unit.Centimeter, "d", "", "cm", "", "Dicke", "") },
                { Symbol.Width, (Unit.Centimeter, "b", "", "cm", "", "Breite", "") },
                { Symbol.Height, (Unit.Centimeter, "h", "", "cm", "", "Höhe", "") },
                { Symbol.TemperatureInterior, (Unit.Celsius, "θ", "i", "°C", "", "Raumtemperatur", "") },
                { Symbol.TemperatureExterior, (Unit.Celsius, "θ", "e", "°C", "", "Außentemperatur", "") },
                { Symbol.TemperatureSurfaceInterior, (Unit.Celsius, "θ", "si", "°C", "", "Oberflächentemperatur (innen)", "") },
                { Symbol.TemperatureSurfaceExterior, (Unit.Celsius, "θ", "se", "°C", "", "Oberflächentemperatur (außen)", "") },
                { Symbol.RValueLayer, (Unit.SquareMeterKelvinPerWatt, "R", "i", "m²K", "W", "R-Wert", "Wärmedurchlasswiderstand") },
                { Symbol.RValueElement, (Unit.SquareMeterKelvinPerWatt, "R", "ges", "m²K", "W", "R-Wert", "Wärmedurchlasswiderstand") },
                { Symbol.RValueTotal, (Unit.SquareMeterKelvinPerWatt, "R", "T", "m²K", "W", "R-Wert", "Wärmedurchlasswiderstand") },
                { Symbol.TransferResistanceSurfaceInterior, (Unit.SquareMeterKelvinPerWatt, "R", "si", "m²K", "W", "Wärmeübergangswiderstand (innen)", "") },
                { Symbol.TransferResistanceSurfaceExterior, (Unit.SquareMeterKelvinPerWatt, "R", "se", "m²K", "W", "Wärmeübergangswiderstand (außen)", "") },
                { Symbol.UValue, (Unit.WattsPerSquareMeterKelvin, "U", "", "W", "m²K", "Wärmedurchgangskoeffizient", "U-Wert") },
                { Symbol.RawDensity, (Unit.KilogramPerCubicMeter, "ρ", "", "kg", "m³", "Rohdichte", "") },
                { Symbol.AreaMassDensity, (Unit.KilogramPerSquareMeter, "m'", "", "kg", "m²", "Flächenbezogene Masse", "") },
                { Symbol.SdThickness, (Unit.KilogramPerSquareMeter, "s", "d", "m", "", "sd-Wert", "Wasserdampfdiffusionsäquivalente Luftschichtdicke") },
                { Symbol.RelativeHumidity, (Unit.Percent, "φ", "", "%", "", "Rel. Luftfeuchtigkeit", "") },
                { Symbol.RelativeHumidityInterior, (Unit.Percent, "φ", "i", "%", "", "Rel. Luftfeuchtigkeit (innen)", "") },
                { Symbol.RelativeHumidityExterior, (Unit.Percent, "φ", "e", "%", "", "Rel. Luftfeuchtigkeit (außen)", "") },
                { Symbol.VapourDiffusionResistance, (Unit.None, "µ", "", "-", "", "µ-Wert", "Wasserdampfdiffusionswiderstandszahl (trocken)") },
                { Symbol.ThermalConductivity, (Unit.WattsPerMeterKelvin, "λ", "", "W", "mK", "Wärmeleitfähigkeit", "Bemessungswert der Wärmeleitfähigkeit (WLF)") },
                { Symbol.SpecificHeatCapacity, (Unit.JoulesPerKilogramKelvin, "c", "", "J", "kgK", "spezifische Wärmekapazität", "") },
            };

            // Get the corresponding properties for the given symbol
            if (symbolMappings.TryGetValue(symbol, out var properties))
            {
                Name = properties.name;
                Symbol = symbol;
                Unit = properties.unit;
                SymbolBaseText = properties.baseText;
                SymbolSubscriptText = properties.subscriptText;
                UnitCounterText = properties.counterText;
                UnitDenominatorText = properties.denominatorText;
                Comment = properties.comment;
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
