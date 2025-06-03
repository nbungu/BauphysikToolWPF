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

        public static readonly DependencyProperty UnitLabelProperty =
            DependencyProperty.Register(nameof(UnitLabel), typeof(string), typeof(Gauge), new PropertyMetadata("m"));

        public string UnitLabel
        {
            get => (string)GetValue(UnitLabelProperty);
            set => SetValue(UnitLabelProperty, value);
        }


        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Gauge), new PropertyMetadata("Title"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(nameof(Caption), typeof(string), typeof(Gauge), new PropertyMetadata("Caption"));

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
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
    }
}
