﻿using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class StationaryTempCalc
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
        public StationaryTempCalc(List<Layer> layers)
        {
            if (layers.Count == 0)
                return;

            //User specified (public setter)
            Layers = layers;
            //Calculated parameters (private setter)
            TotalElementWidth = GetTotalElementWidth();
            SumOfLayersR = GetLayersR();    // Gl. 2-54; S.28
            RTotal = GetRTotal();           // Gl. 2-55; S.28
            UValue = GetUValue();           // Gl. 2-57; S.29
            QValue = GetqValue();           // Gl. 2-65; S.31
            LayerTemps = GetLayerTemps();   // Bsp. S.33
            FRsi = GetfRsiValue();          // Gl. 3-1; S.36
            PhiMax = GetMaxRelF();          // Gl. 3-3; S.37
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
                rLayers += l.R_Value;
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
            List<KeyValuePair<double,double>> temp_List = new List<KeyValuePair<double,double>>();

            //Starting from inner side
            double widthPosition = TotalElementWidth;
            double value = Ti - Rsi * QValue; // Tsi

            temp_List.Add(new KeyValuePair<double, double>(widthPosition, value)); // key, value

            for (int i = 0; i<Layers.Count; i++)
            {
                double currentWidthPosition = widthPosition - Layers[i].LayerThickness;
                double currentValue = temp_List.ElementAt(i).Value - Layers[i].R_Value * QValue;
                temp_List.Add(new KeyValuePair<double, double>(currentWidthPosition, currentValue));

                widthPosition = currentWidthPosition;
            }

            // Adding Ti at beginning & Te at end of the List
            //temp_List.Insert(0, new KeyValuePair<double, double>(TotalElementWidth + 1, UserSaved.Ti_Value));
            //temp_List.Insert(temp_List.Count, new KeyValuePair<double, double>(-1, UserSaved.Te_Value));

            if (widthPosition == 0)
                return temp_List;
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