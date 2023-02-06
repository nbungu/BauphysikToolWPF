using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class FO0_ViewModel
    {
        public string Title { get; } = "LandingPage";
        public string ProjectName { get; set; }
        public string ProjectUserName { get; set; }
        public List<Element> Elements { get; private set; }
        public bool IsBuildingUsageA { get; set; } = false; // Usage A = Wohngebäude
        public bool IsBuildingUsageB { get; set; } = false; // Usage B = Nicht-Wohngebäude

        public FO0_ViewModel() // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        {
            ProjectName = FO0_LandingPage.Project.Name;
            ProjectUserName = FO0_LandingPage.Project.UserName;
            Elements = FO0_LandingPage.Elements;
            // TODO Building Usage
        }
    }
}
