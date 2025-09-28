using BauphysikToolWPF.Models.Application;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Session.SelectedProjectChanged += UpdateXamlBindings;
            Session.NewProjectAdded += UpdateXamlBindings;
            Session.SelectedLayerChanged += RefreshIsEditedTag;
            Session.SelectedElementChanged += RefreshIsEditedTag;
            Session.PageChanged += OnPageChanged;

            _dialogService = new DialogService();
            _fileDialogService = new FileDialogService();
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

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
                        MainWindow.SetPage(NavigationPage.ProjectData);
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
                MainWindow.SetPage(NavigationPage.ProjectData);
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
                ProjectSerializer.SaveProjectToFile(Session.SelectedProject, Session.ProjectFilePath);
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
            if (Session.SelectedProject != null && Session.SelectedProject.IsModified)
            {
                MessageBoxResult result = _dialogService.ShowSaveConfirmationDialog();
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            string? filePath = _fileDialogService.ShowOpenFileDialog("BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
            if (filePath != null)
            {
                Project loadedProject = ProjectSerializer.GetProjectFromFile(filePath);
                Session.SelectedProject = loadedProject;
                Session.SelectedProject.IsModified = false;
                Session.ProjectFilePath = filePath;
                RecentProjectsManager.AddRecentProject(filePath);

                Session.OnNewProjectAdded(false);
                MainWindow.SetPage(NavigationPage.ElementCatalogue);
            }
        }

        [RelayCommand]
        private void SetPageButtonClick(NavigationPage targetPage) => MainWindow.SetPage(targetPage);

        [RelayCommand]
        private void ShowInfo() => new InfoWindow().ShowDialog();

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */



        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because no direct input from User
         */


        public string Title => Session.SelectedProject != null ? $"Projekt: {ProjectName}   {Session.ProjectFilePath}" : Session.ProjectFilePath;
        public string ProjectName => Session.SelectedProject != null ? Session.SelectedProject.Name : "Noch kein Projekt geladen";
        public string IsEditedTagColorCode => Session.SelectedProject != null && Session.SelectedProject.IsModified ? "#1473e6" : "#00FFFFFF";
        public bool IsProjectLoaded => !Session.SelectedProject?.IsNewEmptyProject ?? false;
        public Visibility SaveButtonVisibility => IsProjectLoaded ? Visibility.Visible : Visibility.Collapsed;

        #region Page Navigation
        public NavigationPage CurrentParentPage { get; set; }
        public List<NavigationContent> ParentPages => NavigationManager.ParentPages;
        public List<NavigationGroupContent> AvailableChildGroups => NavigationManager.ParentPages.FirstOrDefault(p => p.Page == CurrentParentPage)?.PageGroups ?? new List<NavigationGroupContent>();

        private void OnPageChanged(NavigationPage target, NavigationPage? origin)
        {
            // When targetPage is a parent page, set as current parent page
            if (NavigationManager.ParentPageDictionary.ContainsKey(target)) CurrentParentPage = target;

            NavigationManager.UpdateParentEnabledStates();
            // TODO: Temporär:
            NavigationManager.DisablePage(NavigationPage.BuildingEnvelope);
            //
            target.UpdateParentSelectedState();
            target.UpdateChildSelectedState(AvailableChildGroups);

            // Custom
            if (origin == NavigationPage.ElementCatalogue)
                NavigationManager.ParentPageDictionary[NavigationPage.ElementCatalogue].PageGroups?.ForEach(grp => grp.IsEnabled = true);
            if (target == NavigationPage.ElementCatalogue)
                NavigationManager.ParentPageDictionary[NavigationPage.ElementCatalogue].PageGroups?.ForEach(grp => grp.IsEnabled = false);
            
            OnPropertyChanged(nameof(AvailableChildGroups));

            // Dont need sice relevant collection properties have INotifyPropertyChanged implemented
            //OnPropertyChanged(nameof(ParentPages));
        }

        #endregion

        private void UpdateXamlBindings()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ProjectName));            
            OnPropertyChanged(nameof(IsEditedTagColorCode));
            OnPropertyChanged(nameof(IsProjectLoaded));
            OnPropertyChanged(nameof(SaveButtonVisibility));
            OnPropertyChanged(nameof(AvailableChildGroups));
        }

        private void RefreshIsEditedTag()
        {
            OnPropertyChanged(nameof(IsEditedTagColorCode));
        }
    }
}
