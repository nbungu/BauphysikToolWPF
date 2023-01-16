using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class FractionLabel : Control
    {
        public string CounterText // Zähler
        {
            get
            {
                return (string)GetValue(CounterTextProperty);
            }
            set
            {
                SetValue(CounterTextProperty, value);
            }
        }

        public static readonly DependencyProperty CounterTextProperty = DependencyProperty.Register("CounterText", typeof(string), typeof(FractionLabel), new PropertyMetadata(string.Empty));
        public string DenominatorText // Nenner
        {
            get
            {
                return (string)GetValue(DenominatorTextProperty);
            }
            set
            {
                SetValue(DenominatorTextProperty, value);
            }
        }

        public static readonly DependencyProperty DenominatorTextProperty = DependencyProperty.Register("DenominatorText", typeof(string), typeof(FractionLabel), new PropertyMetadata(string.Empty));
    }
}
