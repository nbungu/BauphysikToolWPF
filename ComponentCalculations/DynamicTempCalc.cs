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
        public double TAD { get; set; } // [-]
        public double TAV { get; set; } // [-]
        public double PenetrationDepth { get; set; } // [m]
        public double PhaseDelay { get; set; } // [h]
        public double ArealHeatCapacity_i { get; set; } // [kJ/(m²K)]
        public double ArealHeatCapacity_e { get; set; } // [kJ/(m²K)]

        public DynamicTempCalc(Element element)
        {
            Element = element;

            LayerHeatTransferMatrices = GetLayerMatrices(Element.Layers, PeriodDuration);
            ElementMatrix = GetElementMatrix(LayerHeatTransferMatrices, Rsi, Rse);
            TAD = GetTAD(ElementMatrix);
            TAV = GetTAV(TAD);
            PenetrationDepth = GetPenetrationDepth(Element.Layers, PeriodDuration);
            PhaseDelay = GetPhaseDelay(ElementMatrix, PeriodDuration);
            ArealHeatCapacity_i = GetArealHeatCapacity(ElementMatrix, PeriodDuration, "i");
            ArealHeatCapacity_e = GetArealHeatCapacity(ElementMatrix, PeriodDuration, "e");
        }

        private double GetPenetrationDepth(List<Layer> layers, int periodDuration)
        {
            double delta = 0; 
            foreach (Layer layer in layers)
            {
                // Eindringtiefe δ [m]: Delta ist diejenige Tiefe in einem halbunendlichen Baustoff, bei der die Temperaturschwankung auf 1/e des Wertes der Oberflächentemperaturschwankung abgeklungen ist:
                delta += Math.Sqrt((layer.Material.ThermalConductivity * periodDuration) / Convert.ToDouble(layer.Material.BulkDensity * layer.Material.SpecificHeatCapacity * Math.PI));
            }
            return Math.Round(delta, 2);
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

            return MultiplyMatrices(elementMatrix, envMatrix_e); ;
        }
        private HeatTransferMatrix MultiplyMatrices(HeatTransferMatrix Z1, HeatTransferMatrix Z2)
        {
            // TODO multiplikation mit Rsi Rse werten funktioniert nicht
            HeatTransferMatrix result = new HeatTransferMatrix()
            {
                Z11 = Complex.Add(Complex.Multiply(Z1.Z11, Z2.Z11), Complex.Multiply(Z1.Z12, Z2.Z21)),
                Z12 = Complex.Add(Complex.Multiply(Z1.Z11, Z2.Z12), Complex.Multiply(Z1.Z12, Z2.Z22)),
                Z21 = Complex.Add(Complex.Multiply(Z1.Z21, Z2.Z11), Complex.Multiply(Z1.Z22, Z2.Z21)),
                Z22 = Complex.Add(Complex.Multiply(Z1.Z21, Z2.Z12), Complex.Multiply(Z1.Z22, Z2.Z22)),
            };
            return result;
        }

        // Temperaturamplitudendämpfung υ [-]
        // Verhältnis zwischen den Amplituden der Aussenlufttemperatur und denjenigen der inneren Wandoberflächentemperatur
        private double GetTAD(HeatTransferMatrix elementMatrix)
        {
            return Math.Round(elementMatrix.Z11.Magnitude, 3);
        }

        private double GetTAV(double tad)
        {
            return Math.Round(1 / tad, 3);
        }

        private double GetPhaseDelay(HeatTransferMatrix elementMatrix, int periodDuration)
        {
            double result_s = (periodDuration * elementMatrix.Z11.Phase) / (2 * Math.PI);
            double result_h = result_s / 3600;
            return Math.Round(result_h,1);
        }

        // flächenbezogene Wärmekapazitäten κ
        private double GetArealHeatCapacity(HeatTransferMatrix elementMatrix, int periodDuration, string side = "i")
        {
            double kappa1 = (periodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z11, 1), elementMatrix.Z12));
            double kappa2 = (periodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z22, 1), elementMatrix.Z12));
            return side == "e" ? Math.Round(kappa2/1000,2) : Math.Round(kappa1/1000,2);
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
                Z12 = new Complex(factorZ12 * (Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi)), factorZ12 * (Math.Cosh(xi) * Math.Sin(xi) - Math.Sinh(xi) * Math.Cos(xi))),
                Z21 = new Complex(factorZ21 * (Math.Sinh(xi) * Math.Cos(xi) - Math.Cosh(xi) * Math.Sin(xi)), factorZ21 * (Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi)))
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
