using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO3_Moisture.xaml
    /// </summary>
    public partial class FO3_Moisture : UserControl
    {
        public static GlaserCalc? GlaserCalculation { get; private set; }
        public FO3_Moisture()
        {
            // If Layers or EnvVars are not set or have changed, update class variables
            if (FO1_Setup.RecalculateGlaser == true)
            {
                GlaserCalculation = new GlaserCalc(FO1_Setup.Layers, UserSaved.UserEnvVars); //for FO3_ViewModel

                // Reset Recalculate Flag
                FO1_Setup.RecalculateGlaser = false;
            }
            InitializeComponent();
        }
    }
}
