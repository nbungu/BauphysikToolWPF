using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BauphysikToolWPF.ComponentCalculations
{
    // Berechnungen nach DIN EN ISO 13786:2018-04
    // https://enbau-online.ch/bauphysik/2-2-waermespeicherung/
    // https://www.htflux.com/de/u-wert-und-dynamisch-thermische-kenngroessen-eines-inhomogenen-wandaufbaus-holzrahmen/
    // https://www.htflux.com/en/free-calculation-tool-for-thermal-mass-of-building-components-iso-13786/
    public class DynamicTempCalc
    {
        // Constant Values
        public const int PeriodDuration = 86400; // [s] = 24 h
        public const int IntervallSteps = 600; // [s] = 10 min
        public const int NormHeatCap = 1080; // Normspeicherkapazität = 0,3 Wh/(kgK) = 1080 J/(kgK) = 1080 Ws/(kgK)

        // Variables for Calculation
        public Element Element { get; set; }
        private HeatTransferMatrix Z_Element { get; set; }
        private List<HeatTransferMatrix> Z_Layers { get; set; }
        private ThermalAdmittanceMatrix Y_Element { get; set; }
        private double Rsi { get; }
        private double Rse { get; }

        // Calculated Results
        public double DynamicRValue { get; set; } // R_dyn [m²K/W]
        public double DynamicUValue { get; set; } // U_dyn [W/m²K]
        public double DecrementFactor { get; set; } // f [-] - Abminderungsfaktor
        public double TAD { get; set; } // υ [-] - Temperaturamplitudendämpfung: Verhältnis zwischen den Amplituden der Aussenlufttemperatur und denjenigen der inneren Wandoberflächentemperatur 
        public double TAV { get; set; } // [-] - Kehrwert der TAD: multipliziert mit 100 enpricht es dem %-Wert der Wärmeamplitude welche innen noch ankommt, aufgrund einer Schwankung außen.
        public double PenetrationDepth { get; set; } // δ [m] periodische Eindringtiefe 
        public int TimeShift { get; set; } // [s] - Phasenverschiebung: Zeitverschiebung des Wärmedurchgangs durch das Bauteil
        public int TimeShift_i { get; set; } // [s] - Zeitverschiebung der Wärmeaufnahme innen
        public int TimeShift_e { get; set; } // [s] - Zeitverschiebung der Wärmeaufnahme außen
        public double ThermalAdmittance_i { get; set; } // [W/m²K] - describes the ability of a surface to absorb and release heat (energy) upon a periodic sinusoidal temperature swing with a period of 24h. 
        public double ThermalAdmittance_e { get; set; } // [W/m²K] - describes the ability to buffer heat upon external temperature swings. Again, it is assumed that the temperature on the opposite side is held constant.
        public double ArealHeatCapacity_i { get; set; } // K1 [kJ/(m²K)] - flächenbezogene (spezifische) Wärmekapazität innen
        public double ArealHeatCapacity_e { get; set; } // K2 [kJ/(m²K)] - flächenbezogene (spezifische) Wärmekapazität außen
        public double EffectiveThermalMass { get; set; } // M [kg/m²]

        public DynamicTempCalc(Element element, Dictionary<string, double> userEnvVars)
        {
            if (element is null || element.Layers.Count == 0)
                return;

            if (userEnvVars is null)
                return;

            // Assign constuctor parameter values
            Element = element;
            Rsi = userEnvVars["Rsi"];
            Rse = userEnvVars["Rse"];

            // Calculated parameters (private setter)
            Z_Layers = CreateLayerMatrices(Element.Layers);
            Z_Element = CreateElementMatrix(Z_Layers, Rsi, Rse);
            Y_Element = new ThermalAdmittanceMatrix(Z_Element);
            DynamicRValue = GetDynamicRValue(Z_Element);
            DynamicUValue = GetDynamicUValue(DynamicRValue);
            DecrementFactor = GetDecrement(DynamicUValue);
            TAD = GetTAD(Z_Element);
            TAV = GetTAV(TAD);
            TimeShift = GetTimeShift(Y_Element);
            TimeShift_i = GetTimeShift(Y_Element, "i");
            TimeShift_e = GetTimeShift(Y_Element, "e");
            ArealHeatCapacity_i = GetArealHeatCapacity(Z_Element, "i");
            ArealHeatCapacity_e = GetArealHeatCapacity(Z_Element, "e");
            ThermalAdmittance_i = GetThermalAdmittance(Y_Element, "i");
            ThermalAdmittance_e = GetThermalAdmittance(Y_Element, "e");
            EffectiveThermalMass = GetThermalMass(Y_Element);
        }


        // Non-static because method accesses intance variables

        // θsi(t)
        public double SurfaceTemp_i_Function(int t, double meanTemp_i, double amplitude_i, double amplitude_e)
        {
            double airTemp_i = AirTemp_Function(t, meanTemp_i, amplitude_i);
            double totalHeatFlux_i = TotalHeatFlux_Function(t, amplitude_i, amplitude_e, "i");
            return Math.Round(airTemp_i + totalHeatFlux_i * Rsi, 2);
        }
        
        // θse(t)
        public double SurfaceTemp_e_Function(int t, double meanTemp_e, double amplitude_i, double amplitude_e)
        {
            double airTemp_e = AirTemp_Function(t, meanTemp_e, amplitude_e);
            double totalHeatFlux_e = TotalHeatFlux_Function(t, amplitude_i, amplitude_e, "e");
            return Math.Round(airTemp_e - totalHeatFlux_e * Rse, 2);
        }

        // Creates time function θi(t) or θe(t) which represents the sinusodial curve of interior/exterior air temperature change
        public double AirTemp_Function(int t, double meanTemp, double amplitude)
        {
            double airTemp = meanTemp + amplitude * Math.Cos(t * (2 * Math.PI) / PeriodDuration);
            return Math.Round(airTemp, 2);
        }

        public double TotalHeatFlux_Function(int t, double amplitude_i, double amplitude_e, string side)
        {
            // t and timeshift in Seconds!!
            double totalHeatFlux = 0;
            double q_static = 0; // TODO
            double q_11 = -1 * Y_Element.Y11.Magnitude * amplitude_i * Math.Cos((t + TimeShift_i) * (2 * Math.PI) / PeriodDuration);
            double q_12 = -1 * Y_Element.Y12.Magnitude * amplitude_i * Math.Cos((t + TimeShift) * (2 * Math.PI) / PeriodDuration);
            double q_21 = Y_Element.Y12.Magnitude * amplitude_e * Math.Cos((t + TimeShift) * (2 * Math.PI) / PeriodDuration);
            double q_22 = Y_Element.Y22.Magnitude * amplitude_e * Math.Cos((t + TimeShift_e) * (2 * Math.PI) / PeriodDuration);
            
            if (side == "i")
                totalHeatFlux = q_static + q_11 + q_21;

            if (side == "e")
                totalHeatFlux = q_static + q_12 + q_22;

            return totalHeatFlux;
        }

        private List<HeatTransferMatrix> CreateLayerMatrices(List<Layer> layers)
        {
            List <HeatTransferMatrix> list = new List<HeatTransferMatrix>();
            foreach (Layer layer in layers)
            {
                // Eindringtiefe δ [m]: Delta ist diejenige Tiefe in einem halbunendlichen Baustoff, bei der die Temperaturschwankung auf 1/e des Wertes der Oberflächentemperaturschwankung abgeklungen ist:
                double delta = Math.Sqrt((layer.Material.ThermalConductivity * PeriodDuration) / Convert.ToDouble(layer.Material.BulkDensity * layer.Material.SpecificHeatCapacity * Math.PI));
                // ξ [-]: Xi ist das Verhältnis von Schichtdicke zu Eindringtiefe
                double xi = (layer.LayerThickness / 100) / delta;
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
            HeatTransferMatrix result = new HeatTransferMatrix()
            {
                Z11 = Complex.Add(Complex.Multiply(Z1.Z11, Z2.Z11), Complex.Multiply(Z1.Z12, Z2.Z21)),
                Z12 = Complex.Add(Complex.Multiply(Z1.Z11, Z2.Z12), Complex.Multiply(Z1.Z12, Z2.Z22)),
                Z21 = Complex.Add(Complex.Multiply(Z1.Z21, Z2.Z11), Complex.Multiply(Z1.Z22, Z2.Z21)),
                Z22 = Complex.Add(Complex.Multiply(Z1.Z21, Z2.Z12), Complex.Multiply(Z1.Z22, Z2.Z22)),
            };
            return result;
        }
        private double GetDynamicRValue(HeatTransferMatrix elementMatrix)
        {
            return Math.Round(Complex.Abs(elementMatrix.Z12), 3);
        }
        private double GetDynamicUValue(double rValue)
        {
            return Math.Round(1/rValue, 3);
        }
        private double GetDecrement(double u_dyn)
        {
            double u_0 = Element.RValue + Rsi + Rse;
            return Math.Round(u_dyn * u_0, 3);
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
            {
                seconds = (PeriodDuration * matrix.Y12.Phase) / (2 * Math.PI) - (PeriodDuration/2);
            }
            if (side == "i")
            {
                seconds = (PeriodDuration * matrix.Y11.Phase) / (2 * Math.PI);
            }
            if (side == "e")
            {
                seconds = (PeriodDuration * matrix.Y22.Phase) / (2 * Math.PI);
            }
            return Convert.ToInt32(seconds);
        }
        private double GetThermalAdmittance(ThermalAdmittanceMatrix matrix, string side)
        {
            if (side == "i")
            {
                return Math.Round(matrix.Y11.Magnitude, 2);
            }
            else if (side == "e")
            {
                return Math.Round(matrix.Y22.Magnitude, 2);
            }
            else
            {
                return 0;
            }
        }
        private double GetArealHeatCapacity(HeatTransferMatrix elementMatrix, string side = "i")
        {
            double kappa1 = (PeriodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z11, 1), elementMatrix.Z12)); // Ws/(m²K) = J/(m²K) 
            double kappa2 = (PeriodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z22, 1), elementMatrix.Z12)); // Ws/(m²K) = J/(m²K)
            return side == "e" ? Math.Round(kappa2/1000, 3) : Math.Round(kappa1/1000, 3); // kJ/(m²K)
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
    }
}
