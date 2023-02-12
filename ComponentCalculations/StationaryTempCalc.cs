using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class StationaryTempCalc
    {
        // Static Class Variables

        public static readonly double FRsiMin = 0.7;

        public static readonly double TsiMin = 12.6;

        // (Instance-) Variables and encapsulated properties - Called before Constructor
        public Element Element { get; private set; } = FO0_LandingPage.SelectedElement; // Access is limited to the containing class or types derived from the containing class within the current assembly
        public double TotalElementWidth { get; private set; } = 0;
        public double SumOfLayersR { get; private set; } = 0;
        public double RTotal { get; private set; } = 0;
        public double UValue { get; private set; } = 0;
        public double QValue { get; private set; } = 0;
        public double FRsi { get; private set; } = 0;
        public double PhiMax { get; private set; } = 0;
        public double Tsi_min { get; private set; } = 0;
        public List<KeyValuePair<double, double>> LayerTemps { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding Temperature in °C
        public double Ti { get; } = UserSaved.Ti;
        public double Te { get; } = UserSaved.Te;
        public double Rsi { get; } = UserSaved.Rsi;
        public double Rse { get; } = UserSaved.Rse;
        public double Rel_Fi { get; } = UserSaved.Rel_Fi;
        public double Rel_Fe { get; } = UserSaved.Rel_Fe;

        // (Instance-) Constructor
        public StationaryTempCalc()
        {
            if (Element.Layers.Count == 0)
                return;

            // Calculated parameters (private setter)
            TotalElementWidth = Element.ElementThickness_cm;
            SumOfLayersR = Element.ElementRValue;
            RTotal = GetRTotal();           // Gl. 2-55; S.28
            UValue = GetUValue();           // Gl. 2-57; S.29
            QValue = GetqValue();           // Gl. 2-65; S.31
            LayerTemps = GetLayerTemps();   // Bsp. S.33
            FRsi = GetfRsiValue();          // Gl. 3-1; S.36
            PhiMax = GetMaxRelF();          // Gl. 3-3; S.37
            Tsi_min = GetTsiMin();          // Gl. 3-1; S.36 umgestellt nach Tsi für fRsi = 0,7
        }

        // Methods
        private double GetRTotal()
        {
            return Math.Round(Rsi + SumOfLayersR + Rse, 2);
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
            double tsi = Math.Round(Ti - Rsi * QValue,2);
            temp_List.Add(new KeyValuePair<double, double>(0, tsi)); // key, value

            // Starting from inner side
            double widthPosition = 0; // cm
            for (int i = 0; i < Element.Layers.Count; i++)
            {
                widthPosition += Element.Layers[i].LayerThickness;
                double tempValue = temp_List.ElementAt(i).Value - Element.Layers[i].R_Value * QValue;
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

        private double GetMaxRelF() //maximal zulässige Raumluftfeuchte
        {
           /* if (FRsi * (Ti - Te) >= 0 && FRsi * (Ti - Te) <= 30)
            {
                double phiMax = 0.8 * Math.Pow((109.8 + FRsi * (Ti - Te) + Te) / (109.8 + Ti), 8.02) * 100;
                return Math.Round(phiMax, 1);
            }
            throw new ArgumentException("Randbedingung zur Berechnung nicht erfüllt."); //TODO Rechnung erlauben, jedoch Hinweis entsprechend einblenden
           */
            double phiMax = 0.8 * Math.Pow((109.8 + FRsi * (Ti - Te) + Te) / (109.8 + Ti), 8.02) * 100;
            return Math.Round(phiMax, 1);
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
