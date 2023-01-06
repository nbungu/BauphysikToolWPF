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
                if(value == null)
                    throw new ArgumentNullException("null layer list specified");
                layers = value;
            }
        }

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

        private double fRsiValue;
        public double FRsiValue { get => fRsiValue; set => fRsiValue = value; }


        private Dictionary<double, double> layerTemps; // Key: Position in cm from inner to outer side (0 cm), Value: corresponding Temperature in °C
        public Dictionary<double, double> LayerTemps { get => layerTemps; set => layerTemps = value; }

        // (Instance-) Constructor
        public StationaryTempCurve()
        {
            //User specified
            Layers = DatabaseAccess.GetLayers();
            //Calculated from input parameters of constructor
            TotalElementWidth = GetTotalElementWidth();
            SumOfLayersR = GetLayersR();
            RTotal = GetRTotal();
            UValue = GetUValue();
            QValue = GetqValue();
            LayerTemps = GetLayerTemps();
            FRsiValue = GetfRsiValue();
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
            return Math.Round(rLayers,3);
        }

        private double GetRTotal()
        {
            return Math.Round(UserSaved.Rsi_Value + SumOfLayersR + UserSaved.Rse_Value, 3);
        }

        private double GetUValue()
        {
            return Math.Round(Math.Pow(RTotal, -1),3);
        }

        private double GetqValue()
        {
            return Math.Round(UValue * (UserSaved.Ti_Value - UserSaved.Te_Value), 3);
        }

        private Dictionary<double, double> GetLayerTemps()
        {
            Dictionary<double, double> elementTemps = new Dictionary<double, double>();

            //Starting from inner side
            double widthPosition = TotalElementWidth;
            double tVal = UserSaved.Ti_Value - UserSaved.Rsi_Value * QValue; // Tsi

            elementTemps.Add(widthPosition, tVal); // key, value

            for (int i = 0; i<Layers.Count; i++)
            {
                double current_widthPosition = widthPosition - Layers[i].LayerThickness;
                double current_tVal = elementTemps.Values.ElementAt(i) - Layers[i].LayerResistance * QValue;
                elementTemps.Add(current_widthPosition, current_tVal);

                widthPosition = current_widthPosition;
            }
            if (widthPosition == 0)
                return elementTemps;
            else throw new ArgumentOutOfRangeException("calculation failed");
        }

        private double GetfRsiValue()
        {
            double tsi = LayerTemps.First().Value;
            return Math.Round((tsi - UserSaved.Te_Value) / (UserSaved.Ti_Value - UserSaved.Te_Value), 3);
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
