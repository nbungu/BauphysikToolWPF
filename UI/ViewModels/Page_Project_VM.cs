using BauphysikToolWPF.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Dialogs;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_Elements.xaml: Used in xaml as "DataContext"
    public partial class Page_Project_VM : ObservableObject
    {
        private readonly IFileDialogService _fileDialogService;
        public Page_Project_VM()
        {
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedProjectChanged += RefreshXamlBindings;
            _fileDialogService = new FileDialogService();
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

        [RelayCommand]
        private void Save()
        {
            if (!File.Exists(UserSaved.ProjectFilePath))
            {
                SaveTo();
            }
            else
            {
                ApplicationServices.WriteToConnectedDatabase(UserSaved.SelectedProject);
                ApplicationServices.SaveProjectToFile(UserSaved.SelectedProject, UserSaved.ProjectFilePath);
                UserSaved.OnSelectedProjectChanged();
            }
        }

        [RelayCommand]
        private void SaveTo()
        {
            ApplicationServices.WriteToConnectedDatabase(UserSaved.SelectedProject);

            string? filePath = _fileDialogService.ShowSaveFileDialog("project.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                ApplicationServices.SaveProjectToFile(UserSaved.SelectedProject, filePath);
                UserSaved.ProjectFilePath = filePath;
                UserSaved.OnSelectedProjectChanged();
            }
        }
        [RelayCommand]
        private void Open()
        {
            string? filePath = _fileDialogService.ShowOpenFileDialog("BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                Project loadedProject = ApplicationServices.LoadProjectFromFile(filePath);
                UserSaved.SelectedProject = loadedProject;
                UserSaved.ProjectFilePath = filePath;
                UserSaved.OnSelectedProjectChanged();
                SwitchPage(NavigationContent.ProjectPage);
            }
        }

        [RelayCommand]
        private void Close(Window? window)
        {
            window?.Close();
        }

        [RelayCommand]
        private void OpenLinkedFile(string filePath)
        {
            try
            {
                Logger.LogInfo("Sie werden weitergeleitet...");
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Logger.LogError($"Failed to open linked file: {ex.Message}");
            }
        }
        [RelayCommand]
        private void DeleteLinkedFile(string file)
        {
            DroppedFilePaths.Remove(file);
            UserSaved.SelectedProject.LinkedFilesList = DroppedFilePaths.ToList();
        }

        partial void OnAuthorNameChanged(string value)
        {
            if (value is null) return;
            UserSaved.SelectedProject.UserName = value;
        }

        partial void OnProjectNameChanged(string value)
        {
            if (value is null) return;
            UserSaved.SelectedProject.Name = value;
            // To Update title bar
            UserSaved.OnSelectedProjectChanged();
        }

        partial void OnIsNewConstrCheckedChanged(bool value)
        {
            UserSaved.SelectedProject.BuildingAge = value ? BuildingAgeType.New : BuildingAgeType.Existing;
            IsNewConstrChecked = value;
            IsExistingConstrChecked = !value;
        }
        partial void OnIsResidentialUsageCheckedChanged(bool value)
        {
            UserSaved.SelectedProject.BuildingUsage = value ? BuildingUsageType.Residential : BuildingUsageType.NonResidential;
            IsResidentialUsageChecked = value;
            IsNonResidentialUsageChecked = !value;
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

        [ObservableProperty]
        private ObservableCollection<string> _droppedFilePaths = new ObservableCollection<string>(UserSaved.SelectedProject.LinkedFilesList);

        [ObservableProperty]
        private bool _isResidentialUsageChecked = UserSaved.SelectedProject.BuildingUsage == BuildingUsageType.Residential;

        [ObservableProperty]
        private bool _isNewConstrChecked = UserSaved.SelectedProject.BuildingAge == BuildingAgeType.New;

        [ObservableProperty]
        private bool _isNonResidentialUsageChecked = UserSaved.SelectedProject.BuildingUsage == BuildingUsageType.NonResidential;

        [ObservableProperty]
        private bool _isExistingConstrChecked = UserSaved.SelectedProject.BuildingAge == BuildingAgeType.Existing;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, No direct User Input involved
         */

        private void RefreshXamlBindings()
        {
            CurrentProject = UserSaved.SelectedProject;
            ProjectName = UserSaved.SelectedProject.Name;
            AuthorName = UserSaved.SelectedProject.UserName;
            DroppedFilePaths = new ObservableCollection<string>(UserSaved.SelectedProject.LinkedFilesList);
            IsResidentialUsageChecked = UserSaved.SelectedProject.BuildingUsage == BuildingUsageType.Residential;
            IsNonResidentialUsageChecked = UserSaved.SelectedProject.BuildingUsage == BuildingUsageType.NonResidential;
            IsNewConstrChecked = UserSaved.SelectedProject.BuildingAge == BuildingAgeType.New;
            IsExistingConstrChecked = UserSaved.SelectedProject.BuildingAge == BuildingAgeType.Existing;
        }
    }
}
