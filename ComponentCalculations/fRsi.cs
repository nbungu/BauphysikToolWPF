using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.EnvironmentData;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class fRsi
    {
        public static readonly double FRsiMin = 0.7;

        public static readonly double TsiMin = 12.6;

        public double Tsi { get; set; } = 0;
        public double FRsi { get; private set; } = 0;
        public double PhiMax { get; private set; } = 0;

        public fRsi() //TODO add add parameters
        {
            //User specified (public setter)
            Tsi = new StationaryTempCurve().LayerTemps.First().Value;
            //Calculated parameters (private setter)
            FRsi = GetfRsiValue();
            PhiMax = GetMaxRelF();
        }
        private double GetfRsiValue()
        {
            //TODO: durch 0 teilen abfangen
            return Math.Round((Tsi - UserSaved.Te_Value) / (UserSaved.Ti_Value - UserSaved.Te_Value), 3);
        }

        private double GetMaxRelF() //maximal zulässige Raumluftfeuchte
        {
            if(FRsi*(UserSaved.Ti_Value-UserSaved.Te_Value) >= 0 && FRsi*(UserSaved.Ti_Value - UserSaved.Te_Value) <= 30)
            {
                double phiMax = 0.8 * Math.Pow((109.8 + FRsi * (UserSaved.Ti_Value - UserSaved.Te_Value) + UserSaved.Te_Value) / (109.8 + UserSaved.Ti_Value), 8.02) * 100;
                return Math.Round(phiMax, 3);
            }
            throw new ArgumentException("Randbedingung zur Berechnung nicht erfüllt.");
        }
    }
}
