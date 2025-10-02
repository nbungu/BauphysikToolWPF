using System.Collections.Generic;

namespace BauphysikToolWPF.Models.UI
{
    public class Enums
    {
        // Keep Order!
        public enum Symbol
        {
            None = 0,

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
            FRsi,                               // fRsi

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
            TransmissionHeatTransferCoef,       // H_T
            VentilationHeatTransferCoef,        // H_V
            HeatTransferCoef,                   // H
            ThermalBridgeSurcharge,             // ΔU_WB
            AirExchangeRate,                    // n
            TempCorrectionFactor,               // F_x
        }

        public static readonly Dictionary<Symbol, (Unit unit, string baseText, string subscriptText, string name, string comment)> SymbolMapping = new()
        {
            { Symbol.None, (Unit.None, "", "", "", "") },
            { Symbol.Length, (Unit.Centimeter, "l", "", "Länge", "") },
            { Symbol.Distance, (Unit.Centimeter, "a", "", "Abstand", "") },
            { Symbol.Area, (Unit.SquareMeter, "A", "", "Fläche", "Fläche") },
            { Symbol.Volume, (Unit.CubicMeter, "V", "", "Volumen", "Volumen") },
            { Symbol.Thickness, (Unit.Centimeter, "d", "", "Dicke", "Dicke") },
            { Symbol.Width, (Unit.Centimeter, "b", "", "Breite", "Breite") },
            { Symbol.Height, (Unit.Centimeter, "h", "", "Höhe", "Höhe") },
            { Symbol.TemperatureInterior, (Unit.Celsius, "θ", "i", "Raumtemperatur", "Raumtemperatur") },
            { Symbol.TemperatureExterior, (Unit.Celsius, "θ", "e", "Außentemperatur", "Außentemperatur") },
            { Symbol.TemperatureSurfaceInterior, (Unit.Celsius, "θ", "si", "Oberflächentemperatur, Innen", "") },
            { Symbol.TemperatureSurfaceExterior, (Unit.Celsius, "θ", "se", "Oberflächentemperatur, Außen", "") },
            { Symbol.RValueLayer, (Unit.SquareMeterKelvinPerWatt, "R", "i", "R-Wert", "Wärmedurchlasswiderstand") },
            { Symbol.RValueElement, (Unit.SquareMeterKelvinPerWatt, "R", "ges", "R-Wert Bauteil", "Wärmedurchlasswiderstand") },
            { Symbol.RValueTotal, (Unit.SquareMeterKelvinPerWatt, "R", "T", "R-Total", "Wärmedurchlasswiderstand") },
            { Symbol.TransferResistanceSurfaceInterior, (Unit.SquareMeterKelvinPerWatt, "R", "si", "Wärmeübergangswiderstand, Innen", "") },
            { Symbol.TransferResistanceSurfaceExterior, (Unit.SquareMeterKelvinPerWatt, "R", "se", "Wärmeübergangswiderstand, Außen", "") },
            { Symbol.UValue, (Unit.WattsPerSquareMeterKelvin, "U", "", "U-Wert", "Wärmedurchgangskoeffizient: Maß für die Wärmedämmung eines Bauteils. Er gibt an, wie viel Wärme pro Quadratmeter und Kelvin durch ein Bauteil fließt, wenn eine Temperaturdifferenz von einem Kelvin zwischen den beiden Seiten besteht. Je niedriger der U-Wert, desto besser ist die Wärmedämmung.") },
            { Symbol.RawDensity, (Unit.KilogramPerCubicMeter, "ρ", "", "Rohdichte", "") },
            { Symbol.AreaMassDensity, (Unit.KilogramPerSquareMeter, "m'", "", "Flächenbez. Masse", "") },
            { Symbol.SdThickness, (Unit.Meter, "s", "d", "sd-Wert", "Wasserdampfdiffusionsäquivalente Luftschichtdicke") },
            { Symbol.RelativeHumidity, (Unit.Percent, "φ", "", "Rel. Luftfeuchtigkeit", "") },
            { Symbol.RelativeHumidityInterior, (Unit.Percent, "φ", "i", "Rel. Luftfeuchtigkeit, Innen", "") },
            { Symbol.RelativeHumidityExterior, (Unit.Percent, "φ", "e", "Rel. Luftfeuchtigkeit, Außen", "") },
            { Symbol.VapourDiffusionResistance, (Unit.None, "µ", "", "µ-Wert", "Wasserdampfdiffusionswiderstandszahl (trocken)") },
            { Symbol.ThermalConductivity, (Unit.WattsPerMeterKelvin, "λ", "", "Wärmeleitfähigkeit", "Bemessungswert der Wärmeleitfähigkeit (WLF)") },
            { Symbol.SpecificHeatCapacity, (Unit.JoulesPerKilogramKelvin, "c", "", "Spezifische Wärmekapazität", "") },
            { Symbol.VolumetricHeatCapacity, (Unit.KilojoulesPerCubicMeterKelvin, "C", "V", "Volumenbez. Wärmekapazität", "") },
            { Symbol.ArealHeatCapacity, (Unit.KilojoulesPerSquareMeterKelvin, "C", "", "Flächenbez. Wärmekapazität", "Quotient aus Wärmekapazität und Bauteilfläche") },
            { Symbol.HeatFluxDensity, (Unit.WattsPerSquareMeter, "q", "", "Wärmestromdichte", "Die Wärmestromdichte beschreibt die Menge an Wärmeenergie, die pro Zeiteinheit durch eine bestimmte Fläche fließt") },
            { Symbol.FRsi, (Unit.None, "f", "Rsi", "Temperaturfaktor für die raumseitige Oberfläche", "Wärmebrücken-Index. Er gibt an, ob eine Baukonstruktion in Bezug auf Wärmebrücken mangelhaft ist.") },
            { Symbol.UValueDynamic, (Unit.WattsPerSquareMeterKelvin, "U", "dyn", "U-dynamisch", "dynamischer Wärmedurchgangskoeffizient") },
            { Symbol.RValueDynamic, (Unit.SquareMeterKelvinPerWatt, "R", "dyn", "R-dynamisch", "dynamischer Wärmedurchlasswiderstand") },
            { Symbol.TemperatureAmplitudeDamping, (Unit.None, "υ", "", "Temperaturamplitudendämpfung", "") },
            { Symbol.TemperatureAmplitudeRatio, (Unit.None, "", "", "Temperaturamplitudenverhältnis", "Kehrwert der TAD: multipliziert mit 100 enpricht es dem %-Wert der Wärmeamplitude welche innen noch ankommt, aufgrund einer Schwankung außen.") },
            { Symbol.TimeShift, (Unit.Hour, "Δt", "", "Phasenverschiebung", "Zeitvorsprung (wenn positiv) oder Verzögerung (wenn negativ)") },
            { Symbol.EffectiveThermalMass, (Unit.KilogramPerSquareMeter, "M", "", "Speicherwirksame Masse", "") },
            { Symbol.DecrementFactor, (Unit.None, "f", "", "Dekrement", "Abminderungsfaktor: Verhältnis des Betrags der dynamischen Wärmeaufnahme zum Wärmedurchgangskoeffizienten U unter\r\nstationären Bedingungen") },
            { Symbol.ThermalAdmittance, (Unit.WattsPerSquareMeterKelvin, "Y", "mm", "Wärmeaufnahme", "komplexe Größe, festgelegt als komplexe Amplitude der Wärmestromdichte durch die an Zone m des\r\nBauteils angrenzende Oberfläche, geteilt durch die komplexe Temperaturamplitude in der gleichen Zone,\r\nwenn die Temperatur auf der anderen Seite konstant gehalten wird") },
            { Symbol.DynamicThermalAdmittance, (Unit.WattsPerSquareMeterKelvin, "Y", "mn", "dynamische Wärmeaufnahme", "komplexe Größe, festgelegt als komplexe Amplitude der Wärmestromdichte durch die an Zone m des Bau-\r\nteils angrenzende Oberfläche geteilt durch die komplexe Temperaturamplitude in der Zone n, wenn die\r\nTemperatur in Zone m konstant gehalten wird") },
            { Symbol.PrimaryEnergy, (Unit.KilowattHourPerYear, "Q", "p", "Jahresprimärenergiebedarf (absolut)", "Beschreibt die Energiemenge, die benötigt wird, um den Endenergiebedarf zu decken, inklusive aller vorgelagerten Energieprozesse (Gewinnung, Umwandlung, Transport).") },
            { Symbol.PrimaryEnergyPerArea, (Unit.KilowattHourPerAreaAndYear, "q", "p", "Jahresprimärenergiebedarf (flächenbezogen)", "Beschreibt die Energiemenge, die benötigt wird, um den Endenergiebedarf zu decken, inklusive aller vorgelagerten Energieprozesse (Gewinnung, Umwandlung, Transport).") },
            { Symbol.AToVRatio, (Unit.PerMeter, "A", "V", "A zu V Verhältnis", "") },
            { Symbol.SpecificHeatTransmissionLoss, (Unit.WattsPerSquareMeterKelvin, "H", "T'", "spezifischer Transmissionswärmeverlust", "spezifischer Transmissionswärmetransferkoeffizient (Bezeichnung in DIN V 18599) bzw. den spezifischer Transmissionswärmeverlust (Bezeichnung in DIN V 4108-6). Auf die wärmeübertragende Hüllfläche bezogener Transmissionswärmeverlust") },
            { Symbol.TransmissionHeatTransferCoef, (Unit.WattsPerKelvin, "H", "T", "Transmissionswärmetransferkoeffizient", "Transmissionswärmeverlust") },
            { Symbol.VentilationHeatTransferCoef, (Unit.WattsPerKelvin, "H", "V", "Lüftungswärmetransferkoeffizient", "Lüftungswärmetransferkoeffizient") },
            { Symbol.HeatTransferCoef, (Unit.WattsPerKelvin, "H", "", "Wärmetransferkoeffizient", "Wärmetransferkoeffizient") },
            { Symbol.ThermalBridgeSurcharge, (Unit.WattsPerSquareMeterKelvin, "ΔU", "WB", "Wärmebrückenzuschlag", "Faktor, der bei der Berechnung des Energiebedarfs eines Gebäudes berücksichtigt werden muss, um den zusätzlichen Wärmeverlust durch Wärmebrücken zu erfassen.") },
            { Symbol.AirExchangeRate, (Unit.PerHour, "n", "", "Luftwechselzahl", "Luftvolumenstrom je Volumeneinheit") },
            { Symbol.TempCorrectionFactor, (Unit.None, "F", "x", "Temperatur-Korrekturfaktor", "Temperatur-Korrekturfaktor für Bauteilart x. Abminderungsfaktor, der dazu dient, den Wärmestrom zwischen einem beheizten Raum und einer angrenzenden nicht beheizten oder unbeheizten Zone korrekt zu bewerten") },
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
            WattsPerKelvin,                // W/K - 
            WattsPerMeterKelvin,           // W/(m·K) - Thermal Conductivity
            WattsPerSquareMeterKelvin,     // W/(m²·K) - Thermal Transmittance (U-value)
            WattsPerSquareMeter,           // W/m² - Heat Flux Density
            KilojoulesPerSquareMeterKelvin,// kJ/(m²·K) - Heat Storage Capacity
            SquareMeterKelvinPerWatt,      // m²·K/W - Thermal Resistance (R-value)
            KilojoulesPerCubicMeterKelvin, // kJ/(m³·K) - Volumetric Heat Capacity

            // Other
            Percent,                    // %
            PerMeter,                   // 1/m
            PerHour,                    // 1/h   
        }
        public static readonly Dictionary<Unit, (string counterText, string denominatorText, string fullString)> UnitDisplayMapping = new()
        {
            { Unit.None, ("", "", "") },

            // Temperature
            { Unit.Celsius, ("°C", "", "°C") },
            { Unit.Kelvin, ("K", "", "K") },

            // Density
            { Unit.KilogramPerCubicMeter, ("kg", "m³", "kg/m³") },
            { Unit.KilogramPerSquareMeter, ("kg", "m²", "kg/m²") },
            { Unit.GramPerCubicMeter, ("g", "m³", "g/m³") },
            { Unit.MilligramPerCubicMeter, ("mg", "m³", "mg/m³") },

            // Volume Flow
            { Unit.CubicMeterPerSecond, ("m³", "s", "m³/s") },
            { Unit.CubicMeterPerHour, ("m³", "h", "m³/h") },
            { Unit.CubicMeterPerDay, ("m³", "d", "m³/d") },

            // Pressure
            { Unit.Pascal, ("Pa", "", "Pa") },
            { Unit.Bar, ("bar", "", "bar") },
            { Unit.Atmosphere, ("atm", "", "atm") },

            // Length
            { Unit.Meter, ("m", "", "m") },
            { Unit.Centimeter, ("cm", "", "cm") },
            { Unit.Millimeter, ("mm", "", "mm") },
            { Unit.Inch, ("in", "", "in") },
            { Unit.Foot, ("ft", "", "ft") },

            // Area
            { Unit.SquareMeter, ("m²", "", "m²") },
            { Unit.SquareCentimeter, ("cm²", "", "cm²") },
            { Unit.SquareMillimeter, ("mm²", "", "mm²") },
            { Unit.SquareInch, ("in²", "", "in²") },
            { Unit.SquareFoot, ("ft²", "", "ft²") },

            // Volume
            { Unit.CubicMeter, ("m³", "", "m³") },
            { Unit.CubicCentimeter, ("cm³", "", "cm³") },
            { Unit.CubicMillimeter, ("mm³", "", "mm³") },
            { Unit.CubicInch, ("in³", "", "in³") },
            { Unit.CubicFoot, ("ft³", "", "ft³") },
            { Unit.Liter, ("L", "", "L") },
            { Unit.Milliliter, ("mL", "", "mL") },
            { Unit.Gallon, ("gal", "", "gal") },
            { Unit.Quart, ("qt", "", "qt") },
            { Unit.Pint, ("pt", "", "pt") },
            { Unit.Cup, ("cup", "", "cup") },
            { Unit.Ounce, ("oz", "", "oz") },

            // Mass
            { Unit.Kilogram, ("kg", "", "kg") },
            { Unit.Gram, ("g", "", "g") },
            { Unit.Milligram, ("mg", "", "mg") },
            { Unit.Ton, ("t", "", "t") },
            { Unit.Pound, ("lb", "", "lb") },

            // Energy
            { Unit.KilowattHour, ("kWh", "", "kWh") },
            { Unit.KilowattHourPerYear, ("kWh", "a", "kWh/a") },
            { Unit.KilowattHourPerAreaAndYear, ("kWh", "m²a", "kWh/m²a") },
            { Unit.Joule, ("J", "", "J") },
            { Unit.Calorie, ("cal", "", "cal") },

            // Power
            { Unit.Watt, ("W", "", "W") },
            { Unit.Kilowatt, ("kW", "", "kW") },
            { Unit.Horsepower, ("hp", "", "hp") },

            // Force
            { Unit.Newton, ("N", "", "N") },
            { Unit.Kilonewton, ("kN", "", "kN") },
            { Unit.Megapascal, ("MPa", "", "MPa") },
            { Unit.Gigapascal, ("GPa", "", "GPa") },

            // Time
            { Unit.Second, ("s", "", "s") },
            { Unit.Minute, ("min", "", "min") },
            { Unit.Hour, ("h", "", "h") },
            { Unit.Day, ("d", "", "d") },
            { Unit.Week, ("week", "", "week") },
            { Unit.Month, ("M", "", "M") },
            { Unit.Year, ("a", "", "a") },

            // Building Physics
            { Unit.JoulesPerKilogramKelvin, ("J", "kgK", "J/kgK") },
            { Unit.WattsPerKelvin, ("W", "K", "W/K") },
            { Unit.WattsPerMeterKelvin, ("W", "mK", "W/mK") },
            { Unit.WattsPerSquareMeterKelvin, ("W", "m²K", "W/m²K") },
            { Unit.WattsPerSquareMeter, ("W", "m²", "W/m²") },
            { Unit.KilojoulesPerSquareMeterKelvin, ("kJ", "m²K", "kJ/m²K") },
            { Unit.SquareMeterKelvinPerWatt, ("m²K", "W", "m²K/W") },
            { Unit.KilojoulesPerCubicMeterKelvin, ("kJ", "m³K", "kJ/m³K") },

            // Other
            { Unit.Percent, ("%", "", "%") },
            { Unit.PerMeter, ("1", "m", "1/m") },
            { Unit.PerHour, ("1", "h", "1/h") },
        };
        public static string GetUnitStringFromSymbol(Symbol symbol)
        {
            if (SymbolMapping.TryGetValue(symbol, out var symbolData))
            {
                var unit = symbolData.unit;
                if (UnitDisplayMapping.TryGetValue(unit, out var unitDisplay))
                {
                    return unitDisplay.fullString;
                }
            }
            return ""; // Fallback for unknown symbol or unit
        }

        public enum ElementSortingType
        {
            DateDescending = 0,
            DateAscending,
            NameAscending,
            NameDescending,
            TypeNameAscending,
            TypeNameDescending
        }
        public static readonly Dictionary<ElementSortingType, string> ElementSortingTypeMapping = new()
        {
            { ElementSortingType.DateDescending, "Änderungsdatum (neueste zuerst)" },
            { ElementSortingType.DateAscending, "Änderungsdatum (älteste zuerst)" },
            { ElementSortingType.NameAscending, "Name (aufsteigend)" },
            { ElementSortingType.NameDescending, "Name (absteigend)" }
        };

        public enum ElementGroupingType
        {
            None,
            Type,
            Tag
        }
        public static readonly Dictionary<ElementGroupingType, string> ElementGroupingTypeMapping = new()
        {
            { ElementGroupingType.None, "Ohne" },
            { ElementGroupingType.Type, "Konstruktions-Typ" },
            { ElementGroupingType.Tag, "Tags" }
        };
    }
}
