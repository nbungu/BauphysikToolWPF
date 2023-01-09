using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class StationaryTempCurve
    {
        //static Class variables

        public static readonly double FRsiMin = 0.7;

        public static readonly double TsiMin = 12.6;

        //(Instance-) Variables and encapsulated properties

        private List<Layer> layers = new List<Layer>();
        public List<Layer> Layers //for Validation
        {     
            get { return layers; }  
            set
            {
                if(value == null)
                    throw new ArgumentNullException("null layer list specified");
                layers = value;
            }
        }
        public double TotalElementWidth { get; private set; } = 0;
        public double SumOfLayersR { get; private set; } = 0;
        public double RTotal { get; private set; } = 0;
        public double UValue { get; private set; } = 0;
        public double QValue { get; private set; } = 0;
        public double FRsi { get; private set; } = 0;
        public double PhiMax { get; private set; } = 0;
        public List<KeyValuePair<double, double>> LayerTemps { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding Temperature in °C
        public double Ti { get; private set; } = UserSaved.Ti.Value;
        public double Te { get; private set; } = UserSaved.Te.Value;
        public double Rsi { get; private set; } = UserSaved.Rsi.Value;
        public double Rse { get; private set; } = UserSaved.Rse.Value;
        public double Rel_Fi { get; private set; } = UserSaved.Rel_Fi.Value;
        public double Rel_Fe { get; private set; } = UserSaved.Rel_Fe.Value;

        // (Instance-) Constructor
        public StationaryTempCurve(List<Layer> layers)
        {
            if (layers.Count == 0)
                return;

            //User specified (public setter)
            Layers = layers;
            //Calculated parameters (private setter)
            TotalElementWidth = GetTotalElementWidth();
            SumOfLayersR = GetLayersR();
            RTotal = GetRTotal();
            UValue = GetUValue();
            QValue = GetqValue();
            LayerTemps = GetLayerTemps();
            FRsi = GetfRsiValue();
            PhiMax = GetMaxRelF();
        }

        // Methods
        private double GetTotalElementWidth()
        {
            double width = 0;
            foreach (Layer l in Layers)
            {
                width += l.LayerThickness;
            }
            return width;
        }

        private double GetLayersR()
        {
            double rLayers = 0;
            foreach (Layer l in Layers)
            {
                rLayers += l.LayerResistance;
            }
            return Math.Round(rLayers,2);
        }

        private double GetRTotal()
        {
            return Math.Round(Rsi + SumOfLayersR + Rse, 2);
        }

        private double GetUValue()
        {
            return Math.Round(Math.Pow(RTotal, -1),3);
        }

        private double GetqValue()
        {
            return Math.Round(UValue * (Ti - Te), 3);
        }

        private List<KeyValuePair<double, double>> GetLayerTemps()
        {
            //Dictionaries are not ordered: Instead use List as ordered collection
            List<KeyValuePair<double,double>> elementTemps = new List<KeyValuePair<double,double>>();

            //Starting from inner side
            double widthPosition = TotalElementWidth;
            double tVal = Ti - Rsi * QValue; // Tsi

            elementTemps.Add(new KeyValuePair<double, double>(widthPosition, tVal)); // key, value

            for (int i = 0; i<Layers.Count; i++)
            {
                double current_widthPosition = widthPosition - Layers[i].LayerThickness;
                double current_tVal = elementTemps.ElementAt(i).Value - Layers[i].LayerResistance * QValue;
                elementTemps.Add(new KeyValuePair<double, double>(current_widthPosition, current_tVal));

                widthPosition = current_widthPosition;
            }

            // Adding Ti at beginning & Te at end of the List
            //elementTemps.Insert(0, new KeyValuePair<double, double>(TotalElementWidth + 1, UserSaved.Ti_Value));
            //elementTemps.Insert(elementTemps.Count, new KeyValuePair<double, double>(-1, UserSaved.Te_Value));

            if (widthPosition == 0)
                return elementTemps;
            else throw new ArgumentOutOfRangeException("calculation failed");
        }
        private double GetfRsiValue()
        {
            //TODO: durch 0 teilen abfangen
            if (Ti - Te == 0)
                return 0;

            return Math.Round((LayerTemps.First().Value - Te) / (Ti - Te), 2);
        }

        private double GetMaxRelF() //maximal zulässige Raumluftfeuchte
        {
            if (FRsi * (Ti - Te) >= 0 && FRsi * (Ti - Te) <= 30)
            {
                double phiMax = 0.8 * Math.Pow((109.8 + FRsi * (Ti - Te) + Te) / (109.8 + Ti), 8.02) * 100;
                return Math.Round(phiMax, 1);
            }
            throw new ArgumentException("Randbedingung zur Berechnung nicht erfüllt."); //TODO Rechnung erlauben, jedoch Hinweis entsprechend einblenden
        }

        /* Hardcoded example:
         double tsiVal = tiVal - SurfaceResistance.selectedRsi * qValue;
        Tsi_Value.Text = "θsi [°C]: " + tsiVal.ToString();

		double t1_2Val = tsiVal - (layers[0].LayerResistance) * qValue;
		T1_2_Value.Text = "θ1/2 [°C]: " + t1_2Val.ToString();

		double t2_3Val = t1_2Val - (layers[1].LayerResistance) * qValue;
		T2_3_Value.Text = "θ2/3 [°C]: " + t2_3Val.ToString();

		double t3_4Val = t2_3Val- (layers[2].LayerResistance) * qValue;
		T3_4_Value.Text = "θ3/4 [°C]: " + t3_4Val.ToString();

		double tseVal = t3_4Val - (layers[3].LayerResistance) * qValue;
		Tse_Value.Text = "θse [°C]: " + tseVal.ToString();
         */
    }
}
