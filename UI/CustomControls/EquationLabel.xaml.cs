using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für EquationLabel.xaml
    /// </summary>
    public partial class EquationLabel : UserControl
    {
        public EquationLabel()
        {
            InitializeComponent();
        }

        public string BaseText
        {
            get => (string)GetValue(BaseTextProperty);
            set => SetValue(BaseTextProperty, value);
        }

        public static readonly DependencyProperty BaseTextProperty = DependencyProperty.Register(nameof(BaseText), typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string SubscriptText
        {
            get => (string)GetValue(SubscriptTextProperty);
            set => SetValue(SubscriptTextProperty, value);
        }

        public static readonly DependencyProperty SubscriptTextProperty = DependencyProperty.Register(nameof(SubscriptText), typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string ValueUnit
        {
            get => (string)GetValue(ValueUnitProperty);
            set => SetValue(ValueUnitProperty, value);
        }

        public static readonly DependencyProperty ValueUnitProperty = DependencyProperty.Register(nameof(ValueUnit), typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string FollowingText
        {
            get => (string)GetValue(FollowingTextProperty);
            set => SetValue(FollowingTextProperty, value);
        }

        public static readonly DependencyProperty FollowingTextProperty = DependencyProperty.Register(nameof(FollowingText), typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public double SubscriptFontSize
        {
            get => (double)GetValue(SubscriptFontSizeProperty);
            set => SetValue(SubscriptFontSizeProperty, value);
        }

        public static readonly DependencyProperty SubscriptFontSizeProperty = DependencyProperty.Register(nameof(SubscriptFontSize), typeof(double), typeof(EquationLabel), new PropertyMetadata(12.0));
    }
}
