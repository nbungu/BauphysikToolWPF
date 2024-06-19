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
            if (UserSaved.SelectedElement != null) GlaserCalculation = new GlaserCalc(UserSaved.SelectedElement);
            InitializeComponent();
        }
    }
}
