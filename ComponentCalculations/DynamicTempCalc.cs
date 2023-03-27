using BauphysikToolWPF.SessionData;
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
        // Variables for Calculation
        private Element Element { get; set; }
        private List<HeatTransferMatrix> LayerHeatTransferMatrices { get; set; }
        private HeatTransferMatrix ElementMatrix { get; set; }
        private int PeriodDuration { get; } = 86400;
        private double Rsi { get; } = UserSaved.Rsi;
        private double Rse { get; } = UserSaved.Rse;

        // Calculated Results
        public double DynamicRValue { get; set; } // R_dyn [m²K/W]
        public double DynamicUValue { get; set; } // U_dyn [W/m²K]
        public double DecrementFactor { get; set; } // f [-] - Abminderungsfaktor
        public double TAD { get; set; } // [-]
        public double TAV { get; set; } // [-]
        public double PenetrationDepth { get; set; } // δ [m] periodische Eindringtiefe 
        public double TimeShift { get; set; } // [h] - Phasenverschiebung: Zeitverschiebung des Wärmedurchgangs durch das Bauteil
        public double TimeShift_i { get; set; } // [h] - Zeitverschiebung der Wärmeaufnahme innen
        public double TimeShift_e { get; set; } // [h] - Zeitverschiebung der Wärmeaufnahme außen
        public double ThermalAdmittance_i { get; set; } // [W/m²K] - describes the ability of a surface to absorb and release heat (energy) upon a periodic sinusoidal temperature swing with a period of 24h. 
        public double ThermalAdmittance_e { get; set; } // [W/m²K] - describes the ability to buffer heat upon external temperature swings. Again, it is assumed that the temperature on the opposite side is held constant.
        public double ArealHeatCapacity_i { get; set; } // K1 [kJ/(m²K)] - flächenbezogene (spezifische) Wärmekapazität innen
        public double ArealHeatCapacity_e { get; set; } // K2 [kJ/(m²K)] - flächenbezogene (spezifische) Wärmekapazität außen
        public double EffectiveThermalMass { get; set; } // M [kg/m²]
        public double Test { get; set; } // Y [W/(m²K)]

        public DynamicTempCalc(Element element)
        {
            Element = element;

            LayerHeatTransferMatrices = GetLayerMatrices(Element.Layers, PeriodDuration);
            ElementMatrix = GetElementMatrix(LayerHeatTransferMatrices, Rsi, Rse);
            DynamicRValue = GetDynamicRValue(ElementMatrix);
            DynamicUValue = GetDynamicUValue(DynamicRValue);
            DecrementFactor = GetDecrement(DynamicUValue);
            TAD = GetTAD(ElementMatrix);
            TAV = GetTAV(TAD);
            TimeShift = GetTimeShift(ElementMatrix, PeriodDuration);
            TimeShift_i = GetTimeShift(ElementMatrix, PeriodDuration, "i");
            TimeShift_e = GetTimeShift(ElementMatrix, PeriodDuration, "e");
            ArealHeatCapacity_i = GetArealHeatCapacity(ElementMatrix, PeriodDuration, "i");
            ArealHeatCapacity_e = GetArealHeatCapacity(ElementMatrix, PeriodDuration, "e");
            Test = TestF(ElementMatrix, PeriodDuration);
            EffectiveThermalMass = GetThermalMass(ElementMatrix, PeriodDuration);
        }

        private List<HeatTransferMatrix> GetLayerMatrices(List<Layer> layers, int periodDuration)
        {
            List <HeatTransferMatrix> list = new List<HeatTransferMatrix>();
            foreach (Layer layer in layers)
            {
                // Eindringtiefe δ [m]: Delta ist diejenige Tiefe in einem halbunendlichen Baustoff, bei der die Temperaturschwankung auf 1/e des Wertes der Oberflächentemperaturschwankung abgeklungen ist:
                double delta = Math.Sqrt((layer.Material.ThermalConductivity * periodDuration) / Convert.ToDouble(layer.Material.BulkDensity * layer.Material.SpecificHeatCapacity * Math.PI));
                // ξ [-]: Xi ist das Verhältnis von Schichtdicke zu Eindringtiefe
                double xi = (layer.LayerThickness / 100) / delta;
                list.Add(HeatTransferMatrix.CreateLayerMatrix(delta, xi, layer.Material.ThermalConductivity));
            }
            return list;
        }

        private HeatTransferMatrix GetElementMatrix(List<HeatTransferMatrix> layerMatrices, double rsi, double rse)
        {
            // Add interior environment Matrix to the element
            HeatTransferMatrix elementMatrix = HeatTransferMatrix.CreateEnvironmentMatrix(rsi);

            // Mulitply every Layer HeatTransferMatrix Z1*Z2*Z3*....
            for (int i = 0; i < layerMatrices.Count; i++)
            {
                elementMatrix = MultiplyMatrices(elementMatrix, layerMatrices[i]);
            }

            // Multiply Last Layer Matrix with exterior environment Matrix
            HeatTransferMatrix envMatrix_e = HeatTransferMatrix.CreateEnvironmentMatrix(rse);

            return MultiplyMatrices(elementMatrix, envMatrix_e);
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

        // Temperaturamplitudendämpfung υ [-]
        // Verhältnis zwischen den Amplituden der Aussenlufttemperatur und denjenigen der inneren Wandoberflächentemperatur
        private double GetTAD(HeatTransferMatrix elementMatrix)
        {
            return Math.Round(Complex.Abs(elementMatrix.Z22),1);
        }

        private double GetTAV(double tad)
        {
            return Math.Round(1 / tad, 3);
        }

        private double GetTimeShift(HeatTransferMatrix elementMatrix, int periodDuration, string? side = null)
        {
            if (side == null)
            {
                Complex Y12 = Complex.Divide(1, elementMatrix.Z12);
                double result_s = (periodDuration * Y12.Phase) / (2 * Math.PI) - (periodDuration/2);
                double result_h = result_s / 3600;
                return Math.Round(result_h, 2);
            }
            else if (side == "i")
            {
                Complex Y11 = -1 * Complex.Divide(elementMatrix.Z11, elementMatrix.Z12);
                double result_s = (periodDuration * Y11.Phase) / (2 * Math.PI);
                double result_h = result_s / 3600;
                return Math.Round(result_h, 2);
            }
            else if (side == "e")
            {
                Complex Y22 = -1 * Complex.Divide(elementMatrix.Z22, elementMatrix.Z12);
                double result_s = (periodDuration * Y22.Phase) / (2 * Math.PI);
                double result_h = result_s / 3600;
                return Math.Round(result_h, 2);
            }
            else
            {
                return 0;
            }
        }

        // flächenbezogene Wärmekapazitäten κ
        private double GetArealHeatCapacity(HeatTransferMatrix elementMatrix, int periodDuration, string side = "i")
        {
            double kappa1 = (periodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z11, 1), elementMatrix.Z12)); // Ws/(m²K) = J/(m²K) 
            double kappa2 = (periodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z22, 1), elementMatrix.Z12)); // Ws/(m²K) = J/(m²K)
            return side == "e" ? Math.Round(kappa2/1000, 3) : Math.Round(kappa1/1000, 3); // kJ/(m²K)
        }

        private double GetThermalMass(HeatTransferMatrix elementMatrix, int periodDuration)
        {
            double Y = Complex.Abs(Complex.Divide(elementMatrix.Z11, elementMatrix.Z12)); // W/(m²K)
            double c_0 = 1080; // Normspeicherkapazität = 0,3 Wh/(kgK) = 1080 J/(kgK) = 1080 Ws/(kgK)
            double M = (Y * periodDuration) / (2 * Math.PI * c_0);
            return Math.Round(M, 2);
        }

        private double TestF(HeatTransferMatrix elementMatrix, int periodDuration)
        {
            double Y11 = Complex.Abs(Complex.Divide(elementMatrix.Z11, elementMatrix.Z12)); // W/(m²K)
            double res = (Y11 * periodDuration) / (2 * Math.PI); // Ws/(m²K) = J/(m²K)
            return Math.Round(res/1000, 2); // kJ/(m²K)
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
            HeatTransferMatrix matrix = new HeatTransferMatrix()
            {
                Z11 = new Complex(Math.Cosh(xi) * Math.Cos(xi), Math.Sinh(xi) * Math.Sin(xi)),
                Z22 = new Complex(Math.Cosh(xi) * Math.Cos(xi), Math.Sinh(xi) * Math.Sin(xi)),
                Z12 = factorZ12 * new Complex(Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi), Math.Cosh(xi) * Math.Sin(xi) - Math.Sinh(xi) * Math.Cos(xi)),
                Z21 = factorZ21 * new Complex(Math.Sinh(xi) * Math.Cos(xi) - Math.Cosh(xi) * Math.Sin(xi), Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi))
            };
            return matrix;
        }
        // Creates Environment Border Matrix: If a layer side borders any other environment, instead of direct layer to layer contact: e.g. air layer oder outside air
        public static HeatTransferMatrix CreateEnvironmentMatrix(double rValue)
        {
            HeatTransferMatrix matrix = new HeatTransferMatrix()
            {
                Z11 = 1,
                Z12 = -rValue,
                Z22 = 1,
                Z21 = 0,
            };
            return matrix;
        }
    }
}
