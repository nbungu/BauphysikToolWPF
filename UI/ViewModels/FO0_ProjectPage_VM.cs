using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

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

        // TODO: use Enums as parameter
        [RelayCommand]
        private void ChangeBuildingStats(string? property)
        {
            if (property is null) return;

            var proj = DatabaseAccess.QueryProjectById(FO0_ProjectPage.SelectedProjectId);
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
        private void ChangeProjectName(string? property)
        {
            if (property is null) return;

            var proj = DatabaseAccess.QueryProjectById(FO0_ProjectPage.SelectedProjectId);
            proj.Name = property;
            DatabaseAccess.UpdateProject(proj);
        }

        [RelayCommand]
        private void ChangeProjectEditor(string? property)
        {
            if (property is null) return;

            var proj = DatabaseAccess.QueryProjectById(FO0_ProjectPage.SelectedProjectId);
            proj.UserName = property;
            DatabaseAccess.UpdateProject(proj);
        }

        [RelayCommand]
        private void Close(Window? window)
        {
            if (window is null) return;
            window.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        // TODO only call Project from DB once

        [ObservableProperty]
        private Project _currentProject = DatabaseAccess.QueryProjectById(FO0_ProjectPage.SelectedProjectId);
    }
}
