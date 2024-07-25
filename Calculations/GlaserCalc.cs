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
    public class GlaserCalc : TemperatureCurveCalc
    {
        // private fields as Instance Variables
        public double RelFi { get; set; }
        public double RelFe { get; set; }

        // public fields as Properties
        public double PhiMax { get; private set; }
        public double TaupunktMax_i { get; private set; }
        public double P_sat_i { get; private set; }
        public double P_sat_e { get; private set; }
        public SortedDictionary<double, double> LayerPsat { get; private set; } = new SortedDictionary<double, double>(); // Key: Position in m from inner to outer side (0 m), Value: corresponding P_sat in Pa
        public SortedDictionary<double, double> LayerP { get; private set; } = new SortedDictionary<double, double>(); // Key: Position in m from inner to outer side (0 m), Value: corresponding P in Pa
        public new bool IsValid { get; private set; }

        // (Instance-) Constructor
        public GlaserCalc() {}
        public GlaserCalc(Element element, double rsi, double rse, double ti, double te, double relFi, double relFe) : base(element, rsi, rse, ti, te)
        {
            if (element.Layers.Count == 0 || element is null) return;

            RelFi = Math.Max(0, relFi);
            RelFe = Math.Max(0, relFe);

            CalculateGlaser();
        }
        public void CalculateGlaser()
        {
            try
            {
                // Calculated parameters (private setter)
                PhiMax = GetMaxRelFi(Ti, Te, FRsi);             // Gl. 3-3; S.37
                TaupunktMax_i = GetMaxTaupunkt_i(Ti, RelFi);   // Gl. 2.21; S.365. Taupunkttemperatur
                P_sat_i = P_sat(Ti);                            // Gl. 2.4; S.164. Sättigungsdampfdruck innen (Luft)
                P_sat_e = P_sat(Te);                            // Gl. 2.4; S.164.  Sättigungsdampfdruck außen (Luft)
                LayerPsat = GetLayerPsat();                       // Gl. 2.4; S.164. Sättigungsdampfdrücke für jede Schichtgrenze
                LayerP = GetLayerP(RelFi, RelFe, Ti, Te, Element.SdThickness); // Gl. 2.3; S.164, Wasserdampfpartialdruck (innen und außen)
                IsValid = true;
                Logger.LogInfo($"Successfully calculated Glaser values of Element: {Element}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error calculating Glaser values of Element: {Element}, {ex.Message}");
            }
        }

        // Methods
        private SortedDictionary<double, double> GetLayerPsat()
        {
            //Dictionary is not ordered: Instead use List as ordered collection
            var p_sat_List = new SortedDictionary<double, double>(); //new Methode(): Konstruktoren aufruf
            
            //Starting from inner side
            double widthPosition = 0;
            for (int i = 0; i < LayerTemps.Count; i++)
            {
                double currentValue = P_sat(LayerTemps.ElementAt(i).Value);
                //double currentValue = P_sat(LayerTemps[i]);
                p_sat_List.Add(widthPosition, currentValue);

                if (i == RelevantLayers.Count)
                    break; //avoid index out of range exception on Layers[i]: Has 1 less item than in LayerTemps[i]

                widthPosition += RelevantLayers[i].Sd_Thickness;
            }
            return p_sat_List;

            /*if (Math.Round(widthPosition, 3) == TotalSdWidth)
                return p_sat_List;
            else throw new ArgumentOutOfRangeException("calculation failed: sd_width doesn't add up!");*/
        }

        //Bestimmung des Wasserdampfpartialdrucks innen und außen. Gleichung 2.3 umgestellt nach p
        private SortedDictionary<double, double> GetLayerP(double relFi, double relFe, double ti, double te, double sdThickness)
        {
            if (sdThickness == 0) return new SortedDictionary<double, double>();

            double pi = Math.Round((relFi / 100) * P_sat(ti), 2);
            double pe = Math.Round((relFe / 100) * P_sat(te), 2);

            return new SortedDictionary<double, double>
            {
                { 0, pi },
                { sdThickness, pe }
            };
        }
        private double P_sat(double temperature)
        {
            double a = (temperature < 0) ? 4.689 : 288.68;
            double b = (temperature < 0) ? 1.486 : 1.098;
            double n = (temperature < 0) ? 12.3 : 8.02;
            double p_sat = a * Math.Pow(b + (temperature / 100), n);
            return Math.Round(p_sat, 2);
        }

        // public static, avoid full instance creation process if only a single Value needs to be Calculated 
        public static double GetMaxRelFi(double ti, double te, double fRsi) //maximal zulässige Raumluftfeuchte
        {
            /* if (FRsi * (Ti - Te) >= 0 && FRsi * (Ti - Te) <= 30)
             {
                 double phiMax = 0.8 * Math.Pow((109.8 + FRsi * (Ti - Te) + Te) / (109.8 + Ti), 8.02) * 100;
                 return Math.Round(phiMax, 2);
             }
             throw new ArgumentException("Randbedingung zur Berechnung nicht erfüllt."); //TODO Rechnung erlauben, jedoch Hinweis entsprechend einblenden
            */
            double phiMax = 0.8 * Math.Pow((109.8 + fRsi * (ti - te) + te) / (109.8 + ti), 8.02) * 100;
            return Math.Round(phiMax, 2);
        }
        public static double GetMaxTaupunkt_i(double ti, double rel_fi) // Taupunkttemperatur Luft innenseite
        {
            if (ti < 0)
            {
                // Sublimationskurve 
            }
            double theta_T_i = Math.Pow(rel_fi / 100, 0.1247) * (109.8 + ti) - 109.8;
            return Math.Round(theta_T_i, 3);
        }
    }
}
