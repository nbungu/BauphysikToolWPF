﻿using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für F04_Dynamic.xaml
    /// </summary>
    public partial class Page_DynamicResults : UserControl
    {
        public Page_DynamicResults()
        {
            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
