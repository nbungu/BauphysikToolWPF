using System.Windows;
using System.Windows.Controls;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für Gauge.xaml
    /// </summary>
    public partial class Gauge : UserControl
    {
        public Gauge()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(Gauge), new PropertyMetadata(25.0));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(Gauge), new PropertyMetadata(0.0));

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(Gauge), new PropertyMetadata(100.0));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty UnitCounterProperty =
            DependencyProperty.Register(nameof(UnitCounter), typeof(string), typeof(Gauge), new PropertyMetadata("m"));

        public string UnitCounter
        {
            get => (string)GetValue(UnitCounterProperty);
            set => SetValue(UnitCounterProperty, value);
        }

        public static readonly DependencyProperty UnitDenominatorProperty =
            DependencyProperty.Register(nameof(UnitDenominator), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string UnitDenominator
        {
            get => (string)GetValue(UnitDenominatorProperty);
            set => SetValue(UnitDenominatorProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Gauge), new PropertyMetadata("Gauge"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty ShowReferenceMarkerTickProperty =
            DependencyProperty.Register(nameof(ShowReferenceMarkerTick), typeof(bool), typeof(Gauge), new PropertyMetadata(true));

        public bool ShowReferenceMarkerTick
        {
            get => (bool)GetValue(ShowReferenceMarkerTickProperty);
            set => SetValue(ShowReferenceMarkerTickProperty, value);
        }

        public static readonly DependencyProperty ReferenceMarkerValueProperty =
            DependencyProperty.Register(nameof(ReferenceMarkerValue), typeof(double), typeof(Gauge), new PropertyMetadata(50.0));

        public double ReferenceMarkerValue
        {
            get => (double)GetValue(ReferenceMarkerValueProperty);
            set => SetValue(ReferenceMarkerValueProperty, value);
        }

        public static readonly DependencyProperty ShowUnitLabelProperty =
            DependencyProperty.Register(nameof(ShowUnitLabel), typeof(bool), typeof(Gauge), new PropertyMetadata(true));

        public bool ShowUnitLabel
        {
            get => (bool)GetValue(ShowUnitLabelProperty);
            set => SetValue(ShowUnitLabelProperty, value);
        }
    }
}
