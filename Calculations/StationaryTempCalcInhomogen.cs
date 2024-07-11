using BauphysikToolWPF.Models;
using BauphysikToolWPF.SessionData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.Calculations
{
    public class StationaryTempCalcInhomogen
    {
        protected double _ti = UserSaved.Ti;
        protected double _te = UserSaved.Te;
        protected double _rsi = UserSaved.Rsi;
        protected double _rse = UserSaved.Rse;

        public Element Element { get; } = new Element();
        public double RTotal { get; }
        public double UValue { get; }
        public double QValue { get; }
        public double FRsi { get; }
        public double Tsi_min { get; }
        public double Tsi { get; }
        public double Tse { get; }
        public SortedDictionary<double, double> LayerTemps { get; } = new SortedDictionary<double, double>();
        public bool IsValid { get; }

        public StationaryTempCalcInhomogen() { }

        public StationaryTempCalcInhomogen(Element element)
        {
            if (element.Layers.Count == 0 || element is null) return;

            Element = element;

            //var (rUpper, rLower) = GetInhomogeneousRTotals(element);
            //RTotal = (rUpper + rLower) / 2;
            IsValid = true;
        }

        /*private Dictionary<string, double> GetAreaFractions(Element element)
        {
            
            
            
            double rUpper = 0;
            double rLower = _rsi + _rse;

            var areaFractions = new List<double>();
            var rUpperValues = new List<double>();
            var rLayerValues = new List<double>();

            foreach (var layer in element.Layers)
            {
                if (layer.HasSubConstruction)
                {
                    var subConstruction = layer.SubConstruction;
                    var rValue = layer.R_Value;
                    var rSubValue = subConstruction.R_Value;

                    double areaFractionA = subConstruction.SubConstructionDirection == SubConstructionDirection.Vertical ?
                        (subConstruction.Spacing - subConstruction.Width) / subConstruction.Spacing :
                        (subConstruction.Thickness - subConstruction.Width) / subConstruction.Thickness;
                    double areaFractionB = 1 - areaFractionA;

                    areaFractions.Add(areaFractionA);
                    rUpperValues.Add(rValue + rSubValue);
                    rLayerValues.Add(1 / (areaFractionA / rValue + areaFractionB / rSubValue));
                }
                else
                {
                    rUpper += layer.R_Value;
                    rLower += layer.R_Value;
                }
            }

            rUpper += 1 / rUpperValues.Sum(r => 1 / r);
            rLower += rLayerValues.Sum();

            return (rUpper, rLower);
        }*/

        
    }
}
