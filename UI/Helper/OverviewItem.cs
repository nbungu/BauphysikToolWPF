using System.Windows.Media;

namespace BauphysikToolWPF.UI.Helper
{
    public class OverviewItem
    {
        public string SymbolBase { get; set; } = string.Empty;
        public string SymbolSubscript { get; set; } = string.Empty;
        public double Value { get; set; }
        public double? RequirementValue { private get; set; }
        public string RequirementStatement
        {
            get { return (RequirementValue is null || RequirementValue == -1) ? "-" : RequirementValue.ToString(); }
        }
        public string Unit { get; set; } = string.Empty;
        public bool IsRequirementMet { get; set; } = false;
        public Brush Color
        {
            get
            {
                return IsRequirementMet ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
        }
    }
}
