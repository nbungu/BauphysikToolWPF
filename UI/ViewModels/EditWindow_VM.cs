using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for MainWindow.xaml: Used in xaml as "DataContext"
    public partial class EditWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from EditLayerWindow.cs and EditElementWindow.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "EditWindow";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */



        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */
    }
}
