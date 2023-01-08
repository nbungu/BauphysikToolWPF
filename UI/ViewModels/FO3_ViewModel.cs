using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.UI.ViewModels
{
    public class FO3_ViewModel
    {
        public string Title { get; } = "Moisture"; // Called by 'InitializeComponent()' due to Class-Binding in xaml via DataContext
        public fRsi FRsiCalc { get; set; }

        public FO3_ViewModel() // Called by 'InitializeComponent()' from FO2_Calculate.cs due to Class-Binding in xaml via DataContext
        {
            this.FRsiCalc = new fRsi();
        }
    }
}
