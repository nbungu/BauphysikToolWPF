using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.Defaults;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.EnvironmentData;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Reflection.Emit;
using System.Windows.Shapes;
using System.Windows.Ink;
using LiveChartsCore.Measure;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauphysikToolWPF.UI.ViewModels
{
    [ObservableObject]
    public partial class FO3_ViewModel
    {
        public string Title { get; } = "Moisture"; // Called by 'InitializeComponent()' due to Class-Binding in xaml via DataContext
        
        //public GlaserCurve GlaserCurveCalc { get; set; }
        public RectangularSection[] LayerSections { get; set; }
        public ISeries[] GlaserCurve { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint TooltipTextPaint { get; set; }
        public SolidColorPaint TooltipBackgroundPaint { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; }
        public SolidColorPaint LegendBackgroundPaint { get; set; }
        public FO3_ViewModel() // Called by 'InitializeComponent()' from FO3_Moisture.cs due to Class-Binding in xaml via DataContext
        {
            
        }
    }
}
