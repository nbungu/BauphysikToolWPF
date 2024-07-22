using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für Page_TemperatureResults.xaml
    /// </summary>
    public partial class Page_TemperatureResults : UserControl
    {
        public static TemperatureCurveCalc TemperatureCurveCalculation { get; private set; } = new TemperatureCurveCalc();

        public Page_TemperatureResults()
        {
            if (UserSaved.SelectedElement != null) TemperatureCurveCalculation = new TemperatureCurveCalc(UserSaved.SelectedElement);

            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }
    }
}
