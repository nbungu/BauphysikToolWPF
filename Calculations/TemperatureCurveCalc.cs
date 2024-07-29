using BauphysikToolWPF.Models;
using BT.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.Calculations
{
    /*
     * When creating this as a 'new' Instance, all fields will auto calculate the values (using the current user envVars)
     * 
     * When using the static Methods from outside this class, a single value can be calculated without creating full class instance 
     */
    public class TemperatureCurveCalc : ThermalValuesCalc
    {
        // Calculated Properties
        public double FRsi { get; private set; }
        public double TsiMin { get; private set; }
        public double Tsi { get; private set; }
        public double Tse { get; private set; }
        public SortedDictionary<double, double> LayerTemps { get; private set; } = new SortedDictionary<double, double>(); // Key: Position in cm from inner to outer side (0 cm), Value: corresponding Temperature in °C

        // Constructors
        public TemperatureCurveCalc() : base() {}
        public TemperatureCurveCalc(Element element, double rsi, double rse, double ti, double te) : base(element, rsi, rse, ti, te)
        {
            if (element.Layers.Count == 0 || element is null) return;
            
            CalculateTemperatureCurve();
        }
        
        public void CalculateTemperatureCurve()
        {
            CalculateLayerTemps();  // Bsp. S.33
            CalculatefRsiValue();   // Gl. 3-1; S.36. Schimmelwahrscheinlichkeit
            CalculateTsiMin();      // Gl. 3-1; S.36 umgestellt nach Tsi für fRsi = 0,7. Schimmelwahrscheinlichkeit
        }
        
        private void CalculateLayerTemps()
        {
            try
            {
                // Dictionaries are not ordered: Instead use List as ordered collection
                var tempList = new SortedDictionary<double, double>();

                // first tempValue (Tsi)
                double tsi = Math.Round(Ti - Rsi * QValue, 3);
                tempList.Add(0, tsi); // key, value

                // Starting from inner side
                double widthPosition = 0; // cm
                for (int i = 0; i < RelevantLayers.Count; i++)
                {
                    widthPosition += RelevantLayers[i].Thickness;
                    if (RelevantLayers[i].Thickness <= 0) continue;

                    double tempValue = Math.Round(tempList.ElementAt(i).Value - RelevantLayers[i].R_Value * QValue, 3);
                    tempList.Add(widthPosition, tempValue);
                }
                LayerTemps = tempList;
                Tsi = tsi;
                Tse = LayerTemps.Last().Value;
                Logger.LogInfo($"Successfully calculated Layer Temparatures of Element: {Element}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error calculating Layer Temparatures of Element: {Element}, {ex.Message}");
            }

        }
        private void CalculatefRsiValue()
        {
            // Durch 0 teilen abfangen
            if (Ti - Te == 0) FRsi = 0;
            else FRsi = Math.Round((Tsi - Te) / (Ti - Te), 3);
        }
        private void CalculateTsiMin()
        {
            TsiMin = Math.Round(0.7 * (Ti - Te) + Te, 3);
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
