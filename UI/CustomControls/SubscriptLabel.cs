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
    public class SubscriptLabel : Control
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

        public static readonly DependencyProperty BaseTextProperty = DependencyProperty.Register("BaseText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));

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

        public static readonly DependencyProperty SubscriptTextProperty = DependencyProperty.Register("SubscriptText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));
        public string PrependText
        {
            get
            {
                return (string)GetValue(PrependTextProperty);
            }
            set
            {
                SetValue(PrependTextProperty, value);
            }
        }

        public static readonly DependencyProperty PrependTextProperty = DependencyProperty.Register("PrependText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));
        public string AppendText
        {
            get
            {
                return (string)GetValue(AppendTextProperty);
            }
            set
            {
                SetValue(AppendTextProperty, value);
            }
        }

        public static readonly DependencyProperty AppendTextProperty = DependencyProperty.Register("AppendText", typeof(string), typeof(SubscriptLabel), new PropertyMetadata(string.Empty));
        public static void SetSubscriptFontSize(UIElement element, double value)
        {
            element.SetValue(SubscriptFontSizeProperty, value);
        }
        public static double GetSubscriptFontSize(UIElement element)
        {
            return (double)element.GetValue(SubscriptFontSizeProperty);
        }

        public static readonly DependencyProperty SubscriptFontSizeProperty = DependencyProperty.Register("SubscriptFontSize", typeof(double), typeof(SubscriptLabel), new PropertyMetadata((double)12.0));

        public static void SetFontSize(UIElement element, double value)
        {
            element.SetValue(FontSizeProperty, value);
        }
        public static double GetFontSize(UIElement element)
        {
            return (double)element.GetValue(FontSizeProperty);
        }

        public static new readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(SubscriptLabel), new PropertyMetadata((double)14.0));

    }
}
