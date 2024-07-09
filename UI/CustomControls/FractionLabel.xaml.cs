using System;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für FractionLabel.xaml
    /// </summary>
    public partial class FractionLabel : UserControl
    {
        public FractionLabel()
        {
            InitializeComponent();
        }

        public string CounterText // Zähler
        {
            get => (string)GetValue(CounterTextProperty);
            set => SetValue(CounterTextProperty, value);
        }

        public static readonly DependencyProperty CounterTextProperty = DependencyProperty.Register(nameof(CounterText), typeof(string), typeof(FractionLabel), new PropertyMetadata(string.Empty));
        public string DenominatorText // Nenner
        {
            get => (string)GetValue(DenominatorTextProperty);
            set => SetValue(DenominatorTextProperty, value);
        }

        public static readonly DependencyProperty DenominatorTextProperty = DependencyProperty.Register(nameof(DenominatorText), typeof(string), typeof(FractionLabel), new PropertyMetadata(string.Empty));

        private void CounterTextBlock_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FractionLine.X2 = Math.Max(e.NewSize.Width, DenominatorTextBlock.ActualWidth);
        }

        private void DenominatorTextBlock_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FractionLine.X2 = Math.Max(e.NewSize.Width, CounterTextBlock.ActualWidth);
        }
    }
}
