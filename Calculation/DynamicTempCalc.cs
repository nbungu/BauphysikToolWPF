using BauphysikToolWPF.Models.Domain;
using BT.Logging;
using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Numerics;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{
    // Berechnungen nach DIN EN ISO 13786:2018-04
    // https://enbau-online.ch/bauphysik/2-2-waermespeicherung/
    // https://www.htflux.com/de/u-wert-und-dynamisch-thermische-kenngroessen-eines-inhomogenen-wandaufbaus-holzrahmen/
    // https://www.htflux.com/en/free-calculation-tool-for-thermal-mass-of-building-components-iso-13786/
    public class DynamicTempCalc : ThermalValuesCalc
    {
        // Constant Values
        public const int PeriodDuration = 86400; // [s] = 24 h
        public const int IntervallSteps = 600; // [s] = 10 min
        public const int NormHeatCap = 1080; // Normspeicherkapazität = 0,3 Wh/(kgK) = 1080 J/(kgK) = 1080 Ws/(kgK)

        // private Instance Variables
        private HeatTransferMatrix _zElement = new HeatTransferMatrix();
        private List<HeatTransferMatrix> _zLayers = new List<HeatTransferMatrix>();
        private ThermalAdmittanceMatrix _yElement = new ThermalAdmittanceMatrix();

        // Calculated Properties
        public double DynamicRValue { get; private set; } // R_dyn [m²K/W]
        public double DynamicUValue { get; private set; } // U_dyn [W/m²K]
        public double DecrementFactor { get; private set; } // f [-] - Abminderungsfaktor
        public double TAD { get; private set; } // υ [-] - Temperaturamplitudendämpfung: Verhältnis zwischen den Amplituden der Aussenlufttemperatur und denjenigen der inneren Wandoberflächentemperatur 
        public double TAV { get; private set; } // [-] - Kehrwert der TAD: multipliziert mit 100 enpricht es dem %-Wert der Wärmeamplitude welche innen noch ankommt, aufgrund einer Schwankung außen.
        //public double PenetrationDepth { get; } // δ [m] periodische Eindringtiefe 
        public int TimeShift { get; private set; } // [s] - Phasenverschiebung: Zeitverschiebung des Wärmedurchgangs durch das Bauteil
        public int TimeShift_i { get; private set; } // [s] - Zeitverschiebung der Wärmeaufnahme innen
        public int TimeShift_e { get; private set; } // [s] - Zeitverschiebung der Wärmeaufnahme außen
        public double ThermalAdmittance_i { get; private set; } // [W/m²K] - describes the ability of a surface to absorb and release heat (energy) upon a periodic sinusoidal temperature swing with a period of 24h. 
        public double DynamicThermalAdmittance_i { get; private set; } // [W/m²K] - describes the ability of a surface to absorb and release heat (energy) upon a periodic sinusoidal temperature swing with a period of 24h. 
        public double ThermalAdmittance_e { get; private set; } // [W/m²K] - describes the ability to buffer heat upon external temperature swings. Again, it is assumed that the temperature on the opposite side is held constant.
        public double ArealHeatCapacity_i { get; private set; } // K1 [kJ/(m²K)] - flächenbezogene (spezifische) Wärmekapazität innen
        public double ArealHeatCapacity_e { get; private set; } // K2 [kJ/(m²K)] - flächenbezogene (spezifische) Wärmekapazität außen
        public double EffectiveThermalMass { get; private set; } // M [kg/m²]
        public new bool IsValid { get; private set; } = false;

        public DynamicTempCalc() {}

        // Chose Inheritance approach vs Composition approach 
        public DynamicTempCalc(Element? element, ThermalValuesCalcConfig config) : base(element, config)
        {
            if (Element is null) return;

            CalculateDynamicValues();
        }

        public void CalculateDynamicValues()
        {
            if (Element is null) return;

            try
            {
                // Calculated parameters (private setter)
                _zLayers = CreateLayerMatrices(RelevantLayers);
                _zElement = CreateElementMatrix(_zLayers, Rsi, Rse);
                _yElement = new ThermalAdmittanceMatrix(_zElement);
                DynamicRValue = GetDynamicRValue(_zElement);
                DynamicUValue = GetDynamicUValue(DynamicRValue);
                DecrementFactor = GetDecrement(DynamicUValue);
                TAD = GetTAD(_zElement);
                TAV = GetTAV(TAD);
                TimeShift = GetTimeShift(_yElement);
                TimeShift_i = GetTimeShift(_yElement, "i");
                TimeShift_e = GetTimeShift(_yElement, "e");
                ArealHeatCapacity_i = GetArealHeatCapacity(_zElement, "i");
                ArealHeatCapacity_e = GetArealHeatCapacity(_zElement, "e");
                ThermalAdmittance_i = GetThermalAdmittance(_yElement, "i");
                DynamicThermalAdmittance_i = GetDynamicThermalAdmittance(_yElement, "i");
                ThermalAdmittance_e = GetThermalAdmittance(_yElement, "e");
                EffectiveThermalMass = GetThermalMass(_yElement);
                IsValid = true;
                Logger.LogInfo($"Successfully calculated DynamicTempCalc values of Element: {Element}");
            }
            catch (Exception ex)
            {
                IsValid = false;
                Logger.LogError($"Error calculating DynamicTempCalc values of Element: {Element}, {ex.Message}");
            }
        }

        // public Instance Methods, when user requires additional Data. 
        // Non-static Methods because method accesses intance variables only available when 'new' Instance is created and then the Method can be used

        // θsi(t)
        public double SurfaceTemp_i_Function(int t, double meanTemp_i, double amplitude_i, double amplitude_e, double qStatic)
        {
            double airTemp_i = AirTemp_Function(t, meanTemp_i, amplitude_i);
            double totalHeatFlux_i = TotalHeatFlux_Function(t, amplitude_i, amplitude_e, qStatic, "i");
            return Math.Round(airTemp_i + totalHeatFlux_i * Rsi, 2);
        }

        // θse(t)
        public double SurfaceTemp_e_Function(int t, double meanTemp_e, double amplitude_i, double amplitude_e, double qStatic)
        {
            double airTemp_e = AirTemp_Function(t, meanTemp_e, amplitude_e);
            double totalHeatFlux_e = TotalHeatFlux_Function(t, amplitude_i, amplitude_e, qStatic, "e");
            return Math.Round(airTemp_e - totalHeatFlux_e * Rse, 2);
        }

        // Creates time function θi(t) or θe(t) which represents the sinusodial curve of interior/exterior air temperature change
        public double AirTemp_Function(int t, double meanTemp, double amplitude)
        {
            double airTemp = meanTemp + amplitude * Math.Cos(t * (2 * Math.PI) / PeriodDuration);
            return Math.Round(airTemp, 2);
        }

        public double TotalHeatFlux_Function(int t, double amplitude_i, double amplitude_e, double qStatic, string side)
        {
            // t and timeshift in Seconds!!
            double totalHeatFlux = 0;
            double q_static = qStatic;
            double q_11 = -1 * _yElement.Y11.Magnitude * amplitude_i * Math.Cos((t + TimeShift_i) * (2 * Math.PI) / PeriodDuration);
            double q_12 = -1 * _yElement.Y12.Magnitude * amplitude_i * Math.Cos((t + TimeShift) * (2 * Math.PI) / PeriodDuration);
            double q_21 = _yElement.Y12.Magnitude * amplitude_e * Math.Cos((t + TimeShift) * (2 * Math.PI) / PeriodDuration);
            double q_22 = _yElement.Y22.Magnitude * amplitude_e * Math.Cos((t + TimeShift_e) * (2 * Math.PI) / PeriodDuration);

            if (side == "i")
                totalHeatFlux = q_static + q_11 + q_21;

            if (side == "e")
                totalHeatFlux = q_static + q_12 + q_22;

            return totalHeatFlux;
        }

        public ObservablePoint[] CreateDataPoints(Symbol symbol, double meanTemp_e, double meanTemp_i, double amplitude_i, double amplitude_e, int iterations = PeriodDuration / IntervallSteps + 1, int startingTime = 0)
        {
            ObservablePoint[] dataPoints = new ObservablePoint[iterations];

            double qStatic = Math.Round(UValue * (meanTemp_i - meanTemp_e), 2);

            if (symbol == Symbol.TemperatureSurfaceExterior) // θse(t)
            {
                for (int i = startingTime; i < iterations; i++)
                {
                    int timePoint = i * IntervallSteps; // time axis [s]
                    double x = timePoint;
                    double y = SurfaceTemp_e_Function(timePoint, meanTemp_e, amplitude_i, amplitude_e, qStatic); // temperature axis [°C]
                    dataPoints[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
                }
            }
            else if (symbol == Symbol.TemperatureSurfaceInterior) // θsi(t)
            {
                for (int i = startingTime; i < iterations; i++)
                {
                    int timePoint = i * IntervallSteps; // time axis [s]
                    double x = timePoint;
                    double y = SurfaceTemp_i_Function(timePoint, meanTemp_i, amplitude_i, amplitude_e, qStatic); // temperature axis [°C]
                    dataPoints[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
                }
            }
            else if (symbol == Symbol.TemperatureExterior) // θe(t)
            {
                for (int i = startingTime; i < iterations; i++)
                {
                    int timePoint = i * IntervallSteps; // time axis [s]
                    double x = timePoint;
                    double y = AirTemp_Function(timePoint, meanTemp_e, amplitude_e); // temperature axis [°C]
                    dataPoints[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
                }
            }
            else if (symbol == Symbol.TemperatureInterior) // θi(t)
            {
                for (int i = startingTime; i < iterations; i++)
                {
                    int timePoint = i * IntervallSteps; // time axis [s]
                    double x = timePoint;
                    double y = AirTemp_Function(timePoint, meanTemp_i, amplitude_i); // temperature axis [°C]
                    dataPoints[i] = new ObservablePoint(x, y); // Add x,y Coords to the Array
                }
            }
            return dataPoints;
        }

        // private Methods to assign calculated values to Class Properties
        private List<HeatTransferMatrix> CreateLayerMatrices(List<Layer> layers)
        {
            List<HeatTransferMatrix> list = new List<HeatTransferMatrix>();
            foreach (Layer layer in layers)
            {
                if (!layer.IsEffective)
                    break;
                // Eindringtiefe δ [m]: Delta ist diejenige Tiefe in einem halbunendlichen Baustoff, bei der die Temperaturschwankung auf 1/e des Wertes der Oberflächentemperaturschwankung abgeklungen ist:
                double delta = Math.Sqrt((layer.Material.ThermalConductivity * PeriodDuration) / Convert.ToDouble(layer.Material.BulkDensity * layer.Material.SpecificHeatCapacity * Math.PI));
                // ξ [-]: Xi ist das Verhältnis von Schichtdicke zu Eindringtiefe
                double xi = (layer.Thickness / 100) / delta;
                list.Add(HeatTransferMatrix.CreateLayerMatrix(delta, xi, layer.Material.ThermalConductivity));
            }
            return list;
        }
        private HeatTransferMatrix CreateElementMatrix(List<HeatTransferMatrix> layerMatrices, double rsi, double rse)
        {
            // Add exterior environment Matrix to the element
            HeatTransferMatrix elementMatrix = HeatTransferMatrix.CreateEnvironmentMatrix(rse);

            // Mulitply every Layer HeatTransferMatrix: From Outside to Inside! Z_n * Z_n-1 * ... * Z2 * Z1
            for (int i = layerMatrices.Count - 1; i >= 0; i--)
            {
                elementMatrix = MultiplyMatrices(elementMatrix, layerMatrices[i]);
            }

            // Multiply inner most Layer Matrix with interior environment Matrix
            return MultiplyMatrices(elementMatrix, HeatTransferMatrix.CreateEnvironmentMatrix(rsi));
        }
        private HeatTransferMatrix MultiplyMatrices(HeatTransferMatrix Z1, HeatTransferMatrix Z2)
        {
            return new HeatTransferMatrix()
            {
                Z11 = Complex.Add(Complex.Multiply(Z1.Z11, Z2.Z11), Complex.Multiply(Z1.Z12, Z2.Z21)),
                Z12 = Complex.Add(Complex.Multiply(Z1.Z11, Z2.Z12), Complex.Multiply(Z1.Z12, Z2.Z22)),
                Z21 = Complex.Add(Complex.Multiply(Z1.Z21, Z2.Z11), Complex.Multiply(Z1.Z22, Z2.Z21)),
                Z22 = Complex.Add(Complex.Multiply(Z1.Z21, Z2.Z12), Complex.Multiply(Z1.Z22, Z2.Z22)),
            };
        }
        private double GetDynamicRValue(HeatTransferMatrix elementMatrix)
        {
            return Math.Round(Complex.Abs(elementMatrix.Z12), 3);
        }
        private double GetDynamicUValue(double dyn_rValue)
        {
            return Math.Round(1 / dyn_rValue, 3);
        }
        private double GetDecrement(double uDynValue)
        {
            double uValue = 1 / (Rsi + Element.RGesValue + Rse);
            return Math.Round(uDynValue * uValue, 3);
        }
        private double GetTAD(HeatTransferMatrix elementMatrix)
        {
            return Math.Round(Complex.Abs(elementMatrix.Z22), 1);
        }
        private double GetTAV(double tad)
        {
            return Math.Round(1 / tad, 3);
        }
        private int GetTimeShift(ThermalAdmittanceMatrix matrix, string? side = null)
        {
            double seconds = 0;
            if (side == null)
                seconds = (PeriodDuration * matrix.Y12.Phase) / (2 * Math.PI) - (PeriodDuration / 2);

            if (side == "i")
                seconds = (PeriodDuration * matrix.Y11.Phase) / (2 * Math.PI);

            if (side == "e")
                seconds = (PeriodDuration * matrix.Y22.Phase) / (2 * Math.PI);

            return Convert.ToInt32(seconds);
        }
        private double GetThermalAdmittance(ThermalAdmittanceMatrix matrix, string side)
        {
            double result = 0;
            if (side == "i")
                result = Math.Round(matrix.Y11.Magnitude, 2);

            if (side == "e")
                result = Math.Round(matrix.Y22.Magnitude, 2);

            return result;
        }
        private double GetDynamicThermalAdmittance(ThermalAdmittanceMatrix matrix, string side)
        {
            double result = 0;
            if (side == "i")
                result = Math.Round(matrix.Y12.Magnitude, 2);

            return result;
        }
        private double GetArealHeatCapacity(HeatTransferMatrix elementMatrix, string side)
        {
            double kappa = 0;
            if (side == "i")
                kappa = (PeriodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z11, 1), elementMatrix.Z12)); // Ws/(m²K) = J/(m²K) 

            if (side == "e")
                kappa = (PeriodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z22, 1), elementMatrix.Z12)); // Ws/(m²K) = J/(m²K)

            return Math.Round(kappa / 1000, 2); // kJ/(m²K)
        }
        private double GetThermalMass(ThermalAdmittanceMatrix matrix)
        {
            double c_0 = NormHeatCap;
            double M = (Complex.Abs(matrix.Y11) * PeriodDuration) / (2 * Math.PI * c_0);
            return Math.Round(M, 2);
        }
    }
    internal class HeatTransferMatrix
    {
        // Complex values of the Heat Transfer Matrix
        public Complex Z11 { get; set; }
        public Complex Z12 { get; set; }
        public Complex Z22 { get; set; }
        public Complex Z21 { get; set; }

        // Creates Layer Heat Transfer Matrix
        public static HeatTransferMatrix CreateLayerMatrix(double delta, double xi, double lambda)
        {
            double factorZ12 = -delta / (2 * lambda);
            double factorZ21 = -lambda / delta;
            HeatTransferMatrix elementMatrix = new HeatTransferMatrix()
            {
                Z11 = new Complex(Math.Cosh(xi) * Math.Cos(xi), Math.Sinh(xi) * Math.Sin(xi)),
                Z22 = new Complex(Math.Cosh(xi) * Math.Cos(xi), Math.Sinh(xi) * Math.Sin(xi)),
                Z12 = factorZ12 * new Complex(Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi), Math.Cosh(xi) * Math.Sin(xi) - Math.Sinh(xi) * Math.Cos(xi)),
                Z21 = factorZ21 * new Complex(Math.Sinh(xi) * Math.Cos(xi) - Math.Cosh(xi) * Math.Sin(xi), Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi))
            };
            return elementMatrix;
        }
        // Creates Environment Border Matrix: If a layer side borders any other environment, instead of direct layer to layer contact: e.g. air layer oder outside air
        public static HeatTransferMatrix CreateEnvironmentMatrix(double rValue)
        {
            HeatTransferMatrix elementMatrix = new HeatTransferMatrix()
            {
                Z11 = 1,
                Z12 = -rValue,
                Z22 = 1,
                Z21 = 0,
            };
            return elementMatrix;
        }
    }
    internal class ThermalAdmittanceMatrix
    {
        // Complex values of the Heat Transfer Matrix
        public Complex Y11 { get; set; }
        public Complex Y12 { get; set; }
        public Complex Y22 { get; set; }
        public Complex? Y21 { get; set; }

        public ThermalAdmittanceMatrix(HeatTransferMatrix elementMatrix)
        {
            Y11 = -1 * Complex.Divide(elementMatrix.Z11, elementMatrix.Z12);
            Y12 = Complex.Divide(1, elementMatrix.Z12);
            Y21 = null; // Wert in Norm nicht definiert
            Y22 = -1 * Complex.Divide(elementMatrix.Z22, elementMatrix.Z12);
        }
        public ThermalAdmittanceMatrix() {}
    }
}
