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
        public FO2_Temperature() //
        {
            InitializeComponent();
            // -> Initializes xaml objects
            // -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
            // -> e.g. Calls the FO1_ViewModel constructor & LiveChartsViewModel constructor
        }
    }
}
