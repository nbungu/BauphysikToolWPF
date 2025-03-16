using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für Page_BuildingEnvelope.xaml
    /// </summary>
    public partial class Page_BuildingEnvelope : UserControl
    {
        public Page_BuildingEnvelope()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
