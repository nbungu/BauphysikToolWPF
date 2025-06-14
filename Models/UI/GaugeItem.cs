using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.UI
{
    public class GaugeItem
    {
        public string Title { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string Tooltip { get; set; }


        public Symbol Symbol { get; }
        public RequirementComparison Comparison { get; }
        public double? Value { get; }
        public string SymbolBase => SymbolMapping.TryGetValue(Symbol, out var mapping) ? mapping.baseText : string.Empty;
        public string SymbolSubscript => SymbolMapping.TryGetValue(Symbol, out var mapping) ? mapping.subscriptText : string.Empty;


        public double? MarkerValue { get; set; } = null;
        public string MarkerSymbolBase => SymbolMapping.TryGetValue(Symbol, out var mapping) ? mapping.baseText : string.Empty;
        public string MarkerSymbolSubscript
        {
            get
            {
                string baseSubscript = string.Empty;

                if (SymbolMapping.TryGetValue(Symbol, out var mapping))
                {
                    baseSubscript = mapping.subscriptText;
                }

                string suffix = string.Empty;
                switch (Comparison)
                {
                    // Any calculated Value needs to be less than or equal to the MarkerValue -> Marker is the "max" Value
                    case RequirementComparison.LessThan:
                    case RequirementComparison.LessThanOrEqual:
                        suffix = "max";
                        break;
                    // Any calculated Value needs to be greater than or equal to the MarkerValue -> Marker is the "min" Value
                    case RequirementComparison.GreaterThan:
                    case RequirementComparison.GreaterThanOrEqual:
                        suffix = "min";
                        break;
                }

                if (string.IsNullOrEmpty(suffix))
                    return baseSubscript;

                if (string.IsNullOrEmpty(baseSubscript))
                    return suffix;

                return baseSubscript + "," + suffix;
            }
        }

        public double ScaleMin { get; set; }
        public double ScaleMax { get; set; }

        public string Unit => GetUnitStringFromSymbol(Symbol);

        public GaugeItem(Symbol symbol, double? value = null, double? markerValue = null, RequirementComparison comparisonType = RequirementComparison.None)
        {
            Symbol = symbol;
            Value = value;
            MarkerValue = markerValue;
            Comparison = comparisonType;
            if (SymbolMapping.TryGetValue(Symbol, out var mapping))
            {
                Title = mapping.name + ":";
                Tooltip = mapping.comment;
            }
            else
            {
                Title = string.Empty;
                Tooltip = string.Empty;
            }
            ScaleMin = 0.0; // Default minimum scale value
            ScaleMax = 2 * MarkerValue ?? 2 * Value ?? 1.0;
        }   
    }
}
