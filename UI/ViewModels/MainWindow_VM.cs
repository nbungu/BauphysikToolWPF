using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for MainWindow.xaml: Used in xaml as "DataContext"
    public partial class MainWindow_VM : ObservableObject
    {
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;

        // Called by 'InitializeComponent()' from MainWindow.cs due to Class-Binding in xaml via DataContext
        public MainWindow_VM()
        {
            
            Session.SelectedProjectChanged += RefreshXamlBindings;
            Session.NewProjectAdded += RefreshXamlBindings;
            // TODO: necessary?
            Session.SelectedLayerChanged += RefreshXamlBindings;
            Session.SelectedElementChanged += RefreshXamlBindings;

            _dialogService = new DialogService();
            _fileDialogService = new FileDialogService();
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
                        DomainModelFactory.CreateNewProject();
                        Session.OnNewProjectAdded(false);
                        SwitchPage(NavigationPage.ProjectPage);
                        break;
                    case MessageBoxResult.Cancel:
                        // Do nothing, user cancelled the action
                        break;
                }
            }
            else
            {
                DomainModelFactory.CreateNewProject();
                Session.OnNewProjectAdded(false);
                SwitchPage(NavigationPage.ProjectPage);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (Session.SelectedProject is null) return;

            if (!File.Exists(Session.ProjectFilePath))
            {
                SaveTo();
            }
            else
            {
                DomainModelSerializer.SaveProjectToFile(Session.SelectedProject, Session.ProjectFilePath);
                Session.SelectedProject.IsModified = false;
                Session.OnSelectedProjectChanged(false);
            }
        }

        [RelayCommand]
        private void SaveTo()
        {
            if (Session.SelectedProject is null) return;

            string saveFileName = Session.SelectedProject.Name == "" ? "unbekannt" : Session.SelectedProject.Name;
            string? filePath = _fileDialogService.ShowSaveFileDialog($"{saveFileName}.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                DomainModelSerializer.SaveProjectToFile(Session.SelectedProject, filePath);
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
                Project loadedProject = DomainModelSerializer.GetProjectFromFile(filePath);
                Session.SelectedProject = loadedProject;
                Session.SelectedProject.IsModified = false;
                Session.ProjectFilePath = filePath;
                RecentProjectsManager.AddRecentProject(filePath);

                Session.OnNewProjectAdded(false);
                SwitchPage(NavigationPage.ElementCatalogue);
            }
        }

        [RelayCommand]
        private void ShowInfo()
        {
            new InfoWindow().ShowDialog();
        }

        partial void OnSelectedParentPageItemChanged(NavigationContent? value)
        {
            if (value != null) MainWindow.SetPage(value.Page);
        }

        partial void OnSelectedChildPageItemChanged(NavigationContent? value)
        {
            if (value != null) MainWindow.SetPage(value.Page);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvailableNavigationGroups))]
        private NavigationContent? _selectedParentPageItem;

        [ObservableProperty]
        private NavigationContent? _selectedChildPageItem;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because no direct input from User
         */

        public List<NavigationContent> ParentPages { get; set; } = MainWindow.NavigationContent;
        public List<NavigationGroupContent> AvailableNavigationGroups => SelectedParentPageItem?.GroupContent ?? new List<NavigationGroupContent>();
        public string Title => Session.SelectedProject != null ? $"Projekt: {ProjectName}   {Session.ProjectFilePath}" : Session.ProjectFilePath;
        public string ProjectName => Session.SelectedProject != null ? Session.SelectedProject.Name : "Noch kein Projekt geladen";
        public string IsEditedTagColorCode => Session.SelectedProject != null && Session.SelectedProject.IsModified ? "#1473e6" : "#00FFFFFF";
        public bool IsProjectLoaded => !Session.SelectedProject?.IsNewEmptyProject ?? false;
        public Visibility SaveButtonVisibility => IsProjectLoaded ? Visibility.Visible : Visibility.Collapsed;

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ProjectName));            
            OnPropertyChanged(nameof(IsEditedTagColorCode));
            OnPropertyChanged(nameof(IsProjectLoaded));
            OnPropertyChanged(nameof(SaveButtonVisibility));
        }

        //private void UpdateSelectedChildPages()
        //{
        //    AvailableNavigationGroups.Clear();
        //    if (SelectedParentPageItem?.GroupContent != null)
        //    {
        //        foreach (var group in SelectedParentPageItem.GroupContent)
        //        {
        //            foreach (var page in group.ChildPages)
        //            {
        //                SelectedChildPages.Add(page);
        //            }
        //        }
        //    }
        //}
    }
}
