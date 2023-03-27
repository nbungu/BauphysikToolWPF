using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class StationaryTempCalc
    {
        // (Instance-) Variables and encapsulated properties - Called before Constructor
        public Element Element { get; private set; } // Access is limited to the containing class or types derived from the containing class within the current assembly
        public double RTotal { get; private set; } = 0;
        public double UValue { get; private set; } = 0;
        public double QValue { get; private set; } = 0;
        public double FRsi { get; private set; } = 0;
        public double Tsi_min { get; private set; } = 0;
        public double Tsi { get; private set; } = 0;
        public double Tse { get; private set; } = 0;
        public List<KeyValuePair<double, double>> LayerTemps { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding Temperature in °C
        public double Ti { get; }
        public double Te { get; }
        public double Rsi { get; }
        public double Rse { get; }
        public double Rel_Fi { get; }
        public double Rel_Fe { get; }

        // (Instance-) Constructor
        public StationaryTempCalc(Element element, Dictionary<string, double> userEnvVars)
        {
            if (element is null || element.Layers.Count == 0)
                return;

            if (userEnvVars is null)
                return;

            // Assign constuctor parameter values
            Element = element;
            Ti = userEnvVars["Ti"];
            Te = userEnvVars["Te"];
            Rsi = userEnvVars["Rsi"];
            Rse = userEnvVars["Rse"];
            Rel_Fi = userEnvVars["Rel_Fi"];
            Rel_Fe = userEnvVars["Rel_Fe"];

            // Calculated parameters (private setter)
            RTotal = GetRTotal();           // Gl. 2-55; S.28
            UValue = GetUValue();           // Gl. 2-57; S.29
            QValue = GetqValue();           // Gl. 2-65; S.31
            LayerTemps = GetLayerTemps();   // Bsp. S.33
            FRsi = GetfRsiValue();          // Gl. 3-1; S.36. Schimmelwahrscheinlichkeit
            Tsi_min = GetTsiMin();          // Gl. 3-1; S.36 umgestellt nach Tsi für fRsi = 0,7. Schimmelwahrscheinlichkeit
            Tsi = LayerTemps.FirstOrDefault().Value;
            Tse = LayerTemps.LastOrDefault().Value;
        }

        // Methods
        private double GetRTotal()
        {
            return Math.Round(Rsi + Element.RValue + Rse, 2);
        }

        private double GetUValue()
        {
            return Math.Round(Math.Pow(RTotal, -1), 3);
        }

        private double GetqValue()
        {
            return Math.Round(UValue * (Ti - Te), 3);
        }

        private List<KeyValuePair<double, double>> GetLayerTemps()
        {
            // Dictionaries are not ordered: Instead use List as ordered collection
            List<KeyValuePair<double, double>> temp_List = new List<KeyValuePair<double, double>>();

            // first tempValue (Tsi)
            double tsi = Math.Round(Ti - Rsi * QValue, 2);
            temp_List.Add(new KeyValuePair<double, double>(0, tsi)); // key, value

            // Starting from inner side
            double widthPosition = 0; // cm
            for (int i = 0; i < Element.Layers.Count; i++)
            {
                if (!Element.Layers[i].IsEffective)
                    break;
                widthPosition += Element.Layers[i].LayerThickness;
                double tempValue = Math.Round(temp_List.ElementAt(i).Value - Element.Layers[i].R_Value * QValue, 2);
                temp_List.Add(new KeyValuePair<double, double>(widthPosition, tempValue));
            }
            return temp_List;

            // TODO implement 
            /*if (widthPosition == 0)
                return temp_List;
            else throw new ArgumentOutOfRangeException("calculation failed");*/
        }
        private double GetfRsiValue()
        {
            // Durch 0 teilen abfangen
            if (Ti - Te == 0)
                return 0;

            return Math.Round((LayerTemps.First().Value - Te) / (Ti - Te), 2);
        }
        private double GetTsiMin()
        {
            return Math.Round(0.7*(Ti-Te)+Te, 2);
        }

        /* Hardcoded example:
         double tsiVal = tiVal - SurfaceResistance.selectedRsi * qValue;
        Tsi_Value.Text = "θsi [°C]: " + tsiVal.ToString();

		double t1_2Val = tsiVal - (layers[0].R_Value) * qValue;
		T1_2_Value.Text = "θ1/2 [°C]: " + t1_2Val.ToString();

		double t2_3Val = t1_2Val - (layers[1].R_Value) * qValue;
		T2_3_Value.Text = "θ2/3 [°C]: " + t2_3Val.ToString();

		double t3_4Val = t2_3Val- (layers[2].R_Value) * qValue;
		T3_4_Value.Text = "θ3/4 [°C]: " + t3_4Val.ToString();

		double tseVal = t3_4Val - (layers[3].R_Value) * qValue;
		Tse_Value.Text = "θse [°C]: " + tseVal.ToString();
         */
    }
}
