using BauphysikToolWPF.Models.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddElementWindow.xaml: Used in xaml as "DataContext"
    public partial class PropertyWindow_VM : ObservableObject
    {
        //private readonly Element? _targetElement = Session.SelectedProject?.Elements.FirstOrDefault(e => e?.InternalId == AddElementWindow.TargetElementInternalId, null);

        // Called by 'InitializeComponent()' from PropertyWindow.cs due to Class-Binding in xaml via DataContext
        public PropertyWindow_VM() { }
        
        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void ApplyChanges(Window? window)
        {
            // To be able to Close EditElementWindow from within this ViewModel
            if (window is null) return;
            window.Close();
        }

        [RelayCommand]
        private void Cancel(Window? window)
        {
            // To be able to Close EditElementWindow from within this ViewModel
            if (window is null) return;
            window.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        //[ObservableProperty]
        //private string _selectedElementName;


        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */
        public string Title { get; set; }
        public string PropertyBagTitle { get; set; }
        public IEnumerable<IPropertyItem> PropertyBag { get; set; }
    }
}
