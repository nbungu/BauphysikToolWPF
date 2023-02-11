using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class GlaserCalc : StationaryTempCalc
    {
        // (Instance-) Variables and encapsulated properties
        public double TotalSdWidth { get; private set; } = 0;
        public List<KeyValuePair<double, double>> LayerPsat { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding P_sat in Pa
        public List<KeyValuePair<double, double>> LayerP { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding P in Pa

        // (Instance-) Constructor
        public GlaserCalc()
        {
            if (Element.Layers.Count == 0) // inherited class member from StationaryTempCalc
                return;

            // Calculated parameters (private setter)
            TotalSdWidth = Element.ElementSdThickness;  // Gl. 5.2; S.246
            LayerPsat = GetLayerPsat();                 // Gl. 2.4; S.164
            LayerP = GetLayerP();                       // Gl. 2.3; S.164
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
                new KeyValuePair<double, double>(TotalSdWidth, pe)
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

    }
}
