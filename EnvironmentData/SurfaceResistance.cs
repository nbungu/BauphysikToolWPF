using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.EnvironmentData
{
    public static class SurfaceResistance
    {
        //TODO: Kapselung für validation + in get/set anweisung ein event implementieren
        public static Dictionary<string, double> selectedRsi = null;
        public static Dictionary<string, double> selectedRse = null;


        // Dictionary: To save data as Key/Value Pairs + LINQ methods applicable 
        public static Dictionary<string, double> RsiData { get; set; } = new Dictionary<string, double>() { { "Default", 0.13 }, { "Exterior Wall", 0.13 }, { "Sloped Roof", 0.13 }, { "Basement Ceiling", 0.17 }, { "Flat Roof", 0.10 } };
        public static Dictionary<string, double> RseData { get; set; } = new Dictionary<string, double>() { { "Default", 0.04 }, { "Ventilated outer layer", 0.08 } };

        public static void AddRsiData(string key, double value)
        {
            if (key == String.Empty || key == null)
                key = "Custom";
            if(value < 0)
                value = 0;

            RsiData.Add(key, value);
        }
        public static void AddRseData(string key, double value)
        {
            if (key == String.Empty || key == null)
                key = "Custom";
            if (value < 0)
                value = 0;

            RsiData.Add(key, value);
        }
    }
}
