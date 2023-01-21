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
    /// Interaktionslogik für FO2_Temperature.xaml
    /// </summary>
    public partial class FO2_Temperature : UserControl //Page
    {
        public static StationaryTempCalc StationaryTempCalculation { get; private set; }
        
        public FO2_Temperature()
        {
            // If Layers is not set or has changed, update class variables
            //TODO EnvVars can change too!!
            if (FO1_Setup.Layers != StationaryTempCalculation?.Layers)
            {
                StationaryTempCalculation = new StationaryTempCalc(FO1_Setup.Layers); //for FO2_ViewModel
            }
            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }
    }
}
