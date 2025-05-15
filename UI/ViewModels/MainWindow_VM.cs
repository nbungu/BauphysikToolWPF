using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
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
            Session.SelectedProjectChanged += RefreshXamlBindings;
            Session.NewProjectAdded += RefreshXamlBindings;
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
                        DomainModelFactory.CreateNewProject();
                        Session.OnNewProjectAdded(false);
                        MainWindow.SetPage(NavigationPage.ProjectPage);
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
                MainWindow.SetPage(NavigationPage.ProjectPage);
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
                MainWindow.SetPage(NavigationPage.ElementCatalogue);
            }
        }

        [RelayCommand]
        private void ParentPageButtonClick(NavigationPage parentPage)
        {
            MainWindow.SetPage(parentPage);
        }

        [RelayCommand]
        private void ChildPageButtonClick(NavigationPage childPage)
        {
            MainWindow.SetPage(childPage);
        }

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
        
        public List<NavigationContent> ParentPages => MainWindow.ParentPages;
        public NavigationContent? SelectedParentPageItem => ParentPages.FirstOrDefault(p => p.IsSelected);
        public IEnumerable<NavigationGroupContent> AvailableChildGroups => SelectedParentPageItem?.PageGroups?.Where(grp => grp.IsEnabled) ?? new List<NavigationGroupContent>();

        #endregion

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ProjectName));            
            OnPropertyChanged(nameof(IsEditedTagColorCode));
            OnPropertyChanged(nameof(IsProjectLoaded));
            OnPropertyChanged(nameof(SaveButtonVisibility));
            OnPropertyChanged(nameof(ParentPages));
            OnPropertyChanged(nameof(AvailableChildGroups));
        }

        private void OnPageChanged(NavigationPage target, NavigationPage? origin)
        {
            ParentPages.ForEach(p => p.IsEnabled = IsProjectLoaded);

            if (MainWindow.ParentPageDictionary.ContainsKey(target)) ParentPages.ForEach(p => p.IsSelected = p.Page == target);
            
            if (origin == NavigationPage.ElementCatalogue)
                MainWindow.ParentPageDictionary[NavigationPage.ElementCatalogue].PageGroups?.ForEach(grp => grp.IsEnabled = true);
            if (target == NavigationPage.ElementCatalogue)
                MainWindow.ParentPageDictionary[NavigationPage.ElementCatalogue].PageGroups?.ForEach(grp => grp.IsEnabled = false);

            // Dont need sice relevant collection properties have INotifyPropertyChanged implemented
            //OnPropertyChanged(nameof(ParentPages));
            //OnPropertyChanged(nameof(AvailableChildGroups));
        }

        private void RefreshIsEditedTag()
        {
            OnPropertyChanged(nameof(IsEditedTagColorCode));
        }
    }
}
