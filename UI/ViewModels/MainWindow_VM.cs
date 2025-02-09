using System.IO;
using System.Windows;
using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        private void SwitchPage(NavigationContent desiredPage)
        {
            MainWindow.SetPage(desiredPage);
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
                        SwitchPage(NavigationContent.ProjectPage);
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
                SwitchPage(NavigationContent.ProjectPage);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (!File.Exists(Session.ProjectFilePath))
            {
                SaveTo();
            }
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

            string saveFileName = Session.SelectedProject.Name == "" ? "unbekannt" : Session.SelectedProject.Name;
            string? filePath = _fileDialogService.ShowSaveFileDialog($"{saveFileName}.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
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
                Session.SelectedProject.IsModified = false;
                Session.ProjectFilePath = filePath;
                ApplicationServices.AddRecentProject(filePath);

                Session.OnNewProjectAdded(false);
                SwitchPage(NavigationContent.LandingPage);
            }
        }

        [RelayCommand]
        private void ShowInfo()
        {
            new InfoWindow().ShowDialog();
        }

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

        public string Title => Session.SelectedProject.Name == "" ? $"{Session.ProjectFilePath}" : $"Projekt: {ProjectName}   {Session.ProjectFilePath}";
        public string ProjectName => Session.SelectedProject.Name == "" ? "-" : Session.SelectedProject.Name;
        public string IsEditedTagColorCode => Session.SelectedProject.IsModified ? "#1473e6" : "#00FFFFFF";

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ProjectName));            
            OnPropertyChanged(nameof(IsEditedTagColorCode));
        }
    }
}
