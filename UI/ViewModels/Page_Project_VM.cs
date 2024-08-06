using BauphysikToolWPF.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Dialogs;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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
        private readonly IDialogService _dialogService;

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext
        public Page_Project_VM()
        {
            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            
            //UserSaved.SelectedProjectChanged += RefreshXamlBindings;

            _dialogService = new DialogService();
            _fileDialogService = new FileDialogService();
        }
        
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
            if (UserSaved.SelectedProject.IsModified)
            {
                MessageBoxResult result = _dialogService.ShowSaveConfirmationDialog();

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        ApplicationServices.CreateNewProject();
                        UserSaved.OnSelectedProjectChanged(false);
                        break;
                    case MessageBoxResult.Cancel:
                        // Do nothing, user cancelled the action
                        break;
                }
            }
            else
            {
                ApplicationServices.CreateNewProject();
                UserSaved.OnSelectedProjectChanged(false);
            }
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
                UserSaved.SelectedProject.IsModified = false;
                UserSaved.OnSelectedProjectChanged(false);
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
                UserSaved.SelectedProject.IsModified = false;
                UserSaved.OnSelectedProjectChanged(false);
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
                UserSaved.SelectedProject.IsModified = false;
                UserSaved.OnSelectedProjectChanged(false);
            }
        }

        [RelayCommand]
        private void OpenLinkedFile(string? filePath)
        {
            if (filePath is null) return;
            try
            {
                if (filePath.Contains("%appdata%", StringComparison.InvariantCultureIgnoreCase))
                {
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    filePath = filePath.Replace("%appdata%", appDataPath, StringComparison.InvariantCultureIgnoreCase);
                }
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                Logger.LogInfo($"Opened linked file: {filePath}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to open linked file: {ex.Message}");
            }
        }
        [RelayCommand]
        private void DeleteLinkedFile(string? file)
        {
            if (file is null) return;
            DroppedFilePaths.Remove(file);
            UserSaved.SelectedProject.LinkedFilesList = DroppedFilePaths.ToList();
            UserSaved.SelectedProject.IsModified = true;
            // Update single XAML Binding Property
            UserSaved.OnSelectedProjectChanged();
            OnPropertyChanged(nameof(DroppedFilePaths));
        }

        partial void OnAuthorNameChanged(string? value)
        {
            if (value is null) return;
            UserSaved.SelectedProject.UserName = value;
            UserSaved.SelectedProject.IsModified = true;
            // Update single XAML Binding Property
            UserSaved.OnSelectedProjectChanged();
            OnPropertyChanged(nameof(AuthorName));
        }

        partial void OnProjectNameChanged(string? value)
        {
            if (value is null) return;
            UserSaved.SelectedProject.Name = value;
            UserSaved.SelectedProject.IsModified = true;
            // Update single XAML Binding Property
            UserSaved.OnSelectedProjectChanged();
            OnPropertyChanged(nameof(ProjectName));
        }

        partial void OnCommentChanged(string? value)
        {
            if (value is null) return;
            UserSaved.SelectedProject.Comment = value;
            UserSaved.SelectedProject.IsModified = true;
            // Update single XAML Binding Property
            UserSaved.OnSelectedProjectChanged();
            OnPropertyChanged(nameof(Comment));
        }

        partial void OnIsNewConstrCheckedChanged(bool value)
        {
            UserSaved.SelectedProject.BuildingAge = value ? BuildingAgeType.New : BuildingAgeType.Existing;
            UserSaved.SelectedProject.IsModified = true;
            // Update single XAML Binding Property
            UserSaved.OnSelectedProjectChanged();
            OnPropertyChanged(nameof(IsNewConstrChecked));
            OnPropertyChanged(nameof(IsExistingConstrChecked));
        }
        partial void OnIsResidentialUsageCheckedChanged(bool value)
        {
            UserSaved.SelectedProject.BuildingUsage = value ? BuildingUsageType.Residential : BuildingUsageType.NonResidential;
            UserSaved.SelectedProject.IsModified = true;
            // Update single XAML Binding Property
            UserSaved.OnSelectedProjectChanged();
            OnPropertyChanged(nameof(IsResidentialUsageChecked));
            OnPropertyChanged(nameof(IsNonResidentialUsageChecked));
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _projectName = UserSaved.SelectedProject.Name;

        [ObservableProperty]
        private string _authorName = UserSaved.SelectedProject.UserName;

        [ObservableProperty]
        private string _comment = UserSaved.SelectedProject.Comment;

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

        //private void RefreshXamlBindings()
        //{
        //    ProjectName = UserSaved.SelectedProject.Name;
        //    AuthorName = UserSaved.SelectedProject.UserName;
        //    Comment = UserSaved.SelectedProject.Comment;
        //    DroppedFilePaths = new ObservableCollection<string>(UserSaved.SelectedProject.LinkedFilesList);
        //    IsResidentialUsageChecked = UserSaved.SelectedProject.BuildingUsage == BuildingUsageType.Residential;
        //    IsNonResidentialUsageChecked = UserSaved.SelectedProject.BuildingUsage == BuildingUsageType.NonResidential;
        //    IsNewConstrChecked = UserSaved.SelectedProject.BuildingAge == BuildingAgeType.New;
        //    IsExistingConstrChecked = UserSaved.SelectedProject.BuildingAge == BuildingAgeType.Existing;
        //}
    }
}
