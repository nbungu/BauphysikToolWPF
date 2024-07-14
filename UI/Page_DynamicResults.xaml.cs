using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für F04_Dynamic.xaml
    /// </summary>
    public partial class Page_DynamicResults : UserControl
    {
        public static DynamicTempCalc DynamicTempCalculation { get; private set; } = new DynamicTempCalc();

        public Page_DynamicResults()
        {
            // Save computation time and only recalculate if needed
            // Only if Element, Layers or EnvVars are not set or have changed: update class variables.
            if (Page_LayerSetup.Recalculate)
            {
                DynamicTempCalculation = new DynamicTempCalc(UserSaved.SelectedElement);

                // Reset Recalculate Flag
                Page_LayerSetup.Recalculate = false;
            }

            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor

        }
    }
}
