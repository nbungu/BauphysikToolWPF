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
        //(Instance-) Variables and encapsulated properties

        private List<Layer> layers;
        public List<Layer> Layers //for Validation
        {     
            get { return layers; }  
            set
            {
                if(value == null || value.Count == 0)
                    throw new ArgumentNullException("Empty layer list specified");
                layers = value;
            }
        }
        private double rsi;
        public double Rsi //for Validation
        {
            get { return rsi; }
            set 
            {
                if(value >= 0)
                {
                    rsi = value;
                }
                else
                {
                    throw new ArgumentException("Rsi Value cannot be negative");
                }
            }
        }
        private double rse;
        public double Rse //for Validation
        {
            get { return rse; }
            set
            {
                if (value >= 0)
                {
                    rse = value;
                }
                else
                {
                    throw new ArgumentException("Rse Value cannot be negative");
                }
            }
        }
        private double ti;
        public double Ti { get => ti; set => ti = value; }

        private double te;
        public double Te { get => te; set => te = value; }

        private double totalElementWidth;
        public double TotalElementWidth { get => totalElementWidth; set => totalElementWidth = value; }

        private double sumOfLayersR;
        public double SumOfLayersR { get => sumOfLayersR; set => sumOfLayersR = value; }

        private double rTotal;
        public double RTotal { get => rTotal; set => rTotal = value; }

        private double uValue;
        public double UValue { get => uValue; set => uValue = value; }

        private double qValue;
        public double QValue { get => qValue; set => qValue = value; }

        private double[] layerTemps;
        public double[] LayerTemps { get => layerTemps; set => layerTemps = value; }

        // (Instance-) Constructor
        public StationaryTempCurve(List<Layer> layers, double rsi, double rse, double ti, double te)
        {
            //User specified
            Layers = layers;
            Rsi = rsi;
            Rse = rse;
            Ti = ti;
            Te = te;
            //Calculated from input parameters of constructor
            TotalElementWidth = GetTotalElementWidth();
            SumOfLayersR = GetLayersR();
            RTotal = GetRTotal();
            UValue = GetUValue();
            QValue = GetqValue();
            LayerTemps = GetLayerTemps();
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
            return rLayers;
        }

        private double GetRTotal()
        {
            return SurfaceResistance.selectedRsiValue + SumOfLayersR + SurfaceResistance.selectedRseValue;
        }

        private double GetUValue()
        {
            return Math.Pow(RTotal, -1);
        }

        private double GetqValue()
        {
            return UValue * (ReferenceTemp.selectedTiValue - ReferenceTemp.selectedTeValue);
        }

        private double[] GetLayerTemps()
        {
            double[] elementTemps = new double[Layers.Count+1]; //e.g. 4 Layers have 5 Temps: 3 temps in between each other + left & right surface temp.

            //Starting from inner side
            double tsi = ReferenceTemp.selectedTiValue - SurfaceResistance.selectedRsiValue * QValue;
            elementTemps[0] = tsi;

            for (int i = 0; i<(Layers.Count); i++)
            {
                double current_tVal = elementTemps[i] - Layers[i].LayerResistance * QValue;
                elementTemps[i+1] = current_tVal;
            }
            return elementTemps;
        }

        /* Hardcoded example:
         double tsiVal = tiVal - SurfaceResistance.selectedRsiValue * qValue;
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
