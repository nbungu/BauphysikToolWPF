using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for MainWindow.xaml: Used in xaml as "DataContext"
    public partial class MainWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from MainWindow.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "Main";

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

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Project _currentProject = DatabaseAccess.QueryProjectById(FO0_ProjectPage.SelectedProjectId);
    }
}
