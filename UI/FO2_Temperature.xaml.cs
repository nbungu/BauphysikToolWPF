using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
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
        public string SymbolBase { get; set; } = string.Empty;
        public string SymbolSubscript { get; set; } = string.Empty;
        public double Value { get; set; }
        public double? RequirementValue { private get; set; }
        public string RequirementStatement
        {
            get { return (RequirementValue is null || RequirementValue == -1) ? "keine Anforderung" : RequirementValue.ToString(); }
        }
        public string Unit { get; set; } = string.Empty;
        public bool IsRequirementMet { get; set; } = false; 
        public Brush Color
        {
            get
            {
                return IsRequirementMet ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
        }
    }
}
