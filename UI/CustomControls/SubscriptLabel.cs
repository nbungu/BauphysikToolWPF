using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class SubscriptLabel : Control
    {
        public string BaseText
        {
            get => (string)GetValue(BaseTextProperty);
            set => SetValue(BaseTextProperty, value);
        }

        public static readonly DependencyProperty BaseTextProperty = DependencyProperty.Register("BaseText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));

        public string SubscriptText
        {
            get => (string)GetValue(SubscriptTextProperty);
            set => SetValue(SubscriptTextProperty, value);
        }

        public static readonly DependencyProperty SubscriptTextProperty = DependencyProperty.Register("SubscriptText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));
        public string PrependText
        {
            get => (string)GetValue(PrependTextProperty);
            set => SetValue(PrependTextProperty, value);
        }

        public static readonly DependencyProperty PrependTextProperty = DependencyProperty.Register("PrependText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));
        public string AppendText
        {
            get => (string)GetValue(AppendTextProperty);
            set => SetValue(AppendTextProperty, value);
        }

        public static readonly DependencyProperty AppendTextProperty = DependencyProperty.Register("AppendText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));

        public double SubscriptFontSize
        {
            get => (double)GetValue(SubscriptFontSizeProperty);
            set => SetValue(SubscriptFontSizeProperty, value);
        }

        public static readonly DependencyProperty SubscriptFontSizeProperty = DependencyProperty.Register("SubscriptFontSize", typeof(double), typeof(SubscriptLabel), new PropertyMetadata(12.0));
    }
}
