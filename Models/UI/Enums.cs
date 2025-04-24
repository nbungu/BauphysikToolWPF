using System.Collections.Generic;

namespace BauphysikToolWPF.Models.UI
{
    public class Enums
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
            TemperatureInterior,                // θ_i
            TemperatureExterior,                // θ_e
            TemperatureSurfaceInterior,         // θ_si
            TemperatureSurfaceExterior,         // θ_se

            // R-Values
            RValueLayer,                        // R_i
            RValueElement,                      // R_ges
            RValueTotal,                        // R_Total (incl. R_si+R_se)
            TransferResistanceSurfaceInterior,  // R_si
            TransferResistanceSurfaceExterior,  // R_se
            UValue,                             // U

            // Other
            RawDensity,
            AreaMassDensity,                    // m'
            SdThickness,
            VapourDiffusionResistance,          // µ
            RelativeHumidity,
            RelativeHumidityInterior,           // RelFi
            RelativeHumidityExterior,           // RelFe

            ThermalConductivity,                // λ
            SpecificHeatCapacity,               // c
            VolumetricHeatCapacity,             // C_V
            ArealHeatCapacity,                  // C_i
            HeatFluxDensity,                    // q
            FRsi,

            // Dynamic Thermal Values
            UValueDynamic,
            RValueDynamic,
            TemperatureAmplitudeDamping,
            TemperatureAmplitudeRatio,
            TimeShift,
            EffectiveThermalMass,
            DecrementFactor,
            ThermalAdmittance,
            DynamicThermalAdmittance,
        }
        public static readonly Dictionary<Symbol, (Unit unit, string baseText, string subscriptText, string counterText, string denominatorText, string name, string comment)> SymbolMapping = new()
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
                { Symbol.TemperatureSurfaceInterior, (Unit.Celsius, "θ", "si", "°C", "", "Oberflächentemperatur, Innen", "") },
                { Symbol.TemperatureSurfaceExterior, (Unit.Celsius, "θ", "se", "°C", "", "Oberflächentemperatur, Außen", "") },
                { Symbol.RValueLayer, (Unit.SquareMeterKelvinPerWatt, "R", "i", "m²K", "W", "R-Wert", "Wärmedurchlasswiderstand") },
                { Symbol.RValueElement, (Unit.SquareMeterKelvinPerWatt, "R", "ges", "m²K", "W", "R-Wert Bauteil", "Wärmedurchlasswiderstand") },
                { Symbol.RValueTotal, (Unit.SquareMeterKelvinPerWatt, "R", "T", "m²K", "W", "R-Total", "Wärmedurchlasswiderstand") },
                { Symbol.TransferResistanceSurfaceInterior, (Unit.SquareMeterKelvinPerWatt, "R", "si", "m²K", "W", "Wärmeübergangswiderstand, Innen", "") },
                { Symbol.TransferResistanceSurfaceExterior, (Unit.SquareMeterKelvinPerWatt, "R", "se", "m²K", "W", "Wärmeübergangswiderstand, Außen", "") },
                { Symbol.UValue, (Unit.WattsPerSquareMeterKelvin, "U", "", "W", "m²K", "Wärmedurchgangskoeffizient", "U-Wert") },
                { Symbol.RawDensity, (Unit.KilogramPerCubicMeter, "ρ", "", "kg", "m³", "Rohdichte", "") },
                { Symbol.AreaMassDensity, (Unit.KilogramPerSquareMeter, "m'", "", "kg", "m²", "Flächenbez. Masse", "") },
                { Symbol.SdThickness, (Unit.KilogramPerSquareMeter, "s", "d", "m", "", "sd-Wert", "Wasserdampfdiffusionsäquivalente Luftschichtdicke") },
                { Symbol.RelativeHumidity, (Unit.Percent, "φ", "", "%", "", "Rel. Luftfeuchtigkeit", "") },
                { Symbol.RelativeHumidityInterior, (Unit.Percent, "φ", "i", "%", "", "Rel. Luftfeuchtigkeit, Innen", "") },
                { Symbol.RelativeHumidityExterior, (Unit.Percent, "φ", "e", "%", "", "Rel. Luftfeuchtigkeit, Außen", "") },
                { Symbol.VapourDiffusionResistance, (Unit.None, "µ", "", "-", "", "µ-Wert", "Wasserdampfdiffusionswiderstandszahl (trocken)") },
                { Symbol.ThermalConductivity, (Unit.WattsPerMeterKelvin, "λ", "", "W", "mK", "Wärmeleitfähigkeit", "Bemessungswert der Wärmeleitfähigkeit (WLF)") },
                { Symbol.SpecificHeatCapacity, (Unit.JoulesPerKilogramKelvin, "c", "", "J", "kgK", "Spezifische Wärmekapazität", "") },
                { Symbol.VolumetricHeatCapacity, (Unit.KilojoulesPerCubicMeterKelvin, "C", "V", "kJ", "m³K", "Volumenbez. Wärmekapazität", "") },
                { Symbol.ArealHeatCapacity, (Unit.KilojoulesPerCubicMeterKelvin, "C", "", "kJ", "m²K", "Flächenbez. Wärmekapazität", "Quotient aus Wärmekapazität und Bauteilfläche") },
                { Symbol.HeatFluxDensity, (Unit.WattsPerSquareMeter, "q", "", "W", "m²", "Wärmestromdichte", "") },
                { Symbol.UValueDynamic, (Unit.WattsPerSquareMeterKelvin, "U", "dyn", "W", "m²K", "U-dynamisch", "dynamischer Wärmedurchgangskoeffizient") },
                { Symbol.RValueDynamic, (Unit.SquareMeterKelvinPerWatt, "R", "dyn", "m²K", "W", "R-dynamisch", "dynamischer Wärmedurchlasswiderstand") },
                { Symbol.TemperatureAmplitudeDamping, (Unit.None, "υ", "", "", "", "Temperaturamplitudendämpfung", "") },
                { Symbol.TemperatureAmplitudeRatio, (Unit.None, "", "", "", "", "Temperaturamplitudenverhältnis", "Kehrwert der TAD: multipliziert mit 100 enpricht es dem %-Wert der Wärmeamplitude welche innen noch ankommt, aufgrund einer Schwankung außen.") },
                { Symbol.TimeShift, (Unit.Hour, "Δt", "", "h", "", "Phasenverschiebung", "Zeitvorsprung (wenn positiv) oder Verzögerung (wenn\r\nnegativ)") },
                { Symbol.EffectiveThermalMass, (Unit.KilogramPerSquareMeter, "M", "", "kg", "m²", "Speicherwirksame Masse", "") },
                { Symbol.DecrementFactor, (Unit.None, "f", "", "", "", "Dekrement", "Abminderungsfaktor: Verhältnis des Betrags der dynamischen Wärmeaufnahme zum Wärmedurchgangskoeffizienten U unter\r\nstationären Bedingungen") },
                { Symbol.ThermalAdmittance, (Unit.WattsPerSquareMeterKelvin, "Y", "mm", "W", "m²K", "Wärmeaufnahme", "komplexe Größe, festgelegt als komplexe Amplitude der Wärmestromdichte durch die an Zone m des\r\nBauteils angrenzende Oberfläche, geteilt durch die komplexe Temperaturamplitude in der gleichen Zone,\r\nwenn die Temperatur auf der anderen Seite konstant gehalten wird") },
                { Symbol.DynamicThermalAdmittance, (Unit.WattsPerSquareMeterKelvin, "Y", "mn", "W", "m²K", "dynamische Wärmeaufnahme", "komplexe Größe, festgelegt als komplexe Amplitude der Wärmestromdichte durch die an Zone m des Bau-\r\nteils angrenzende Oberfläche geteilt durch die komplexe Temperaturamplitude in der Zone n, wenn die\r\nTemperatur in Zone m konstant gehalten wird") },
            };

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
            WattsPerSquareMeter,           // W/m² - Heat Flux Density
            KilojoulesPerSquareMeterKelvin,// kJ/(m²·K) - Heat Storage Capacity
            SquareMeterKelvinPerWatt,      // m²·K/W - Thermal Resistance (R-value)
            KilojoulesPerCubicMeterKelvin, // kJ/(m³·K) - Volumetric Heat Capacity

            // Other
            Percent                 // %
        }
    }
}
