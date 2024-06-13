using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_ProjectPage_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title => "ProjectPage";

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
        private void ChangeBuildingStats(string property = "")
        {
            switch (property)
            {
                case "BuildingUsage0":
                    UserSaved.CurrentProject.IsNonResidentialUsage = true;
                    break;
                case "BuildingUsage1":
                    UserSaved.CurrentProject.IsResidentialUsage = true;
                    break;
                case "BuildingAge0":
                    UserSaved.CurrentProject.IsExistingConstruction = true;
                    break;
                case "BuildingAge1":
                    UserSaved.CurrentProject.IsNewConstruction = true;
                    break;
                default:
                    return;
            }
        }

        // TODO: Execute on every page change?!
        [RelayCommand]
        private void SaveProject()
        {
            DatabaseAccess.UpdateProject(UserSaved.CurrentProject);
        }

        [RelayCommand]
        private void ChangeProjectName(string property = "")
        {
            UserSaved.CurrentProject.Name = property;
        }

        [RelayCommand]
        private void ChangeProjectEditor(string property = "")
        {
            UserSaved.CurrentProject.UserName = property;
        }

        [RelayCommand]
        private void Close(Window? window)
        {
            window?.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */



        //[ObservableProperty]
        //private Project _currentProject = UserSaved.CurrentProject;
    }
}
