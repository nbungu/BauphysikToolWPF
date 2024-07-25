using System.Windows.Controls;

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
    }
}
