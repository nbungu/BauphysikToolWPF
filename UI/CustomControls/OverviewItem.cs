using System.Globalization;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class OverviewItem
    {
        public string SymbolBase { get; set; } = string.Empty;
        public string SymbolSubscript { get; set; } = string.Empty;
        public double Value { get; set; }
        public double? RequirementValue { get; set; }
        public string RequirementStatement => RequirementValue?.ToString("F2", CultureInfo.CurrentCulture) ?? "ohne";

        private string _unit = string.Empty;
        public string Unit
        {
            get
            {
                if (RequirementValue is null) return "";
                else return _unit;
            }
            set => _unit = value;
        }
        public bool IsRequirementMet { get; set; }
        public SolidColorBrush Color => IsRequirementMet ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        public string Comment { get; set; } = string.Empty;
    }
}
