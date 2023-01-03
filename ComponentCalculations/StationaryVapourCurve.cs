using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    // Berechnung der Wasserdampfpartialdrücke pi und pe an den Außenoberflächen
    // Berechnung der Sättigungsdampfdrücke (Wasserdampfpartialdrücke im Sättigungszustand) psat an den Schichtgrenzen
    public class StationaryVapourCurve : StationaryTempCurve
    {
        // (Instance-) Constructor
        // !! derived class cannot inherit the constructor of the base class because constructors are not the members of the class
        //  constructors of both classes must be executed
        public StationaryVapourCurve(double vapourConcentration, List<Layer> layers, double rsi, double rse, double ti, double te) : base(layers, rsi, rse, ti, te)
        {

        }
    }
}
