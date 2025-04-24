using BauphysikToolWPF.Services.Application;
using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Controls;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.UI
{
    /*
     * To ensure that changing PropertyItem.Value reflects on the corresponding SelectedLayer,
     * you can use a delegate to dynamically get and set the Thickness property of SelectedLayer
     *
     *
     * usage with getter and setter:
     *
     * var thicknessProperty = new PropertyItem<double>(Symbol.Thickness, () => SelectedLayer.Thickness, value => SelectedLayer.Thickness = value);
     *
     * usage with getter only:
     *
     * var thicknessProperty = new PropertyItem<double>(Symbol.Thickness, () => SelectedLayer.Thickness);
     */


    public class PropertyItem<T> : IPropertyItem
    {
        public static event Notify? PropertyChanged; // event

        public static void OnPropertyChanged() //protected virtual method
        {
            PropertyChanged?.Invoke(); // if PropertyChanged is not null then call delegate
        }

        //------Variablen-----//

        // _getter and _setter are delegates used to get and set the Value.
        private Func<T>? _getter;
        private Action<T>? _setter;
        private T _value;

        //------Eigenschaften-----//

        public object Value
        {
            get => _getter != null ? (object)_getter() : _value;
            set
            {
                // Ensure value is not null if Value is non-nullable
                if (value == null) throw new ArgumentNullException(nameof(value), "Value cannot be null.");

                if (Equals(Value, value)) return;

                if (_setter != null)
                {
                    _setter((T)value);
                    if (TriggerPropertyChanged) OnPropertyChanged();
                }
                else
                {
                    _value = (T)value;
                    if (TriggerPropertyChanged) OnPropertyChanged();
                }
            }
        }
        public string Name { get; set; } = string.Empty;
        public Symbol Symbol { get; set; }
        public Unit Unit { get; set; }
        public string Comment { get; set; } = string.Empty;

        //------Not part of the Database-----//
        public bool IsReadonly { get; set; } = true;
        public int DecimalPlaces { get; set; } = 2;
        public bool TriggerPropertyChanged { get; set; } = true;
        public object[] PropertyValues { get; set; } = Array.Empty<object>();
        public string SymbolBaseText { get; set; } = string.Empty;
        public string SymbolSubscriptText { get; set; } = string.Empty;
        public string UnitCounterText { get; set; } = string.Empty;
        public string UnitDenominatorText { get; set; } = string.Empty;

        //------Konstruktor-----//

        public PropertyItem() { }

        public PropertyItem(string name, Func<T> getter, Action<T> setter)
        {
            Name = name;
            IsReadonly = false;
            _getter = getter;
            _setter = setter;
        }
        public PropertyItem(string name, Func<T> getter)
        {
            Name = name;
            IsReadonly = true;
            _getter = getter;
        }

        public PropertyItem(Symbol symbol, Func<T> getter)
        {
            SetPropertiesBySymbol(symbol);
            IsReadonly = true;
            _getter = getter;
        }
        public PropertyItem(Symbol symbol, Func<T> getter, Action<T> setter)
        {
            SetPropertiesBySymbol(symbol);
            IsReadonly = false;
            _getter = getter;
            _setter = setter;
        }
        public PropertyItem(string name, Symbol symbol, Func<T> getter)
        {
            SetPropertiesBySymbol(symbol);
            Name = name;
            IsReadonly = true;
            _getter = getter;
        }
        public PropertyItem(string name, Symbol symbol, Func<T> getter, Action<T> setter)
        {
            SetPropertiesBySymbol(symbol);
            Name = name;
            IsReadonly = false;
            _getter = getter;
            _setter = setter;
        }

        //------Methoden-----//

        private void SetPropertiesBySymbol(Symbol symbol)
        {
            //TemperatureAmplitudeDamping,
            //TemperatureAmplitudeRatio,
            //TimeShift,
            //EffectiveThermalMass,
            //DecrementFactor

            // Get the corresponding properties for the given symbol
            if (SymbolMapping.TryGetValue(symbol, out var properties))
            {
                Name = properties.name;
                Symbol = symbol;
                Unit = properties.unit;
                SymbolBaseText = properties.baseText;
                SymbolSubscriptText = properties.subscriptText;
                UnitCounterText = properties.counterText;
                UnitDenominatorText = properties.denominatorText;
                Comment = properties.comment;
            }
            else
            {
                throw new ArgumentException($"Unknown symbol: {symbol}", nameof(symbol));
            }
        }
    }
}
