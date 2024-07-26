using BauphysikToolWPF.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for MainWindow.xaml: Used in xaml as "DataContext"
    public partial class MainWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from MainWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => "Main";

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
        
        [RelayCommand]
        private void New()
        {

        }

        [RelayCommand]
        private void Save()
        {
            ApplicationServices.WriteToConnectedDatabase(UserSaved.SelectedProject);

            string filePath = @"C:\Users\arnes\Desktop\project.btk";
            ApplicationServices.SaveProjectToFile(UserSaved.SelectedProject, filePath);
        }

        [RelayCommand]
        private void Open()
        {
            string filePath = @"C:\Users\arnes\Desktop\project.btk";
            Project loadedProject = ApplicationServices.LoadProjectFromFile(filePath);
            UserSaved.SelectedProject = loadedProject;
        }


        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Project _currentProject = UserSaved.SelectedProject;
    }
}
