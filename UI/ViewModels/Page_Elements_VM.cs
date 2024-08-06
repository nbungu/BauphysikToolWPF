using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.CustomControls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using BauphysikToolWPF.Services;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_Elements.xaml: Used in xaml as "DataContext"
    public partial class Page_Elements_VM : ObservableObject
    {
        public Page_Elements_VM()
        {
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            UserSaved.SelectedElementChanged += RefreshXamlBindings;
        }

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext
        public string Title => "LandingPage";
        public List<string> SortingProperties => ElementComparer.SortingTypes; // Has to match ElementSortingType enum values (+Order)
        public List<string> GroupingProperties => ElementComparer.GroupingTypes; // Has to match ElementSortingType enum values (+Order)
        
        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            if (SelectedElement is null) return;
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void AddNewElement()
        {
            UserSaved.SelectedElementId = -1; // Reset SelectedElement
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddElementWindow().ShowDialog();
        }

        // Create New Element / Edit Existing Element
        [RelayCommand]
        private void EditElement(int selectedInternalId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            UserSaved.SelectedElementId = selectedInternalId;
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddElementWindow().ShowDialog();
        }

        [RelayCommand]
        private void DeleteElement(int selectedInternalId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            // Delete selected Element
            UserSaved.SelectedProject.Elements.RemoveAll(e => e.InternalId == selectedInternalId);
            UserSaved.SelectedElementId = -1; // Reset SelectedElement
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void DeleteAllElements()
        {
            // Delete all Elements
            UserSaved.SelectedProject.Elements.Clear();
            UserSaved.SelectedElementId = -1; // Reset SelectedElement
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void CopyElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            UserSaved.SelectedElementId = selectedInternalId;
            UserSaved.SelectedProject.Elements.Add(UserSaved.SelectedElement.Copy());
            UserSaved.SelectedElementId = -1; // Reset SelectedElement
            UserSaved.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void SelectElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            UserSaved.SelectedElementId = selectedInternalId;
            UserSaved.Recalculate = true;
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void ElementDoubleClick()
        {
            SwitchPage(NavigationContent.LayerSetup);
        }

        [RelayCommand]
        private void CreatePdf()
        {
            DocumentDesigner.SingleElementOverview();
        }

        // This method will be called whenever SortingPropertyIndex changes
        // Workaround since Combobox has no Command or Click option
        partial void OnSortingPropertyIndexChanged(int value)
        {
            UserSaved.SelectedProject.Elements.Sort(new ElementComparer(SelectedSorting));
            RefreshXamlBindings();
        }
        
        // This method will be called whenever SortingPropertyIndex changes
        // Workaround since Combobox has no Command or Click option
        partial void OnGroupingPropertyIndexChanged(int value)
        {
            GroupedElements = UpdateGroupedItemsSource();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         *
         * e.g.: when _sortingPropertyIndex changes, the PropertyChanged event should also be raised for the properties SelectedSorting
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
        [NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        private static Element? _selectedElement; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        private List<Element> _elements = UserSaved.SelectedProject.Elements;

        [ObservableProperty]
        private ICollectionView? _groupedElements;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public bool ElementToolsAvailable => SelectedElement != null;

        // Returns False if Index is 0. Index 0 means without Grouping, since "Ohne" is first entry in Combobox
        public bool IsGroupingEnabled => GroupingPropertyIndex > 0;

        // For Grouping and Sorting of WrapPanel: Expose as Static for 'GroupingTypeToPropertyName' Converter
        public static ElementSortingType SelectedSorting => (ElementSortingType)_sortingPropertyIndex;

        public static ElementGroupingType SelectedGrouping => (ElementGroupingType)_groupingPropertyIndex;


        private void RefreshXamlBindings()
        {
            // Update InternalIds and reset SelectedElement
            UserSaved.SelectedProject.AssignInternalIdsToElements();

            // Trigger re-grouping
            OnGroupingPropertyIndexChanged(_groupingPropertyIndex);
            Elements = new List<Element>();
            Elements = UserSaved.SelectedProject.Elements;
            SelectedElement = null;
            SelectedElement = UserSaved.SelectedElement;
        }

        private ICollectionView UpdateGroupedItemsSource()
        {
            var cvs = new CollectionViewSource { Source = UserSaved.SelectedProject.Elements };
            var pgd = new PropertyGroupDescription(".", new GroupingTypeToPropertyName(SelectedGrouping));
            cvs.GroupDescriptions.Add(pgd);
            return cvs.View;
        }
    }
}
