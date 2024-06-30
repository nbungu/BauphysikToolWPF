using System.Collections.Generic;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.CustomControls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class TestWindow_VM : ObservableObject
    {
        

        /*
         * Regular Instance Variables
         * 
         * Not depending on UI changes. No Observable function.
         */

        
        //public LiveChartsCore.Measure.Margin ChartMargin_i { get; private set; } = new LiveChartsCore.Measure.Margin(64, 16, 0, 64);

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
        private void EditElement() // Binding in XAML via 'EditElementCommand'
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditElementWindow().ShowDialog();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        [ObservableProperty]
        private List<PropertyItem> _properties = new List<PropertyItem>
        {
            new PropertyItem { Name = "Property1", Value = "Value1" },
            new PropertyItem { Name = "Property2", Value = "Value2", PropertyValues = new string[] { "Value2", "Value3", "Value4" } },
            new PropertyItem { Name = "Property3", Value = "Value3" },
            new PropertyItem { Name = "Property4", Value = "Value4", PropertyValues = new string[] { "Value2", "Value3", "Value4" } }
        };


        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the Values above
         */


    }
}
