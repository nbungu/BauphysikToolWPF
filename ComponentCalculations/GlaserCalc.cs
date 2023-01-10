using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public double TotalElementWidth { get; private set; } = 0;

        public List<KeyValuePair<double, double>> LayerPsat { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding P_sat in Pa
        public List<KeyValuePair<double, double>> LayerP { get; private set; } = new List<KeyValuePair<double, double>>();// Key: Position in cm from inner to outer side (0 cm), Value: corresponding P in Pa

        // (Instance-) Constructor
        public GlaserCalc(List<Layer> layers) : base(layers) //parameter aus base class mitnehmen
        {
            //User specified (public setter)
            Layers = layers;
            //Calculated parameters (private setter)
            TotalElementWidth = GetTotalEquivalentElementWidth();   // Gl. 5.2; S.246
            LayerPsat = GetLayerPsat();                             // Gl. 2.4; S.164
            LayerP = GetLayerP();                                   // Gl. 2.3; S.164
        }

        // Methods
        private double GetTotalEquivalentElementWidth()
        {
            double width = 0;
            foreach (Layer l in Layers)
            {
                width += l.Sd_Value; // sum of sd-values
            }
            return width;
        }
        private List<KeyValuePair<double, double>> GetLayerPsat()
        {
            //Dictionaries are not ordered: Instead use List as ordered collection
            List<KeyValuePair<double,double>> p_sat_List = new List<KeyValuePair<double,double>>();

            //Starting from inner side
            double widthPosition = TotalElementWidth;
            double value = Ti - Rsi * QValue; // Tsi

            p_sat_List.Add(new KeyValuePair<double, double>(widthPosition, value)); // key, value

            for (int i = 0; i<Layers.Count; i++)
            {
                double currentWidthPosition = widthPosition - Layers[i].LayerThickness;
                double currentValue = p_sat_List.ElementAt(i).Value - Layers[i].R_Value * QValue;
                p_sat_List.Add(new KeyValuePair<double, double>(currentWidthPosition, currentValue));

                widthPosition = currentWidthPosition;
            }

            // Adding Ti at beginning & Te at end of the List
            //p_sat_List.Insert(0, new KeyValuePair<double, double>(TotalElementWidth + 1, UserSaved.Ti_Value));
            //p_sat_List.Insert(p_sat_List.Count, new KeyValuePair<double, double>(-1, UserSaved.Te_Value));

            if (widthPosition == 0)
                return p_sat_List;
            else throw new ArgumentOutOfRangeException("calculation failed");
        }
        private List<KeyValuePair<double, double>> GetLayerP()
        {
            List<KeyValuePair<double, double>> p_List = new List<KeyValuePair<double, double>>();
            return;
        }

    }
}
