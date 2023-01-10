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
    public class GlaserCurve : StationaryTempCurve
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
                if (value == null)
                    throw new ArgumentNullException("null layer list specified");
                layers = value;
            }
        }

        // (Instance-) Constructor
        public GlaserCurve(List<Layer> layers) : base(layers) //parameter aus base class mitnehmen
        {
            Layers =layers;
        }

        // Methods
        
    }
}
