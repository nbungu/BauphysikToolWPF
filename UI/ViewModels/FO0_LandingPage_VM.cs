using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_LandingPage_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "LandingPage";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            if (FO0_LandingPage.SelectedElementId != -1)
                MainWindow.SetPage(desiredPage);
        }

        // Create New Element / Edit Existing Element
        [RelayCommand]
        private void OpenNewElementWindow(int? selectedElementId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            var window = (selectedElementId is null) ? new NewElementWindow() : new NewElementWindow(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId);
        }

        [RelayCommand]
        private void DeleteElement(int? selectedElementId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            if (selectedElementId is null)
                return;

            // Delete selected Layer
            DatabaseAccess.DeleteElement(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // When deleting the Element which was currently SelectedElement
            if (Convert.ToInt32(selectedElementId) == FO0_LandingPage.SelectedElementId)
                FO0_LandingPage.SelectedElementId = -1;

            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId);
        }

        [RelayCommand]
        private void SelectElement(int? selectedElementId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId is null)
                return;

            FO0_LandingPage.SelectedElementId = Convert.ToInt32(selectedElementId);
            MainWindow.SetPage(NavigationContent.SetupLayer);
        }

        /*
         * MVVM Properties
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private List<Element> elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId) ?? new List<Element>();
        
    }
}
