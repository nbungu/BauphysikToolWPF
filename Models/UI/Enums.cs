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

            // DIN 18599
            PrimaryEnergy,                      // Q_p
            PrimaryEnergyPerArea,               // q_p
            AToVRatio,                          // A/V
            SpecificHeatTransmissionLoss,       // H_T'
            ThermalBridgeSurcharge,              // ΔU_WB
            AirExchangeRate,                    // n
        }

        public static readonly Dictionary<Symbol, (Unit unit, string baseText, string subscriptText, string name, string comment)> SymbolMapping = new()
        {
            { Symbol.None, (Unit.None, "", "", "", "") },
            { Symbol.Length, (Unit.Centimeter, "l", "", "Länge", "") },
            { Symbol.Distance, (Unit.Centimeter, "a", "", "Abstand", "") },
            { Symbol.Area, (Unit.SquareMeter, "A", "", "Fläche", "") },
            { Symbol.Volume, (Unit.CubicMeter, "V", "", "Volumen", "") },
            { Symbol.Thickness, (Unit.Centimeter, "d", "", "Dicke", "") },
            { Symbol.Width, (Unit.Centimeter, "b", "", "Breite", "") },
            { Symbol.Height, (Unit.Centimeter, "h", "", "Höhe", "") },
            { Symbol.TemperatureInterior, (Unit.Celsius, "θ", "i", "Raumtemperatur", "") },
            { Symbol.TemperatureExterior, (Unit.Celsius, "θ", "e", "Außentemperatur", "") },
            { Symbol.TemperatureSurfaceInterior, (Unit.Celsius, "θ", "si", "Oberflächentemperatur, Innen", "") },
            { Symbol.TemperatureSurfaceExterior, (Unit.Celsius, "θ", "se", "Oberflächentemperatur, Außen", "") },
            { Symbol.RValueLayer, (Unit.SquareMeterKelvinPerWatt, "R", "i", "R-Wert", "Wärmedurchlasswiderstand") },
            { Symbol.RValueElement, (Unit.SquareMeterKelvinPerWatt, "R", "ges", "R-Wert Bauteil", "Wärmedurchlasswiderstand") },
            { Symbol.RValueTotal, (Unit.SquareMeterKelvinPerWatt, "R", "T", "R-Total", "Wärmedurchlasswiderstand") },
            { Symbol.TransferResistanceSurfaceInterior, (Unit.SquareMeterKelvinPerWatt, "R", "si", "Wärmeübergangswiderstand, Innen", "") },
            { Symbol.TransferResistanceSurfaceExterior, (Unit.SquareMeterKelvinPerWatt, "R", "se", "Wärmeübergangswiderstand, Außen", "") },
            { Symbol.UValue, (Unit.WattsPerSquareMeterKelvin, "U", "", "Wärmedurchgangskoeffizient", "U-Wert") },
            { Symbol.RawDensity, (Unit.KilogramPerCubicMeter, "ρ", "", "Rohdichte", "") },
            { Symbol.AreaMassDensity, (Unit.KilogramPerSquareMeter, "m'", "", "Flächenbez. Masse", "") },
            { Symbol.SdThickness, (Unit.KilogramPerSquareMeter, "s", "d", "sd-Wert", "Wasserdampfdiffusionsäquivalente Luftschichtdicke") },
            { Symbol.RelativeHumidity, (Unit.Percent, "φ", "", "Rel. Luftfeuchtigkeit", "") },
            { Symbol.RelativeHumidityInterior, (Unit.Percent, "φ", "i", "Rel. Luftfeuchtigkeit, Innen", "") },
            { Symbol.RelativeHumidityExterior, (Unit.Percent, "φ", "e", "Rel. Luftfeuchtigkeit, Außen", "") },
            { Symbol.VapourDiffusionResistance, (Unit.None, "µ", "", "µ-Wert", "Wasserdampfdiffusionswiderstandszahl (trocken)") },
            { Symbol.ThermalConductivity, (Unit.WattsPerMeterKelvin, "λ", "", "Wärmeleitfähigkeit", "Bemessungswert der Wärmeleitfähigkeit (WLF)") },
            { Symbol.SpecificHeatCapacity, (Unit.JoulesPerKilogramKelvin, "c", "", "Spezifische Wärmekapazität", "") },
            { Symbol.VolumetricHeatCapacity, (Unit.KilojoulesPerCubicMeterKelvin, "C", "V", "Volumenbez. Wärmekapazität", "") },
            { Symbol.ArealHeatCapacity, (Unit.KilojoulesPerSquareMeterKelvin, "C", "", "Flächenbez. Wärmekapazität", "Quotient aus Wärmekapazität und Bauteilfläche") },
            { Symbol.HeatFluxDensity, (Unit.WattsPerSquareMeter, "q", "", "Wärmestromdichte", "") },
            { Symbol.UValueDynamic, (Unit.WattsPerSquareMeterKelvin, "U", "dyn", "U-dynamisch", "dynamischer Wärmedurchgangskoeffizient") },
            { Symbol.RValueDynamic, (Unit.SquareMeterKelvinPerWatt, "R", "dyn", "R-dynamisch", "dynamischer Wärmedurchlasswiderstand") },
            { Symbol.TemperatureAmplitudeDamping, (Unit.None, "υ", "", "Temperaturamplitudendämpfung", "") },
            { Symbol.TemperatureAmplitudeRatio, (Unit.None, "", "", "Temperaturamplitudenverhältnis", "Kehrwert der TAD: multipliziert mit 100 enpricht es dem %-Wert der Wärmeamplitude welche innen noch ankommt, aufgrund einer Schwankung außen.") },
            { Symbol.TimeShift, (Unit.Hour, "Δt", "", "Phasenverschiebung", "Zeitvorsprung (wenn positiv) oder Verzögerung (wenn\r\nnegativ)") },
            { Symbol.EffectiveThermalMass, (Unit.KilogramPerSquareMeter, "M", "", "Speicherwirksame Masse", "") },
            { Symbol.DecrementFactor, (Unit.None, "f", "", "Dekrement", "Abminderungsfaktor: Verhältnis des Betrags der dynamischen Wärmeaufnahme zum Wärmedurchgangskoeffizienten U unter\r\nstationären Bedingungen") },
            { Symbol.ThermalAdmittance, (Unit.WattsPerSquareMeterKelvin, "Y", "mm", "Wärmeaufnahme", "komplexe Größe, festgelegt als komplexe Amplitude der Wärmestromdichte durch die an Zone m des\r\nBauteils angrenzende Oberfläche, geteilt durch die komplexe Temperaturamplitude in der gleichen Zone,\r\nwenn die Temperatur auf der anderen Seite konstant gehalten wird") },
            { Symbol.DynamicThermalAdmittance, (Unit.WattsPerSquareMeterKelvin, "Y", "mn", "dynamische Wärmeaufnahme", "komplexe Größe, festgelegt als komplexe Amplitude der Wärmestromdichte durch die an Zone m des Bau-\r\nteils angrenzende Oberfläche geteilt durch die komplexe Temperaturamplitude in der Zone n, wenn die\r\nTemperatur in Zone m konstant gehalten wird") },
            { Symbol.PrimaryEnergy, (Unit.KilowattHourPerYear, "Q", "p", "Primärenergie (absolut)", "Energiemenge, die zusätzlich zum Energieinhalt des notwendigen Brennstoffs und der Hilfs-\r\nenergien für die Anlagentechnik auch die Energiemengen einbezieht, die durch vorgelagerte Prozessketten\r\naußerhalb des Gebäudes bei der Gewinnung, Umwandlung und Verteilung der jeweils eingesetzten\r\nBrennstoffe bzw. Stoffe entstehen") },
            { Symbol.PrimaryEnergyPerArea, (Unit.KilowattHourPerAreaAndYear, "q", "p", "Primärenergie (flächenbezogen)", "") },
            { Symbol.AToVRatio, (Unit.PerMeter, "A", "V", "A zu V Verhältnis", "") },
            { Symbol.SpecificHeatTransmissionLoss, (Unit.WattsPerSquareMeterKelvin, "H", "T'", "spezifischer Transmissionswärmeverlust", "spezifischer Transmissionswärmetransferkoeffizient (Bezeichnung in DIN V 18599) bzw. den spezifischer Transmissionswärmeverlust (Bezeichnung in DIN V 4108-6). Auf die wärmeübertragende Hüllfläche bezogener Transmissionswärmeverlust") },
            { Symbol.ThermalBridgeSurcharge, (Unit.WattsPerSquareMeterKelvin, "ΔU", "WB", "Wärmebrückenzuschlag", "") },
            { Symbol.AirExchangeRate, (Unit.PerHour, "n", "", "Luftwechselzahl", "Luftvolumenstrom je Volumeneinheit") },
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
            KilowattHourPerYear,   // kWh/a
            KilowattHourPerAreaAndYear,   // kWh/(m²a)
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
            Percent,                 // %
            PerMeter,                // 1/m
            PerHour,                // 1/h   
        }
        public static readonly Dictionary<Unit, (string counterText, string denominatorText)> UnitDisplayMapping = new()
        {
            { Unit.None, ("-", "") },

            // Temperature
            { Unit.Celsius, ("°C", "") },
            { Unit.Kelvin, ("K", "") },

            // Density
            { Unit.KilogramPerCubicMeter, ("kg", "m³") },
            { Unit.KilogramPerSquareMeter, ("kg", "m²") },
            { Unit.GramPerCubicMeter, ("g", "m³") },
            { Unit.MilligramPerCubicMeter, ("mg", "m³") },

            // Volume Flow
            { Unit.CubicMeterPerSecond, ("m³", "s") },
            { Unit.CubicMeterPerHour, ("m³", "h") },
            { Unit.CubicMeterPerDay, ("m³", "d") },

            // Pressure
            { Unit.Pascal, ("Pa", "") },
            { Unit.Bar, ("bar", "") },
            { Unit.Atmosphere, ("atm", "") },

            // Length
            { Unit.Meter, ("m", "") },
            { Unit.Centimeter, ("cm", "") },
            { Unit.Millimeter, ("mm", "") },
            { Unit.Inch, ("in", "") },
            { Unit.Foot, ("ft", "") },

            // Area
            { Unit.SquareMeter, ("m²", "") },
            { Unit.SquareCentimeter, ("cm²", "") },
            { Unit.SquareMillimeter, ("mm²", "") },
            { Unit.SquareInch, ("in²", "") },
            { Unit.SquareFoot, ("ft²", "") },

            // Volume
            { Unit.CubicMeter, ("m³", "") },
            { Unit.CubicCentimeter, ("cm³", "") },
            { Unit.CubicMillimeter, ("mm³", "") },
            { Unit.CubicInch, ("in³", "") },
            { Unit.CubicFoot, ("ft³", "") },
            { Unit.Liter, ("L", "") },
            { Unit.Milliliter, ("mL", "") },
            { Unit.Gallon, ("gal", "") },
            { Unit.Quart, ("qt", "") },
            { Unit.Pint, ("pt", "") },
            { Unit.Cup, ("cup", "") },
            { Unit.Ounce, ("oz", "") },

            // Mass
            { Unit.Kilogram, ("kg", "") },
            { Unit.Gram, ("g", "") },
            { Unit.Milligram, ("mg", "") },
            { Unit.Ton, ("t", "") },
            { Unit.Pound, ("lb", "") },

            // Energy
            { Unit.KilowattHour, ("kWh", "") },
            { Unit.KilowattHourPerYear, ("kWh", "a") },
            { Unit.KilowattHourPerAreaAndYear, ("kWh", "m²a") },
            { Unit.Joule, ("J", "") },
            { Unit.Calorie, ("cal", "") },

            // Power
            { Unit.Watt, ("W", "") },
            { Unit.Kilowatt, ("kW", "") },
            { Unit.Horsepower, ("hp", "") },

            // Force
            { Unit.Newton, ("N", "") },
            { Unit.Kilonewton, ("kN", "") },
            { Unit.Megapascal, ("MPa", "") },
            { Unit.Gigapascal, ("GPa", "") },

            // Time
            { Unit.Second, ("s", "") },
            { Unit.Minute, ("min", "") },
            { Unit.Hour, ("h", "") },
            { Unit.Day, ("d", "") },
            { Unit.Week, ("week", "") },
            { Unit.Month, ("M", "") },
            { Unit.Year, ("a", "") },

            // Building Physics
            { Unit.JoulesPerKilogramKelvin, ("J", "kgK") },
            { Unit.WattsPerMeterKelvin, ("W", "mK") },
            { Unit.WattsPerSquareMeterKelvin, ("W", "m²K") },
            { Unit.WattsPerSquareMeter, ("W", "m²") },
            { Unit.KilojoulesPerSquareMeterKelvin, ("kJ", "m²K") },
            { Unit.SquareMeterKelvinPerWatt, ("m²K", "W") },
            { Unit.KilojoulesPerCubicMeterKelvin, ("kJ", "m³K") },

            // Other
            { Unit.Percent, ("%", "") },
            { Unit.PerMeter, ("1", "m") },
            { Unit.PerHour, ("1", "h") },
        };
    }
}
