using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for OxyPlot Graph: Used in xaml as "DataContext"
    public class LiveChartsViewModel
    {
        public ISeries[] Series { get; set; }

        public LiveChartsViewModel()
        {
            this.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
                    Fill = null
                }
            };
        }
    }
}
