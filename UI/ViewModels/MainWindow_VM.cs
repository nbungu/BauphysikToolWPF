using System.IO;
using System.Windows;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Dialogs;
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
            UserSaved.SelectedProjectChanged += RefreshXamlBindings;
            UserSaved.NewProjectAdded += RefreshXamlBindings;
            UserSaved.SelectedLayerChanged += RefreshXamlBindings;
            UserSaved.SelectedElementChanged += RefreshXamlBindings;

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
                        UserSaved.OnNewProjectAdded(false);
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
                UserSaved.OnNewProjectAdded(false);
                SwitchPage(NavigationContent.ProjectPage);
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

            string saveFileName = UserSaved.SelectedProject.Name == "" ? "unbekannt" : UserSaved.SelectedProject.Name;
            string? filePath = _fileDialogService.ShowSaveFileDialog($"{saveFileName}.btk", "BTK Files (*.btk)|*.btk|All Files (*.*)|*.*");
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
                UserSaved.SelectedProject.IsModified = false;
                UserSaved.ProjectFilePath = filePath;
                UserSaved.OnNewProjectAdded(false);
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

        public string Title => $"'{UserSaved.SelectedProject.Name}' - {UserSaved.ProjectFilePath}";
        public string ProjectName => UserSaved.SelectedProject.Name == "" ? "-" : UserSaved.SelectedProject.Name;
        public string IsEditedTagColorCode => UserSaved.SelectedProject.IsModified ? "#1473e6" : "#00FFFFFF";

        private void RefreshXamlBindings()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ProjectName));            
            OnPropertyChanged(nameof(IsEditedTagColorCode));
        }
    }
}
