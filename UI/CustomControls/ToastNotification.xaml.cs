using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI.CustomControls
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Interaktionslogik für ToastNotification.xaml
    /// </summary>
    public partial class ToastNotification : UserControl
    {
        public ToastNotification()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ToastNotification), new PropertyMetadata(string.Empty, OnMessageChanged));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToastNotification control) control.ToastText.Text = e.NewValue as string;
        }

        public static readonly DependencyProperty ToastTypeProperty =
            DependencyProperty.Register(nameof(ToastType), typeof(ToastType), typeof(ToastNotification), new PropertyMetadata(ToastType.Info, OnToastTypeChanged));

        public ToastType ToastType
        {
            get => (ToastType)GetValue(ToastTypeProperty);
            set => SetValue(ToastTypeProperty, value);
        }

        private static void OnToastTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToastNotification control) control.UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            switch (ToastType)
            {
                case ToastType.Info:
                    //RootGrid.Background = new SolidColorBrush(Colors.LightGray);
                    ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/info_b.png"));
                    break;
                case ToastType.Success:
                    //RootGrid.Background = new SolidColorBrush(Colors.LightGray);
                    ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/haken.png"));
                    break;
                case ToastType.Warning:
                    //RootGrid.Background = new SolidColorBrush(Colors.LightGray);
                    ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/triangle-warning.png"));
                    break;
                case ToastType.Error:
                    //RootGrid.Background = new SolidColorBrush(Colors.LightGray);
                    ToastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BauphysikToolWPF;component/Resources/Icons/error.png"));
                    break;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}