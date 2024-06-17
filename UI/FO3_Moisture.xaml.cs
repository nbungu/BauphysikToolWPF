using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO3_Moisture.xaml
    /// </summary>
    public partial class FO3_Moisture : UserControl
    {
        public static GlaserCalc GlaserCalculation { get; private set; } = new GlaserCalc();

        public FO3_Moisture()
        {
            // Save computation time and only recalculate if needed
            // Only if Element, Layers or EnvVars are not set or have changed: update class variables.
            if (FO1_SetupLayer.Recalculate)
            {
                GlaserCalculation = new GlaserCalc(UserSaved.SelectedElement); // for FO3_ViewModel

                // Reset Recalculate Flag
                FO1_SetupLayer.Recalculate = false;
            }
            InitializeComponent();
        }
    }
}
