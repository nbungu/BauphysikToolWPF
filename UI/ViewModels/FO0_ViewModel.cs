using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_ViewModel : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "LandingPage";

        // For shortening
        private static int currentProject = FO0_LandingPage.ProjectId;

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
            Elements = DatabaseAccess.QueryElementsByProjectId(currentProject);
        }

        [RelayCommand]
        private void ElementDelete(string? selectedElementId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            if (selectedElementId is null)
                return;

            // Delete selected Layer
            DatabaseAccess.DeleteElement(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(currentProject);
        }

        [RelayCommand]
        private void SelectElement(int selectedElementId = -1) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId == -1)
                return;

            FO0_LandingPage.SelectedElementId = selectedElementId;
            MainWindow.SetPage("Setup");
        }

        /*
         * MVVM Properties
         */

        [ObservableProperty]
        private List<Element> elements = DatabaseAccess.QueryElementsByProjectId(currentProject) ?? new List<Element>();

        [ObservableProperty]
        private string projectName = DatabaseAccess.QueryProjectById(currentProject).Name ?? "";

        [ObservableProperty]
        private string projectUserName = DatabaseAccess.QueryProjectById(currentProject).UserName ?? "";

        [ObservableProperty]
        private bool isBuildingUsage0 = DatabaseAccess.QueryProjectById(currentProject).IsNonResidentialUsage;   // Usage 0 = Nichtwohngebäude

        [ObservableProperty]
        private bool isBuildingUsage1 = DatabaseAccess.QueryProjectById(currentProject).IsResidentialUsage;      // Usage 1 = Wohngebäude

        [ObservableProperty]
        private bool isBuildingAge0 = DatabaseAccess.QueryProjectById(currentProject).IsExistingConstruction;    // Usage 0 = Bestand

        [ObservableProperty]
        private bool isBuildingAge1 = DatabaseAccess.QueryProjectById(currentProject).IsNewConstruction;         // Usage 1 = Neubau      
    }
}
