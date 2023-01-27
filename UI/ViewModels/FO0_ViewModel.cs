using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
using SkiaSharp;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class FO0_ViewModel
    {
        public string Title { get; } = "LandingPage";
        public List<Element> Elements { get; private set; }

        public FO0_ViewModel() // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        {
            Elements = FO0_LandingPage.Elements;
        }
    }
}
