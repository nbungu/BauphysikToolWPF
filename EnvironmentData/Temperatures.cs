using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BauphysikToolWPF.EnvironmentData
{
    public static class Temperatures
    {
        //TODO: Kapselung für validation + in get/set anweisung ein event implementieren
        public static Dictionary<string, double> selectedTi = null;
        public static Dictionary<string, double> selectedTe = null;

        // Dictionary: To save data as Key/Value Pairs + LINQ methods applicable 
        public static Dictionary<string, double> TiData { get; set; } = new Dictionary<string, double>() { { "Glaser, Innenklima", 20 }, { "fRsi Reference", 20 }, { "DIN 18599 Reference", 20 }, { "WTA (Winter)", 20 }, { "WTA (Summer)", 22 }, { "Living Space", 21 }, { "Bathroom", 24 }, { "Office", 21 }, { "Side rooms (non-heated)", 10 }, { "Side rooms (heated)", 15 }, { "Classroom", 21 }, { "Hospital", 22 }, { "Department Store", 18 }, { "Church", 17 }, { "Museum", 18 }, { "Custom Value", 0 } };
        public static Dictionary<string, double> TeData { get; set; } = new Dictionary<string, double>() { { "Glaser, Aussenklima", -10 }, { "fRsi Reference", -5 }, { "DIN V 4108-6: January", -1.3 }, { "DIN V 4108-6: February", 0.6 }, { "DIN V 4108-6: March", 4.1 }, { "DIN V 4108-6: April", 9.5 }, { "DIN V 4108-6: May", 12.9 }, { "DIN V 4108-6: June", 15.7 }, { "DIN V 4108-6: July", 18 }, { "DIN V 4108-6: August", 18.3 }, { "DIN V 4108-6: September", 14.4 }, { "DIN V 4108-6: October", 9.1 }, { "DIN V 4108-6: November", 4.7 }, { "DIN V 4108-6: December", 1.3 }, { "Custom Value", 0 } };
    }
}
