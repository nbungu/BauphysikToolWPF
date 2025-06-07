using System.Windows;
using System.Windows.Controls;

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
        DependencyProperty.Register(nameof(Value), typeof(double?), typeof(Gauge), new PropertyMetadata(null));

        public double? Value
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
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(Gauge), new PropertyMetadata(1.0));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty UnitLabelProperty =
            DependencyProperty.Register(nameof(UnitLabel), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string UnitLabel
        {
            get => (string)GetValue(UnitLabelProperty);
            set => SetValue(UnitLabelProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(nameof(Caption), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public static readonly DependencyProperty ReferenceMarkerValueProperty =
            DependencyProperty.Register(nameof(ReferenceMarkerValue), typeof(double?), typeof(Gauge), new PropertyMetadata(null));

        public double? ReferenceMarkerValue
        {
            get => (double)GetValue(ReferenceMarkerValueProperty);
            set => SetValue(ReferenceMarkerValueProperty, value);
        }

        public static readonly DependencyProperty SymbolLabelBaseProperty =
            DependencyProperty.Register(nameof(SymbolLabelBase), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string SymbolLabelBase
        {
            get => (string)GetValue(SymbolLabelBaseProperty);
            set => SetValue(SymbolLabelBaseProperty, value);
        }

        public static readonly DependencyProperty SymbolLabelSubscriptProperty =
            DependencyProperty.Register(nameof(SymbolLabelSubscript), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string SymbolLabelSubscript
        {
            get => (string)GetValue(SymbolLabelSubscriptProperty);
            set => SetValue(SymbolLabelSubscriptProperty, value);
        }

        public static readonly DependencyProperty MarkerSymbolLabelBaseProperty =
            DependencyProperty.Register(nameof(MarkerSymbolLabelBase), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string MarkerSymbolLabelBase
        {
            get => (string)GetValue(MarkerSymbolLabelBaseProperty);
            set => SetValue(MarkerSymbolLabelBaseProperty, value);
        }

        public static readonly DependencyProperty MarkerSymbolLabelSubscriptProperty =
            DependencyProperty.Register(nameof(MarkerSymbolLabelSubscript), typeof(string), typeof(Gauge), new PropertyMetadata(""));

        public string MarkerSymbolLabelSubscript
        {
            get => (string)GetValue(MarkerSymbolLabelSubscriptProperty);
            set => SetValue(MarkerSymbolLabelSubscriptProperty, value);
        }

        public static readonly DependencyProperty ReverseGradientProperty =
            DependencyProperty.Register(nameof(ReverseGradient), typeof(bool), typeof(Gauge), new PropertyMetadata(false));

        public bool ReverseGradient
        {
            get => (bool)GetValue(ReverseGradientProperty);
            set => SetValue(ReverseGradientProperty, value);
        }
    }
}
