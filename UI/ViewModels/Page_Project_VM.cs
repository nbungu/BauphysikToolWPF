using BauphysikToolWPF.Models;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using BauphysikToolWPF.Repository;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_Elements.xaml: Used in xaml as "DataContext"
    public partial class Page_Project_VM : ObservableObject
    {
        public Page_Project_VM()
        {
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedProjectChanged += RefreshXamlBindings;
        }

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext
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
                    UserSaved.SelectedProject.IsNonResidentialUsage = true;
                    break;
                case "BuildingUsage1":
                    UserSaved.SelectedProject.IsResidentialUsage = true;
                    break;
                case "BuildingAge0":
                    UserSaved.SelectedProject.IsExistingConstruction = true;
                    break;
                case "BuildingAge1":
                    UserSaved.SelectedProject.IsNewConstruction = true;
                    break;
                default:
                    return;
            }
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void SaveProject()
        {
            UserSaved.SelectedProject.UpdateTimestamp();
            DatabaseAccess.UpdateFullProject(UserSaved.SelectedProject);
        }

        [RelayCommand]
        private void Close(Window? window)
        {
            window?.Close();
        }

        partial void OnAuthorNameChanged(string value)
        {
            UserSaved.SelectedProject.UserName = value;
        }

        partial void OnProjectNameChanged(string value)
        {
            UserSaved.SelectedProject.Name = value;
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */
        
        [ObservableProperty]
        private Project _currentProject = UserSaved.SelectedProject;

        [ObservableProperty]
        private string _projectName = UserSaved.SelectedProject.Name;

        [ObservableProperty]
        private string _authorName = UserSaved.SelectedProject.UserName;

        private void RefreshXamlBindings()
        {
            CurrentProject = null;
            CurrentProject = UserSaved.SelectedProject;
        }
    }
}
