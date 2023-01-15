using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using SkiaSharp;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class NewElementWindow_ViewModel
    {
        public string Title { get; } = "NewElement";
        public List<string> Types { get; private set; }

        public NewElementWindow_ViewModel() // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        {
            //For the ComboBox
            Types = DatabaseAccess.GetConstructionTypes().Select(e => e.Name).ToList();
        }
    }
}
