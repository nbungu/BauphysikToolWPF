using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für F04_Dynamic.xaml
    /// </summary>
    public partial class FO4_Dynamic : UserControl
    {
        public static DynamicTempCalc DynamicTempCalculation { get; private set; } = new DynamicTempCalc();

        public FO4_Dynamic()
        {
            // Save computation time and only recalculate if needed
            // Only if Element, Layers or EnvVars are not set or have changed: update class variables.
            if (FO1_SetupLayer.Recalculate)
            {
                DynamicTempCalculation = new DynamicTempCalc(UserSaved.SelectedElement);

                // Reset Recalculate Flag
                FO1_SetupLayer.Recalculate = false;
            }

            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor

        }
    }
}
