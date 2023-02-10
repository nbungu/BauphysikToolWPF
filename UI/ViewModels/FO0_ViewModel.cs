using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public class FO0_ViewModel
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "LandingPage";
        public string ProjectName { get; } = FO0_LandingPage.Project.Name ?? "";
        public string ProjectUserName { get; } = FO0_LandingPage.Project.UserName ?? "";
        public List<Element> Elements { get; } = FO0_LandingPage.Project.Elements;
        public bool IsBuildingUsage0 { get; } = FO0_LandingPage.Project.IsNonResidentialUsage; // Usage 0 = Nichtwohngebäude
        public bool IsBuildingUsage1 { get; } = FO0_LandingPage.Project.IsResidentialUsage;    // Usage 1 = Wohngebäude
        public bool IsBuildingAge0 { get; } = FO0_LandingPage.Project.IsExistingConstruction;  // Usage 0 = Bestand
        public bool IsBuildingAge1 { get; } = FO0_LandingPage.Project.IsNewConstruction;       // Usage 1 = Neubau
    }
}
