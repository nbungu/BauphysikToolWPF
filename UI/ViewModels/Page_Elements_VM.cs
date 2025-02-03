using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.CustomControls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_Elements.xaml: Used in xaml as "DataContext"
    public partial class Page_Elements_VM : ObservableObject
    {
        public Page_Elements_VM()
        {
            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            
            UserSaved.NewProjectAdded += UpdateNewProjectAdded;
            UserSaved.NewElementAdded += UpdateOnNewElementAdded;
            UserSaved.ElementRemoved += UpdateOnElementRemoved;
            UserSaved.SelectedElementChanged += UpdateOnElementChanged;

            // If Images are not rendered yet
            UserSaved.SelectedProject.RenderAllElementImages();
        }

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext

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
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddElementWindow().ShowDialog();
        }

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
            UserSaved.OnElementRemoved();
        }

        [RelayCommand]
        private void DeleteAllElements()
        {
            // Delete all Elements
            UserSaved.SelectedProject.Elements.Clear();
            UserSaved.OnElementRemoved();
        }

        [RelayCommand]
        private void CopyElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            UserSaved.SelectedElementId = selectedInternalId;
            UserSaved.SelectedProject.Elements.Add(UserSaved.SelectedElement.Copy());
            UserSaved.OnNewElementAdded();
        }

        [RelayCommand]
        private void SelectElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            UserSaved.SelectedElementId = selectedInternalId;
            UserSaved.Recalculate = true;
            UpdateOnElementChanged();
        }

        [RelayCommand]
        private void ElementDoubleClick()
        {
            SwitchPage(NavigationContent.LayerSetup);
        }

        [RelayCommand]
        private void CreateSingleElementPdf(int selectedInternalId)
        {
            UserSaved.SelectedElementId = selectedInternalId;
            DocumentDesigner.CreateSingleElementDocument(UserSaved.SelectedElement);
        }

        [RelayCommand]
        private void CreateFullPdf()
        {
            DocumentDesigner.FullCatalogueExport(UserSaved.SelectedProject);
        }

        // This method will be called whenever SortingPropertyIndex changes
        // Workaround since Combobox has no Command or Click option
        partial void OnSortingPropertyIndexChanged(int value)
        {
            UserSaved.SelectedProject.Elements.Sort(new ElementComparer(SelectedSorting));
            UpdateOnElementChanged();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         *
         * e.g.: Everything the user can edit or change: All objects affected by user interaction.
         *
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedSorting))]
        private static int _sortingPropertyIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGroupingEnabled))]
        [NotifyPropertyChangedFor(nameof(SelectedGrouping))]
        [NotifyPropertyChangedFor(nameof(GroupedElements))]
        private static int _groupingPropertyIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        [NotifyPropertyChangedFor(nameof(ElementInfoVisibility))]
        private static Element? _selectedElement; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ExportPdfCatalogueAvailable))]
        [NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        [NotifyPropertyChangedFor(nameof(ElementInfoVisibility))]
        private List<Element> _elements = UserSaved.SelectedProject.Elements;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public bool ElementToolsAvailable => UserSaved.SelectedElementId != -1;
        public bool ExportPdfCatalogueAvailable => false; // TODO: Elements.Count > 0;

        // Returns False if Index is 0. Index 0 means without Grouping, since "Ohne" is first entry in Combobox
        public bool IsGroupingEnabled => GroupingPropertyIndex > 0;

        // For Grouping and Sorting of WrapPanel: Expose as Static for 'GroupingTypeToPropertyName' Converter
        public static ElementSortingType SelectedSorting => (ElementSortingType)_sortingPropertyIndex;
        public static ElementGroupingType SelectedGrouping => (ElementGroupingType)_groupingPropertyIndex;
        public Visibility ElementInfoVisibility => ElementToolsAvailable ? Visibility.Visible : Visibility.Collapsed;
        public List<string> SortingProperties => ElementComparer.SortingTypes; // Has to match ElementSortingType enum values (+Order)
        public List<string> GroupingProperties => ElementComparer.GroupingTypes; // Has to match ElementSortingType enum values (+Order)
        public ICollectionView? GroupedElements => UserSaved.SelectedProject.Elements.Count > 0 ? GetGroupedItemsSource() : null;

        private void UpdateOnElementChanged()
        {
            // only update UI
            RefreshXamlBindings();
        }

        private void UpdateOnNewElementAdded()
        {
            // Update InternalIds
            UserSaved.SelectedProject.AssignInternalIdsToElements();
            // Set selected to newest element
            UserSaved.SelectedElementId = UserSaved.SelectedProject.Elements.Last().InternalId;
            // update UI
            RefreshXamlBindings();
        }

        private void UpdateNewProjectAdded()
        {
            // reset SelectedElement
            UserSaved.SelectedElementId = -1;

            // Update InternalIds and render new images
            UserSaved.SelectedProject.AssignInternalIdsToElements();
            UserSaved.SelectedProject.RenderAllElementImages();
            // update UI
            RefreshXamlBindings();
        }
        private void UpdateOnElementRemoved()
        {
            // reset SelectedElement
            UserSaved.SelectedElementId = -1;
            // set selected element to last
            if (UserSaved.SelectedProject.Elements.Count > 0)
                UserSaved.SelectedElementId = UserSaved.SelectedProject.Elements.Last().InternalId;
            // update UI
            RefreshXamlBindings();
        }

        private void RefreshXamlBindings()
        {
            // Trigger re-grouping
            OnPropertyChanged(nameof(GroupedElements));

            Elements = new List<Element>();
            Elements = UserSaved.SelectedProject.Elements;

            SelectedElement = null;
            SelectedElement = UserSaved.SelectedElementId == -1 ? null : UserSaved.SelectedElement;
        }

        private ICollectionView GetGroupedItemsSource()
        {
            var cvs = new CollectionViewSource { Source = UserSaved.SelectedProject.Elements };
            var pgd = new PropertyGroupDescription(".", new GroupingTypeToPropertyName(SelectedGrouping));
            cvs.GroupDescriptions.Add(pgd);
            return cvs.View;
        }
    }
}
