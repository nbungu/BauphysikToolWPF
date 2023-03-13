using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class GlaserCalc : StationaryTempCalc
    {
        // (Instance-) Variables and encapsulated properties
        public double PhiMax { get; private set; } = 0;
        public double TaupunktMax_i { get; private set; } = 0;
        public double P_sat_i { get; private set; } = 0;
        public double P_sat_e { get; private set; } = 0;
        public List<KeyValuePair<double, double>> LayerPsat { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in m from inner to outer side (0 m), Value: corresponding P_sat in Pa
        public List<KeyValuePair<double, double>> LayerP { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in m from inner to outer side (0 m), Value: corresponding P in Pa

        // (Instance-) Constructor
        public GlaserCalc()
        {
            if (Element.Layers.Count == 0) // inherited class member from StationaryTempCalc
                return;

            // Calculated parameters (private setter)
            PhiMax = GetMaxRelFi(Ti, Te, FRsi);             // Gl. 3-3; S.37
            TaupunktMax_i = GetMaxTaupunkt_i(Ti, Rel_Fi);   // Gl. 2.21; S.365
            LayerPsat = GetLayerPsat();                     // Gl. 2.4; S.164
            LayerP = GetLayerP();                           // Gl. 2.3; S.164
            P_sat_i = P_sat(UserSaved.Ti);                  // Gl. 2.4; S.164
            P_sat_e = P_sat(UserSaved.Te);                  // Gl. 2.4; S.164
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
        private List<KeyValuePair<double, double>> GetLayerP()
        {
            double pi = Math.Round((Rel_Fi / 100) * P_sat(Ti), 1);
            double pe = Math.Round((Rel_Fe / 100) * P_sat(Te), 1);
            List<KeyValuePair<double, double>> p_List = new List<KeyValuePair<double, double>>()
            {
                new KeyValuePair<double, double>(0, pi),
                new KeyValuePair<double, double>(Element.SdThickness, pe)
            };
            return p_List;
        }
        private double P_sat(double temperature)
        {
            double a = (temperature < 0) ? 4.689 : 288.68;
            double b = (temperature < 0) ? 1.486 : 1.098;
            double n = (temperature < 0) ? 12.3 : 8.02;
            double p_sat = a * Math.Pow(b + (temperature / 100), n);
            return Math.Round(p_sat, 1);
        }

        //maximal zulässige Raumluftfeuchte
        private double GetMaxRelFi(double ti, double te, double fRsi) 
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

        // Taupunkttemperatur Luft innenseite
        private double GetMaxTaupunkt_i(double ti, double rel_fi)
        {
            if (ti < 0)
            {
                // Sublimationskurve 
            }
            double theta_T_i = Math.Pow(rel_fi/100, 0.1247) * (109.8 + ti) - 109.8;
            return Math.Round(theta_T_i,2);
        }
    }
}
