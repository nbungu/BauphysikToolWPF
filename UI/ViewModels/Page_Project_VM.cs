using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.Services.Models;
using BauphysikToolWPF.UI.Services;
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
        private readonly IDialogService _dialogService;

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext
        public Page_Project_VM()
        {
            //Session.SelectedProject.AssignInternalIdsToElements();

            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedProjectChanged += RefreshXamlBindings;
            Session.NewProjectAdded += RefreshXamlBindings;

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
        private void Next()
        {
            if (RecentProjectsListVisibility == Visibility.Visible && SelectedListViewItem != null)
            {
                var filePath = SelectedListViewItem.FilePath;
                Project loadedProject = ApplicationServices.LoadProjectFromFile(filePath);

                Session.SelectedProject = loadedProject;
                Session.ProjectFilePath = filePath;
                Session.SelectedProject.IsModified = false;
                ApplicationServices.AddRecentProject(filePath);
                Session.OnNewProjectAdded(false);
            }
            SwitchPage(NavigationContent.LandingPage);
        }

        [RelayCommand]
        private void New()
        {
            if (Session.SelectedProject.IsModified)
            {
                MessageBoxResult result = _dialogService.ShowSaveConfirmationDialog();

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        ApplicationServices.CreateNewProject();
                        Session.OnNewProjectAdded(false);
                        break;
                    case MessageBoxResult.Cancel:
                        // Do nothing, user cancelled the action
                        break;
                }
            }
            else
            {
                ApplicationServices.CreateNewProject();
                Session.OnNewProjectAdded(false);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (!File.Exists(Session.ProjectFilePath)) SaveTo();
            else
            {
                ApplicationServices.WriteToConnectedDatabase(Session.SelectedProject);
                ApplicationServices.SaveProjectToFile(Session.SelectedProject, Session.ProjectFilePath);
                Session.SelectedProject.IsModified = false;
                Session.OnSelectedProjectChanged(false);
            }
        }

        [RelayCommand]
        private void SaveTo()
        {
            ApplicationServices.WriteToConnectedDatabase(Session.SelectedProject);

            string? filePath = _fileDialogService.ShowSaveFileDialog("project.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                ApplicationServices.SaveProjectToFile(Session.SelectedProject, filePath);
                Session.ProjectFilePath = filePath;
                Session.SelectedProject.IsModified = false;
                ApplicationServices.AddRecentProject(filePath);
                Session.OnSelectedProjectChanged(false);
            }
        }
        [RelayCommand]
        private void Open()
        {
            string? filePath = _fileDialogService.ShowOpenFileDialog("BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                Project loadedProject = ApplicationServices.LoadProjectFromFile(filePath);

                Session.SelectedProject = loadedProject;
                Session.ProjectFilePath = filePath;
                Session.SelectedProject.IsModified = false;
                ApplicationServices.AddRecentProject(filePath);
                Session.OnNewProjectAdded(false);
                SwitchPage(NavigationContent.LandingPage);
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
                    string programDataPath = ApplicationServices.LocalProgramDataPath;
                    filePath = filePath.Replace("%appdata%", programDataPath, StringComparison.InvariantCultureIgnoreCase);
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
            Session.SelectedProject.LinkedFilesList = DroppedFilePaths.ToList();
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        [RelayCommand]
        private void RecentProjectDoubleClick()
        {
            if (SelectedListViewItem is null) return;
            Next();
        }

        partial void OnAuthorNameChanged(string value)
        {
            Session.SelectedProject.UserName = value;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        partial void OnProjectNameChanged(string value)
        {
            Session.SelectedProject.Name = value;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        partial void OnCommentChanged(string value)
        {
            Session.SelectedProject.Comment = value;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        partial void OnIsNewConstrCheckedChanged(bool value)
        {
            Session.SelectedProject.BuildingAge = value ? BuildingAgeType.New : BuildingAgeType.Existing;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }
        partial void OnIsResidentialUsageCheckedChanged(bool value)
        {
            Session.SelectedProject.BuildingUsage = value ? BuildingUsageType.Residential : BuildingUsageType.NonResidential;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _projectName = Session.SelectedProject.Name;

        [ObservableProperty]
        private string _authorName = Session.SelectedProject.UserName;

        [ObservableProperty]
        private string _comment = Session.SelectedProject.Comment;

        [ObservableProperty]
        private ObservableCollection<string> _droppedFilePaths = new ObservableCollection<string>(Session.SelectedProject.LinkedFilesList);

        [ObservableProperty]
        private bool _isResidentialUsageChecked = Session.SelectedProject.BuildingUsage == BuildingUsageType.Residential;

        [ObservableProperty]
        private bool _isNewConstrChecked = Session.SelectedProject.BuildingAge == BuildingAgeType.New;

        [ObservableProperty]
        private bool _isNonResidentialUsageChecked = Session.SelectedProject.BuildingUsage == BuildingUsageType.NonResidential;

        [ObservableProperty]
        private bool _isExistingConstrChecked = Session.SelectedProject.BuildingAge == BuildingAgeType.Existing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ElementPageAvailable))]
        private RecentProjectItem? _selectedListViewItem;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, No direct User Input involved
         */

        public List<RecentProjectItem> RecentProjects { get; set; } = ApplicationServices.LoadRecentProjects();
        public Visibility ProjectDataVisibility => RecentProjects.Count == 0 || Session.SelectedProject.CreatedByUser ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RecentProjectsListVisibility => ProjectDataVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        public bool ElementPageAvailable => ProjectDataVisibility == Visibility.Visible || SelectedListViewItem != null;

        private void RefreshXamlBindings()
        {
            // When updating fields/variables
            ProjectName = Session.SelectedProject.Name;
            AuthorName = Session.SelectedProject.UserName;
            Comment = Session.SelectedProject.Comment;
            DroppedFilePaths = new ObservableCollection<string>(Session.SelectedProject.LinkedFilesList);
            IsResidentialUsageChecked = Session.SelectedProject.BuildingUsage == BuildingUsageType.Residential;
            IsNonResidentialUsageChecked = Session.SelectedProject.BuildingUsage == BuildingUsageType.NonResidential;
            IsNewConstrChecked = Session.SelectedProject.BuildingAge == BuildingAgeType.New;
            IsExistingConstrChecked = Session.SelectedProject.BuildingAge == BuildingAgeType.Existing;

            // When updating properties
            OnPropertyChanged(nameof(ProjectDataVisibility));
            OnPropertyChanged(nameof(RecentProjectsListVisibility));
            OnPropertyChanged(nameof(ElementPageAvailable));
        }
    }
}
