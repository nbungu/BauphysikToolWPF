using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_ViewModel : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "LandingPage";


        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void OpenNewElementWindow(string? selectedElementId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            var window = (selectedElementId is null) ? new NewElementWindow() : new NewElementWindow(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_LandingPage.ProjectId);
        }

        [RelayCommand]
        private void ElementDelete(string? selectedElementId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            if (selectedElementId is null)
                return;

            // Delete selected Layer
            DatabaseAccess.DeleteElement(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_LandingPage.ProjectId);
        }

        [RelayCommand]
        private void SelectElement(int selectedElementId = -1) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId == -1)
                return;

            FO0_LandingPage.SelectedElementId = selectedElementId;
            MainWindow.SetPage("Setup");
        }

        // TODO use Enums as parameter
        [RelayCommand]
        private void ChangeBuildingStats(string property)
        {
            if (property is null)
                return;

            var proj = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId);
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
            MainWindow.Main.Close();
        }

        /*
         * MVVM Properties
         * 
         * Initialized and Assigned with Default Values
         */


        [ObservableProperty]
        private List<Element> elements = DatabaseAccess.QueryElementsByProjectId(FO0_LandingPage.ProjectId) ?? new List<Element>();

        [ObservableProperty]
        private string projectName = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId).Name ?? "";

        [ObservableProperty]
        private string projectUserName = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId).UserName ?? "";

        [ObservableProperty]
        private bool isBuildingUsage0 = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId).IsNonResidentialUsage;   // Usage 0 = Nichtwohngebäude

        [ObservableProperty]
        private bool isBuildingUsage1 = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId).IsResidentialUsage;      // Usage 1 = Wohngebäude

        [ObservableProperty]
        private bool isBuildingAge0 = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId).IsExistingConstruction;    // Usage 0 = Bestand

        [ObservableProperty]
        private bool isBuildingAge1 = DatabaseAccess.QueryProjectById(FO0_LandingPage.ProjectId).IsNewConstruction;         // Usage 1 = Neubau      
    }
}
