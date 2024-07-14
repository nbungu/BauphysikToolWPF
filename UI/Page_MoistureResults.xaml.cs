using BauphysikToolWPF.Calculations;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für Page_MoistureResults.xaml
    /// </summary>
    public partial class Page_MoistureResults : UserControl
    {
        public static GlaserCalc GlaserCalculation { get; private set; } = new GlaserCalc();

        public Page_MoistureResults()
        {
            if (UserSaved.SelectedElement != null) GlaserCalculation = new GlaserCalc(UserSaved.SelectedElement);
            InitializeComponent();
        }
    }
}
