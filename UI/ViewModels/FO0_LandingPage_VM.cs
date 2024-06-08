using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.SQLiteRepo.Helper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_LandingPage_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "LandingPage";

        /*
         * Static Class Properties:
         * If List is already loaded, use existing - static - List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */

        public List<string> SortingProperties => ElementOrganisor.SortingTypes; // Has to match ElementSortingType enum values (+Order)

        public List<string> GroupingProperties => ElementOrganisor.GroupingTypes; // Has to match ElementSortingType enum values (+Order)
        
        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            if (FO0_LandingPage.SelectedElementId != -1) MainWindow.SetPage(desiredPage);
        }

        // Create New Element / Edit Existing Element
        [RelayCommand]
        private void EditElement(int? selectedElementId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            var window = (selectedElementId is null) ? new EditElementWindow() : new EditElementWindow(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            if (selectedElementId is null) return;

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.SelectedProjectId, (ElementSortingType)_sortingPropertyIndex, _isAscending);
        }

        [RelayCommand]
        private void DeleteElement(int? selectedElementId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            if (selectedElementId is null) return;

            // Delete selected Element
            DatabaseAccess.DeleteElementById(Convert.ToInt32(selectedElementId));

            // When deleting the Element which was currently SelectedElement
            if (selectedElementId == FO0_LandingPage.SelectedElementId)
            {
                // Reset Selected Element
                FO0_LandingPage.SelectedElementId = -1;
                // Update XAML Binding Property
                SelectedElementId = FO0_LandingPage.SelectedElementId;
            }

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.SelectedProjectId, (ElementSortingType)_sortingPropertyIndex, _isAscending);
        }

        [RelayCommand]
        private void DeleteAllElements()
        {
            // Delete selected Layer
            DatabaseAccess.DeleteAllElements();

            // Reset Selected Layer
            FO0_LandingPage.SelectedElementId = -1;

            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.SelectedProjectId);
            SelectedElementId = FO0_LandingPage.SelectedElementId;
        }

        [RelayCommand]
        private void SelectElement(int? selectedElementId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId is null) return;

            // Set the currently selected Element
            FO0_LandingPage.SelectedElementId = Convert.ToInt32(selectedElementId);

            // Update XAML Binding Property
            SelectedElementId = FO0_LandingPage.SelectedElementId;
        }

        [RelayCommand]
        private void CopyElement(int? selectedElementId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId is null) return;

            // Create copy of selected Element and add to DB
            Element elementCopy = DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId));
            elementCopy.Name += "-Copy";
            DatabaseAccess.CreateElement(elementCopy, withChildren: true);

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.SelectedProjectId, (ElementSortingType)_sortingPropertyIndex, _isAscending);
        }

        [RelayCommand]
        private void ChangeSortingOrder()
        {
            // Change sorting order
            IsAscending = !IsAscending;

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.SelectedProjectId, (ElementSortingType)_sortingPropertyIndex, _isAscending);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedSorting))]
        private static int _sortingPropertyIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGroupingEnabled))]
        [NotifyPropertyChangedFor(nameof(SelectedGrouping))]
        private static int _groupingPropertyIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        private static bool _isAscending = true; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedElement))]
        private List<Element> _elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.SelectedProjectId, (ElementSortingType)_sortingPropertyIndex, _isAscending) ?? new List<Element>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedElement))]
        [NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        private int _selectedElementId = FO0_LandingPage.SelectedElementId;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        // TODO: layersSorted: false -> viel schnellere reaktion
        public Element? SelectedElement => (SelectedElementId != -1) ? DatabaseAccess.QueryElementById(SelectedElementId) : null; 

        public bool ElementToolsAvailable => SelectedElementId != -1;

        // Returns False if Index is 0. Index 0 means without Grouping, since "Ohne" is first entry in Combobox
        public bool IsGroupingEnabled => GroupingPropertyIndex > 0;

        // For Grouping and Sorting of WrapPanel: Expose as Static for 'GroupingTypeToPropertyName' Converter
        public static ElementSortingType SelectedSorting => (ElementSortingType)_sortingPropertyIndex;

        public static ElementGroupingType SelectedGrouping => (ElementGroupingType)_groupingPropertyIndex;
    }
}
