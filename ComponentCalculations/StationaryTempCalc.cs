using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.ComponentCalculations
{
    /*
     * When creating this as a 'new' Instance, all fields will auto calculate the values (using the current user envVars)
     * 
     * When using the static Methods from outside this class, a single value can be calculated without creating full class instance 
     */
    public class StationaryTempCalc
    {
        // protected fields as Instance Variables (= private, but accessible from child class (Inheritance))
        protected double _ti = UserSaved.Ti;
        protected double _te = UserSaved.Te;
        protected double _rsi = UserSaved.Rsi;
        protected double _rse = UserSaved.Rse;

        // public fields as Properties
        public Element Element { get; private set; }  // Access is limited to the containing class or types derived from the containing class within the current assembly
        public double RTotal { get; private set; }
        public double UValue { get; private set; }
        public double QValue { get; private set; }
        public double FRsi { get; private set; }
        public double Tsi_min { get; private set; }
        public double Tsi { get; private set; }
        public double Tse { get; private set; }
        public List<KeyValuePair<double, double>> LayerTemps { get; private set; } // Key: Position in cm from inner to outer side (0 cm), Value: corresponding Temperature in °C

        // Constructor
        public StationaryTempCalc(Element element)
        {
            if (element == null || element.Layers.Count == 0)
            {
                Element = new Element();
                LayerTemps = new List<KeyValuePair<double, double>>();
                return;
            }

            // Assign constuctor parameter values
            Element = element;

            // Calculated parameters (private setter)
            RTotal = GetRTotal(Element.RValue, _rsi, _rse);                 // Gl. 2-55; S.28
            UValue = GetUValue(RTotal);                                     // Gl. 2-57; S.29
            QValue = GetqValue(UValue, _ti, _te);                           // Gl. 2-65; S.31
            LayerTemps = GetLayerTemps(Element.Layers, _ti, _rsi, QValue);  // Bsp. S.33
            Tsi = LayerTemps.FirstOrDefault().Value;
            Tse = LayerTemps.LastOrDefault().Value;
            FRsi = GetfRsiValue(Tsi, _ti, _te);                             // Gl. 3-1; S.36. Schimmelwahrscheinlichkeit
            Tsi_min = GetTsiMin(_ti, _te);                                  // Gl. 3-1; S.36 umgestellt nach Tsi für fRsi = 0,7. Schimmelwahrscheinlichkeit
        }

        // public static, avoid full instance creation process if only a single Value needs to be Calculated 
        public static double GetRTotal(double rValue, double rsi, double rse)
        {
            return Math.Round(rsi + rValue + rse, 2);
        }

        public static double GetUValue(double rTotal)
        {
            return Math.Round(1 / rTotal, 2);
        }

        public static double GetqValue(double uValue, double ti, double te)
        {
            return Math.Round(uValue * (ti - te), 3);
        }

        public static List<KeyValuePair<double, double>> GetLayerTemps(List<Layer> layers, double ti, double rsi, double qValue)
        {
            // Dictionaries are not ordered: Instead use List as ordered collection
            List<KeyValuePair<double, double>> tempList = new List<KeyValuePair<double, double>>();

            // first tempValue (Tsi)
            double tsi = Math.Round(ti - rsi * qValue, 2);
            tempList.Add(new KeyValuePair<double, double>(0, tsi)); // key, value

            // Starting from inner side
            double widthPosition = 0; // cm
            for (int i = 0; i < layers.Count; i++)
            {
                if (!layers[i].IsEffective)
                    break;
                widthPosition += layers[i].LayerThickness;
                double tempValue = Math.Round(tempList.ElementAt(i).Value - layers[i].R_Value * qValue, 2);
                tempList.Add(new KeyValuePair<double, double>(widthPosition, tempValue));
            }
            return tempList;

            // TODO implement 
            /*if (widthPosition == 0)
                return temp_List;
            else throw new ArgumentOutOfRangeException("calculation failed");*/
        }
        public static double GetfRsiValue(double tsi, double ti, double te)
        {
            // Durch 0 teilen abfangen
            if (ti - te == 0)
                return 0;

            return Math.Round((tsi - te) / (ti - te), 2);
        }
        public static double GetTsiMin(double ti, double te)
        {
            return Math.Round(0.7 * (ti - te) + te, 2);
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
