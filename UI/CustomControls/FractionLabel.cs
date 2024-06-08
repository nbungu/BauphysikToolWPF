using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class FractionLabel : Control
    {
        public string CounterText // Zähler
        {
            get => (string)GetValue(CounterTextProperty);
            set => SetValue(CounterTextProperty, value);
        }

        public static readonly DependencyProperty CounterTextProperty = DependencyProperty.Register("CounterText", typeof(string), typeof(FractionLabel), new PropertyMetadata(string.Empty));
        public string DenominatorText // Nenner
        {
            get => (string)GetValue(DenominatorTextProperty);
            set => SetValue(DenominatorTextProperty, value);
        }

        public static readonly DependencyProperty DenominatorTextProperty = DependencyProperty.Register("DenominatorText", typeof(string), typeof(FractionLabel), new PropertyMetadata(string.Empty));
    }
}
