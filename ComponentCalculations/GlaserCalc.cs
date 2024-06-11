using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.ComponentCalculations
{
    /*
     * When creating this as a 'new' Instance, all fields will auto calculate the values (using the current user envVars)
     * 
     * When using the static Methods from outside this class, a single value can be calculated without creating full class instance 
     */
    public class GlaserCalc : StationaryTempCalc
    {
        // private fields as Instance Variables
        private readonly double _rel_Fi = UserSaved.Rel_Fi;
        private readonly double _rel_Fe = UserSaved.Rel_Fe;

        // public fields as Properties
        public double PhiMax { get; private set; }
        public double TaupunktMax_i { get; private set; }
        public double P_sat_i { get; private set; }
        public double P_sat_e { get; private set; }
        public List<KeyValuePair<double, double>> LayerPsat { get; private set; } = new List<KeyValuePair<double, double>>(); // Key: Position in m from inner to outer side (0 m), Value: corresponding P_sat in Pa
        public List<KeyValuePair<double, double>> LayerP { get; private set; } = new List<KeyValuePair<double, double>>(); // Key: Position in m from inner to outer side (0 m), Value: corresponding P in Pa
        public new bool IsValid { get; private set; }

        // (Instance-) Constructor
        public GlaserCalc() : base() {}
        public GlaserCalc(Element element) : base(element)
        {
            if (element.Layers.Count == 0) return;

            // Calculated parameters (private setter)
            PhiMax = GetMaxRelFi(_ti, _te, FRsi);             // Gl. 3-3; S.37
            TaupunktMax_i = GetMaxTaupunkt_i(_ti, _rel_Fi);   // Gl. 2.21; S.365. Taupunkttemperatur
            P_sat_i = P_sat(_ti);                            // Gl. 2.4; S.164. Sättigungsdampfdruck innen (Luft)
            P_sat_e = P_sat(_te);                            // Gl. 2.4; S.164.  Sättigungsdampfdruck außen (Luft)
            LayerPsat = GetLayerPsat();                       // Gl. 2.4; S.164. Sättigungsdampfdrücke für jede Schichtgrenze
            LayerP = GetLayerP(_rel_Fi, _rel_Fe, _ti, _te, Element.SdThickness); // Gl. 2.3; S.164, Wasserdampfpartialdruck (innen und außen)
            IsValid = true;
        }

        // Methods
        private List<KeyValuePair<double, double>> GetLayerPsat()
        {
            //Dictionary is not ordered: Instead use List as ordered collection
            List<KeyValuePair<double, double>> p_sat_List = new List<KeyValuePair<double, double>>(); //new Methode(): Konstruktoren aufruf

            //Starting from inner side
            double widthPosition = 0;
            for (int i = 0; i < LayerTemps.Count; i++)
            {
                double currentValue = P_sat(LayerTemps[i].Value);
                p_sat_List.Add(new KeyValuePair<double, double>(widthPosition, currentValue));

                if (i == Element.Layers.Count)
                    break; //avoid index out of range exception on Layers[i]: Has 1 less item than in LayerTemps[i]

                widthPosition += Element.Layers[i].Sd_Thickness;
            }
            return p_sat_List;

            /*if (Math.Round(widthPosition, 3) == TotalSdWidth)
                return p_sat_List;
            else throw new ArgumentOutOfRangeException("calculation failed: sd_width doesn't add up!");*/
        }

        //Bestimmung des Wasserdampfpartialdrucks innen und außen. Gleichung 2.3 umgestellt nach p
        private List<KeyValuePair<double, double>> GetLayerP(double relFi, double relFe, double ti, double te, double sdThickness)
        {
            double pi = Math.Round((relFi / 100) * P_sat(ti), 1);
            double pe = Math.Round((relFe / 100) * P_sat(te), 1);

            return new List<KeyValuePair<double, double>>()
            {
                new KeyValuePair<double, double>(0, pi),
                new KeyValuePair<double, double>(sdThickness, pe)
            };
        }
        private double P_sat(double temperature)
        {
            double a = (temperature < 0) ? 4.689 : 288.68;
            double b = (temperature < 0) ? 1.486 : 1.098;
            double n = (temperature < 0) ? 12.3 : 8.02;
            double p_sat = a * Math.Pow(b + (temperature / 100), n);
            return Math.Round(p_sat, 1);
        }

        // public static, avoid full instance creation process if only a single Value needs to be Calculated 
        public static double GetMaxRelFi(double ti, double te, double fRsi) //maximal zulässige Raumluftfeuchte
        {
            /* if (FRsi * (Ti - Te) >= 0 && FRsi * (Ti - Te) <= 30)
             {
                 double phiMax = 0.8 * Math.Pow((109.8 + FRsi * (Ti - Te) + Te) / (109.8 + Ti), 8.02) * 100;
                 return Math.Round(phiMax, 1);
             }
             throw new ArgumentException("Randbedingung zur Berechnung nicht erfüllt."); //TODO Rechnung erlauben, jedoch Hinweis entsprechend einblenden
            */
            double phiMax = 0.8 * Math.Pow((109.8 + fRsi * (ti - te) + te) / (109.8 + ti), 8.02) * 100;
            return Math.Round(phiMax, 1);
        }
        public static double GetMaxTaupunkt_i(double ti, double rel_fi) // Taupunkttemperatur Luft innenseite
        {
            if (ti < 0)
            {
                // Sublimationskurve 
            }
            double theta_T_i = Math.Pow(rel_fi / 100, 0.1247) * (109.8 + ti) - 109.8;
            return Math.Round(theta_T_i, 2);
        }
    }
}
