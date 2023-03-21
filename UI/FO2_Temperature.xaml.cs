using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO2_Temperature.xaml
    /// </summary>
    public partial class FO2_Temperature : UserControl
    {
        public static StationaryTempCalc? StationaryTempCalculation { get; private set; }

        public FO2_Temperature()
        {
            // Save computation time and only recalculate if needed
            // Only if Element, Layers or EnvVars are not set or have changed: update class variables.
            if (FO1_SetupLayer.RecalculateTemp == true)
            {
                StationaryTempCalculation = new StationaryTempCalc(DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId)); //for FO2_ViewModel

                // Reset Recalculate Flag
                FO1_SetupLayer.RecalculateTemp = false;
            }
            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }
    }
    public class OverviewItem
    {
        public string SymbolBase { get; set; }
        public string SymbolSubscript { get; set; }
        public double Value { get; set; }
        public double? RequirementValue { get; set; }
        public Color Color { get; set; }

    }
}
