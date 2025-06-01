using System.Globalization;
using System.Windows.Media;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.UI
{
    public class OverviewItem
    {
        public Symbol Symbol { get; set; }
        public string SymbolBase => SymbolMapping.TryGetValue(Symbol, out var mapping) ? mapping.baseText : string.Empty;
        public string SymbolSubscript => SymbolMapping.TryGetValue(Symbol, out var mapping) ? mapping.subscriptText : string.Empty;
        public double Value { get; set; }
        public double? RequirementValue { get; set; }
        public string RequirementStatement => RequirementValue?.ToString("F2", CultureInfo.CurrentCulture) ?? "-";
        
        public string Unit { get; set; } = string.Empty;

        public bool IsRequirementMet { get; set; }
        public SolidColorBrush Color => IsRequirementMet ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        public string Comment { get; set; } = string.Empty;
    }
}
