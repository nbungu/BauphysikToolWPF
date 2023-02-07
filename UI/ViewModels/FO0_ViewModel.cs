using BauphysikToolWPF.ComponentCalculations;
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
        public bool IsBuildingUsage0 { get; set; } // Usage 0 = Nichtwohngebäude
        public bool IsBuildingUsage1 { get; set; } // Usage 1 = Wohngebäude
        public bool IsBuildingAge0 { get; set; } // Usage 0 = Bestand
        public bool IsBuildingAge1 { get; set; } // Usage 1 = Neubau

        public FO0_ViewModel() // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        {
            ProjectName = FO0_LandingPage.Project.Name ?? "";
            ProjectUserName = FO0_LandingPage.Project.UserName ?? "";
            Elements = FO0_LandingPage.Elements; 

            IsBuildingUsage1 = FO0_LandingPage.Project.IsResidentialUsage;
            IsBuildingUsage0 = FO0_LandingPage.Project.IsNonResidentialUsage;
            IsBuildingAge1 = FO0_LandingPage.Project.IsNewConstruction;
            IsBuildingAge0 = FO0_LandingPage.Project.IsExistingConstruction;
        }
    }
}
