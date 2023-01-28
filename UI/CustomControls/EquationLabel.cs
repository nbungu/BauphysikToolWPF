using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class EquationLabel : Control
    {
        public string BaseText
        {
            get
            {
                return (string)GetValue(BaseTextProperty);
            }
            set
            {
                SetValue(BaseTextProperty, value);
            }
        }

        public static readonly DependencyProperty BaseTextProperty = DependencyProperty.Register("BaseText", typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string SubscriptText
        {
            get
            {
                return (string)GetValue(SubscriptTextProperty);
            }
            set
            {
                SetValue(SubscriptTextProperty, value);
            }
        }

        public static readonly DependencyProperty SubscriptTextProperty = DependencyProperty.Register("SubscriptText", typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string Unit
        {
            get
            {
                return (string)GetValue(UnitProperty);
            }
            set
            {
                SetValue(UnitProperty, value);
            }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public string FollowingText
        {
            get
            {
                return (string)GetValue(FollowingTextProperty);
            }
            set
            {
                SetValue(FollowingTextProperty, value);
            }
        }

        public static readonly DependencyProperty FollowingTextProperty = DependencyProperty.Register("FollowingText", typeof(string), typeof(EquationLabel), new PropertyMetadata(string.Empty));

        public static void SetSubscriptFontSize(UIElement element, double value)
        {
            element.SetValue(SubscriptFontSizeProperty, value);
        }
        public static double GetSubscriptFontSize(UIElement element)
        {
            return (double)element.GetValue(SubscriptFontSizeProperty);
        }

        public static readonly DependencyProperty SubscriptFontSizeProperty = DependencyProperty.Register("SubscriptFontSize", typeof(double), typeof(EquationLabel), new PropertyMetadata(12.0));

        public static void SetFontSize(UIElement element, double value)
        {
            element.SetValue(FontSizeProperty, value);
        }
        public static double GetFontSize(UIElement element)
        {
            return (double)element.GetValue(FontSizeProperty);
        }

        public static new readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(EquationLabel), new PropertyMetadata(14.0));

    }
}
