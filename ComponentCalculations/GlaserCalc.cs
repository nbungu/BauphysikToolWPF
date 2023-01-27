using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class GlaserCalc : StationaryTempCalc
    {
        //(Instance-) Variables and encapsulated properties

        private List<Layer> layers = new List<Layer>();
        public List<Layer> Layers //for Validation
        {
            get { return layers; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("null layer list specified");
                layers = value;
            }
        }
        private Dictionary<string,double> envVars = new Dictionary<string, double>();
        public Dictionary<string, double> EnvVars //for Validation
        {
            get { return envVars; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("null envVars dict specified");
                envVars = value;
            }
        }
        public double TotalSdWidth { get; private set; } = 0;
        public List<KeyValuePair<double, double>> LayerPsat { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding P_sat in Pa
        public List<KeyValuePair<double, double>> LayerP { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding P in Pa

        // (Instance-) Constructor
        public GlaserCalc(List<Layer> layers, Dictionary<string, double> envVars) : base(layers, envVars) //parameter aus base class mitnehmen
        {
            //User specified (public setter)
            Layers = layers;
            EnvVars = envVars;
            //Calculated parameters (private setter)
            TotalSdWidth = GetTotalSdWidth();   // Gl. 5.2; S.246
            LayerPsat = GetLayerPsat();         // Gl. 2.4; S.164
            LayerP = GetLayerP();               // Gl. 2.3; S.164
        }

        // Methods
        private double GetTotalSdWidth()
        {
            double width = 0;
            foreach (Layer l in Layers)
            {
                width += l.Sd_Thickness; // sum of sd-values
            }
            return Math.Round(width,3);
        }
        private List<KeyValuePair<double, double>> GetLayerPsat()
        {
            //Dictionary is not ordered: Instead use List as ordered collection
            List<KeyValuePair<double,double>> p_sat_List = new List<KeyValuePair<double,double>>(); //new Methode(): Konstruktoren aufruf

            //Starting from inner side
            double widthPosition = TotalSdWidth;

            for (int i = 0; i<LayerTemps.Count; i++)
            {     
                double currentWidthPosition = Math.Round(widthPosition,3);
                double currentValue = P_sat(LayerTemps[i].Value);
                p_sat_List.Add(new KeyValuePair<double, double>(currentWidthPosition, currentValue));

                if (i == Layers.Count)
                    break; //avoid index out of range exception on Layers[i]: Has 1 less item than in LayerTemps[i]
                widthPosition -= Layers[i].Sd_Thickness;
            }

            if (Math.Round(widthPosition,3) == 0)
                return p_sat_List;
            else throw new ArgumentOutOfRangeException("calculation failed");
        }
        private List<KeyValuePair<double, double>> GetLayerP()
        {
            double pi = Math.Round((Rel_Fi/100) * P_sat(Ti),1);
            double pe = Math.Round((Rel_Fe/100) * P_sat(Te),1);
            List<KeyValuePair<double, double>> p_List = new List<KeyValuePair<double, double>>()
            {
                new KeyValuePair<double, double>(TotalSdWidth, pi),
                new KeyValuePair<double, double>(0, pe)
            };
            return p_List;
        }

        private double P_sat(double temperature)
        {
            double a = (temperature < 0) ? 4.689 : 288.68;
            double b = (temperature < 0) ? 1.486 : 1.098;
            double n = (temperature < 0) ? 12.3 : 8.02;
            double p_sat = a * Math.Pow(b + (temperature / 100), n);
            return Math.Round(p_sat,1);
        }

    }
}
