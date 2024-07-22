using System.Windows.Controls;
using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.SessionData;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für F04_Dynamic.xaml
    /// </summary>
    public partial class TestWindow : UserControl
    {
        public TestWindow()
        {
            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }
    }
}
