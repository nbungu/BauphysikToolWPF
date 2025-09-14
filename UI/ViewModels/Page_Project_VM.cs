using BauphysikToolWPF.Models.Application;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
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
using static BauphysikToolWPF.Models.Domain.Enums;

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
            _dialogService = new DialogService();
            _fileDialogService = new FileDialogService();

            // Allow other UserControls to trigger RefreshXamlBindings of this Window
            Session.SelectedProjectChanged += UpdateXamlBindings;
            Session.NewProjectAdded += UpdateXamlBindings;
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void Next()
        {
            SwitchPage(NavigationPage.ElementCatalogue);
        }

        [RelayCommand]
        private void New()
        {
            if (Session.SelectedProject != null && Session.SelectedProject.IsModified)
            {
                MessageBoxResult result = _dialogService.ShowSaveConfirmationDialog();

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        ProjectFactory.CreateNewProject();
                        Session.OnNewProjectAdded(false);
                        break;
                    case MessageBoxResult.Cancel:
                        // Do nothing, user cancelled the action
                        break;
                }
            }
            else
            {
                ProjectFactory.CreateNewProject();
                Session.OnNewProjectAdded(false);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (Session.SelectedProject is null) return;
            if (!File.Exists(Session.ProjectFilePath)) SaveTo();
            else
            {
                ProjectSerializer.SaveProjectToFile(Session.SelectedProject, Session.ProjectFilePath);
                Session.SelectedProject.IsModified = false;
                Session.OnSelectedProjectChanged(false);
            }
        }

        [RelayCommand]
        private void SaveTo()
        {
            if (Session.SelectedProject is null) return;
            string? filePath = _fileDialogService.ShowSaveFileDialog("project.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                ProjectSerializer.SaveProjectToFile(Session.SelectedProject, filePath);
                Session.ProjectFilePath = filePath;
                Session.SelectedProject.IsModified = false;
                RecentProjectsManager.AddRecentProject(filePath);
                Session.OnSelectedProjectChanged(false);
            }
        }
        [RelayCommand]
        private void Open()
        {
            string? filePath = _fileDialogService.ShowOpenFileDialog("BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                Project loadedProject = ProjectSerializer.GetProjectFromFile(filePath);

                Session.SelectedProject = loadedProject;
                Session.ProjectFilePath = filePath;
                Session.SelectedProject.IsModified = false;
                RecentProjectsManager.AddRecentProject(filePath);
                Session.OnNewProjectAdded(false);
                SwitchPage(NavigationPage.ElementCatalogue);
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
                    string programDataPath = PathService.LocalProgramDataPath;
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
            if (Session.SelectedProject is null) return;

            DroppedFilePaths.Remove(file);
            Session.SelectedProject.LinkedFilesList = DroppedFilePaths.ToList();
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        [RelayCommand]
        private void OpenRecentProject()
        {
            if (SelectedListViewItem is null) return;

            var filePath = SelectedListViewItem.FilePath;
            Project loadedProject = ProjectSerializer.GetProjectFromFile(filePath);

            Session.SelectedProject = loadedProject;
            Session.ProjectFilePath = filePath;
            RecentProjectsManager.AddRecentProject(filePath);
            Session.OnNewProjectAdded(false);

            SwitchPage(NavigationPage.ElementCatalogue);
        }

        partial void OnAuthorNameChanged(string value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.UserName = value;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        partial void OnProjectNameChanged(string value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.Name = value;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        partial void OnCommentChanged(string value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.Comment = value;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        partial void OnIsNewConstrCheckedChanged(bool value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.BuildingAge = value ? BuildingAgeType.New : BuildingAgeType.Existing;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }
        partial void OnIsResidentialUsageCheckedChanged(bool value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.BuildingUsage = value ? BuildingUsageType.Residential : BuildingUsageType.NonResidential;
            Session.SelectedProject.IsModified = true;
            Session.OnSelectedProjectChanged();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        private string _projectName = Session.SelectedProject?.Name ?? "";

        [ObservableProperty]
        private string _authorName = Session.SelectedProject?.UserName ?? "";

        [ObservableProperty]
        private string _comment = Session.SelectedProject?.Comment ?? "";

        [ObservableProperty]
        private ObservableCollection<string> _droppedFilePaths = Session.SelectedProject != null ? new ObservableCollection<string>(Session.SelectedProject.LinkedFilesList) : new ObservableCollection<string>();

        [ObservableProperty]
        private bool _isResidentialUsageChecked = Session.SelectedProject?.BuildingUsage == BuildingUsageType.Residential;

        [ObservableProperty]
        private bool _isNewConstrChecked = Session.SelectedProject?.BuildingAge == BuildingAgeType.New;

        [ObservableProperty]
        private bool _isNonResidentialUsageChecked = Session.SelectedProject?.BuildingUsage == BuildingUsageType.NonResidential;

        [ObservableProperty]
        private bool _isExistingConstrChecked = Session.SelectedProject?.BuildingAge == BuildingAgeType.Existing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ElementPageAvailable))]
        private RecentProjectItem? _selectedListViewItem;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public List<RecentProjectItem> RecentProjects { get; set; } = RecentProjectsManager.LoadRecentProjects();
        public Visibility ProjectDataVisibility => Session.SelectedProject != null && Session.SelectedProject.CreatedByUser ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RecentProjectsListVisibility => ProjectDataVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        public Visibility RecentProjectEntriesVisibility => RecentProjects.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoRecentProjectEntriesVisibility => RecentProjects.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public bool ElementPageAvailable => ProjectDataVisibility == Visibility.Visible || SelectedListViewItem != null;

        private void UpdateXamlBindings()
        {
            // For updating fields/variables
            ProjectName = Session.SelectedProject?.Name ?? "";
            AuthorName = Session.SelectedProject?.UserName ?? "";
            Comment = Session.SelectedProject?.Comment ?? "";
            // TODO: triggers Project changed three times up until here -> MainWindow VM updates

            DroppedFilePaths = Session.SelectedProject != null ? new ObservableCollection<string>(Session.SelectedProject.LinkedFilesList) : new ObservableCollection<string>();
            IsResidentialUsageChecked = Session.SelectedProject?.BuildingUsage == BuildingUsageType.Residential;
            IsNonResidentialUsageChecked = Session.SelectedProject?.BuildingUsage == BuildingUsageType.NonResidential;
            IsNewConstrChecked = Session.SelectedProject?.BuildingAge == BuildingAgeType.New;
            IsExistingConstrChecked = Session.SelectedProject?.BuildingAge == BuildingAgeType.Existing;

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(ProjectDataVisibility));
            OnPropertyChanged(nameof(RecentProjectsListVisibility));
            OnPropertyChanged(nameof(ElementPageAvailable));
        }
    }
}
