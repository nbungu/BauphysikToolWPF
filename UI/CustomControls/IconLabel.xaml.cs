using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.CustomControls
{
    public enum IconLabelState
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Interaktionslogik für IconLabel.xaml
    /// </summary>
    public partial class IconLabel : UserControl
    {
        public IconLabel()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(IconLabel), new PropertyMetadata(null, OnIconSourceChanged));

        public ImageSource IconSource
        {
            get => (ImageSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconLabel control) control.Icon.Source = e.NewValue as ImageSource;
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(IconLabel), new PropertyMetadata(string.Empty, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconLabel control) control.Label.Content = e.NewValue as string;
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(IconLabelState), typeof(IconLabel), new PropertyMetadata(IconLabelState.Info, OnStateChanged));

        public IconLabelState Type
        {
            get => (IconLabelState)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconLabel control) control.UpdateBackground();
        }

        private void UpdateBackground()
        {
            switch (Type)
            {
                case IconLabelState.Info:
                    Background = new SolidColorBrush(Colors.LightBlue);
                    break;
                case IconLabelState.Warning:
                    Background = new SolidColorBrush(Colors.LightGray);
                    break;
                case IconLabelState.Error:
                    Background = new SolidColorBrush(Colors.LightCoral);
                    break;
                default:
                    Background = new SolidColorBrush(Colors.LightGray);
                    break;
            }
        }
    }
}
