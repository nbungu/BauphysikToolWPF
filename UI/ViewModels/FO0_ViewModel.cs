using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;

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
