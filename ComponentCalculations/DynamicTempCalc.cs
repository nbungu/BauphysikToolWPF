using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BauphysikToolWPF.ComponentCalculations
{
    // Berechnungen nach DIN EN ISO 13786:2018-04
    public class DynamicTempCalc
    {
        private Element Element { get; set; }
        private int PeriodDuration { get; set; } = 86400;
        private List<HeatTransferMatrix> LayerHeatTransferMatrices { get; set; }
        private HeatTransferMatrix ElementMatrix { get; set; }
        public double TAD { get; set; }
        public double TAV { get; set; }
        public double PhaseDelay { get; set; } // [h]
        public double AreaHeatCapacity { get; set; } // [J/(m²K)]

        public DynamicTempCalc(Element element)
        {
            Element = element;

            LayerHeatTransferMatrices = GetLayerMatrices();
            ElementMatrix = GetElementMatrix(LayerHeatTransferMatrices);
            TAD = GetTAD(ElementMatrix);
            TAV = GetTAV(TAD);
            PhaseDelay = GetPhaseDelay(ElementMatrix, PeriodDuration);
            AreaHeatCapacity = GetAreaHeatCapacity(ElementMatrix, PeriodDuration);
        }

        private List<HeatTransferMatrix> GetLayerMatrices()
        {
            List <HeatTransferMatrix> list = new List<HeatTransferMatrix>();
            foreach (Layer layer in Element.Layers)
            {
                // Eindringtiefe δ [m]: Delta ist diejenige Tiefe in einem halbunendlichen Baustoff, bei der die Temperaturschwankung auf 1/e des Wertes der Oberflächentemperaturschwankung abgeklungen ist:
                double delta = Math.Sqrt((layer.Material.ThermalConductivity * PeriodDuration) / Convert.ToDouble(layer.Material.BulkDensity * layer.Material.SpecificHeatCapacity * Math.PI));
                // ξ [-]: Xi ist das Verhältnis von Schichtdicke zu Eindringtiefe
                double xi = (layer.LayerThickness / 100) / delta;
                list.Add(new HeatTransferMatrix(delta, xi, layer.Material.ThermalConductivity));
            }
            return list;
        }

        private HeatTransferMatrix GetElementMatrix(List<HeatTransferMatrix> layerMatrices)
        {
            // TODO
            HeatTransferMatrix envBorder_i = new HeatTransferMatrix(envVar: UserSaved.Rsi);
            HeatTransferMatrix envBorder_e = new HeatTransferMatrix(envVar: UserSaved.Rse);

            // Add Starting Value for the Matrix
            HeatTransferMatrix elementMatrix = envBorder_i;


            for (int i = 0; i < layerMatrices.Count; i++)
            {
                HeatTransferMatrix result = MultiplyMatrices(elementMatrix, layerMatrices[i]);
                elementMatrix = result;
            }
            // End Value 
            elementMatrix = MultiplyMatrices(elementMatrix, envBorder_e);

            return elementMatrix;
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
        private double GetAreaHeatCapacity(HeatTransferMatrix elementMatrix, int periodDuration)
        {
            return (periodDuration / (2 * Math.PI)) * Complex.Abs(Complex.Divide(Complex.Subtract(elementMatrix.Z11, 1), elementMatrix.Z12));
        }
    }
    internal class HeatTransferMatrix
    {
        // Complex values of the Heat Transfer Matrix
        public Complex Z11 { get; set; }
        public Complex Z12 { get; set; }
        public Complex Z22 { get; set; }
        public Complex Z21 { get; set; }

        public HeatTransferMatrix(double? delta = null, double xi = -1, double? lambda = null, double? envVar = null)
        {
            // Creates Layer Heat Transfer Matrix
            if (delta != null && xi != -1 && lambda != null)
            {     
                Z11 = new Complex(Math.Cosh(xi) * Math.Cos(xi), Math.Sinh(xi) * Math.Sin(xi));
                Z22 = Z11;
                double? factorZ12 = -delta / (2 * lambda);
                Z12 = new Complex((factorZ12 ?? 0) * (Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi)), (factorZ12 ?? 0) * (Math.Cosh(xi) * Math.Sin(xi) - Math.Sinh(xi) * Math.Cos(xi)));
                double? factorZ21 = -lambda / delta;
                Z12 = new Complex((factorZ21 ?? 0) * (Math.Sinh(xi) * Math.Cos(xi) - Math.Cosh(xi) * Math.Sin(xi)), (factorZ21 ?? 0) * (Math.Sinh(xi) * Math.Cos(xi) + Math.Cosh(xi) * Math.Sin(xi)));
            } 
            // Creates Environment Border Matrix
            else if (envVar != null)
            {
                Z11 = 1;
                Z12 = -envVar ?? 0;
                Z22 = 1;
                Z21 = 0;
            }
            // Return empty Matrix
            else
            {
                return;
            }
        }
    }
}
