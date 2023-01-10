using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO3_Moisture.xaml
    /// </summary>
    public partial class FO3_Moisture : UserControl
    {
        public static GlaserCurve GlaserCalc { get; private set; }
        public FO3_Moisture()
        {
            GlaserCalc = new GlaserCurve(FO1_Setup.Layers); //for FO3_ViewModel
            InitializeComponent();
        }
    }
}
