using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.UI
{
    public interface IPropertyItem
    {
        string Name { get; set; }
        Symbol Symbol { get; set; }
        Unit Unit { get; set; }
        object Value { get; set; }
        string Comment { get; set; }
        bool IsReadonly { get; set; }
        bool TriggerPropertyChanged { get; set; }
        object[] PropertyValues { get; set; }
        string SymbolBaseText { get; set; }
        string SymbolSubscriptText { get; set; }
        string UnitCounterText { get; set; }
        string UnitDenominatorText { get; set; }
    }
}
