using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_ProjectPage_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "ProjectPage";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        // TODO use Enums as parameter
        [RelayCommand]
        private void ChangeBuildingStats(string property)
        {
            if (property is null)
                return;

            var proj = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId);
            switch (property)
            {
                case "BuildingUsage0":
                    proj.IsNonResidentialUsage = true;
                    break;
                case "BuildingUsage1":
                    proj.IsResidentialUsage = true;
                    break;
                case "BuildingAge0":
                    proj.IsExistingConstruction = true;
                    break;
                case "BuildingAge1":
                    proj.IsNewConstruction = true;
                    break;
                default:
                    return;
            }
            DatabaseAccess.UpdateProject(proj);
        }

        [RelayCommand]
        private void Close()
        {
            MainWindow.Close();
        }

        /*
         * MVVM Properties
         * 
         * Initialized and Assigned with Default Values
         */

        // TODO only call Project from DB once

        [ObservableProperty]
        private string projectName = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId).Name ?? "";

        [ObservableProperty]
        private string projectUserName = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId).UserName ?? "";

        [ObservableProperty]
        private bool isBuildingUsage0 = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId).IsNonResidentialUsage;   // Usage 0 = Nichtwohngebäude

        [ObservableProperty]
        private bool isBuildingUsage1 = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId).IsResidentialUsage;      // Usage 1 = Wohngebäude

        [ObservableProperty]
        private bool isBuildingAge0 = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId).IsExistingConstruction;    // Usage 0 = Bestand

        [ObservableProperty]
        private bool isBuildingAge1 = DatabaseAccess.QueryProjectById(FO0_ProjectPage.ProjectId).IsNewConstruction;         // Usage 1 = Neubau      

    }
}
