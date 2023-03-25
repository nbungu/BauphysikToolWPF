using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class DynamicTempCalc
    {
        // TAV und TAD
        /*public double Test_TAD
        {
            // Verfahren nach Haferland+Heindl 
            // oder DIN EN ISO 13786

            // https://energie-m.de/info/waermespeicherfaehigkeit.html
            // https://enbau-online.ch/bauphysik/2-2-waermespeicherung/
            get
            {
                var customEnvVars1 = new Dictionary<string, double>()
                {
                    {"Ti", 20 },
                    {"Te", 35 },
                    {"Rsi", 0.13 },
                    {"Rse", 0.04 },
                    {"Rel_Fi", 50 },
                    {"Rel_Fe", 80 },
                };
                var customEnvVars2 = new Dictionary<string, double>()
                {
                    {"Ti", 20 },
                    {"Te", 15 },
                    {"Rsi", 0.13 },
                    {"Rse", 0.04 },
                    {"Rel_Fi", 50 },
                    {"Rel_Fe", 80 },
                };
                var calc1 = new StationaryTempCalc(DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId), customEnvVars1);
                var calc2 = new StationaryTempCalc(DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId), customEnvVars2);
                double tad = Math.Abs(calc1.Te - calc2.Te) / Math.Abs(calc1.Tsi - calc2.Tsi);

                return Math.Round(tad, 1);
            }
        }*/
    }
}
