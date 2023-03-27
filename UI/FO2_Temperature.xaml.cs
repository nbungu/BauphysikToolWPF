using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO2_Temperature.xaml
    /// </summary>
    public partial class FO2_Temperature : UserControl
    {
        public static StationaryTempCalc? StationaryTempCalculation { get; private set; }
        public static DynamicTempCalc? DynamicTempCalculation { get; private set; }

        public FO2_Temperature()
        {
            // Save computation time and only recalculate if needed
            // Only if Element, Layers or EnvVars are not set or have changed: update class variables.
            if (FO1_SetupLayer.RecalculateTemp == true)
            {
                StationaryTempCalculation = new StationaryTempCalc(DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId), UserSaved.UserEnvVars); //for FO2_ViewModel
                DynamicTempCalculation = new DynamicTempCalc(DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId));

                // Reset Recalculate Flag
                FO1_SetupLayer.RecalculateTemp = false;
            }
            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }
    }
}
