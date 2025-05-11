using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
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
            if (Session.SelectedProject is null) return;

            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            Session.SelectedProject.AssignInternalIdsToElements();
            Session.SelectedProject.Elements.Sort(new ElementComparer(SelectedSorting));

            Session.NewProjectAdded += UpdateNewProjectAdded;
            Session.NewElementAdded += UpdateOnNewElementAdded;
            Session.ElementRemoved += UpdateOnElementRemoved;
            Session.SelectedElementChanged += RefreshXamlBindings;

            // If Images are not rendered yet
            Session.SelectedProject.RenderMissingElementImages();
        }

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage)
        {
            if (SelectedElement is null) return;
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void AddNewElement()
        {
            Session.SelectedElementId = -1;
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddElementWindow().ShowDialog();
        }

        [RelayCommand]
        private void EditElement(int selectedInternalId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            Session.SelectedElementId = selectedInternalId;
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddElementWindow(Session.SelectedElementId).ShowDialog();
        }

        [RelayCommand]
        private void DeleteElement(int selectedInternalId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            // Delete selected Element
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.Elements.RemoveAll(e => e.InternalId == selectedInternalId);
            Session.OnElementRemoved();
        }

        [RelayCommand]
        private void DeleteAllElements()
        {
            // Delete all Elements
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.Elements.Clear();
            Session.OnElementRemoved();
        }

        [RelayCommand]
        private void CopyElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            Session.SelectedElementId = selectedInternalId;

            if (Session.SelectedProject is null) return;
            if (Session.SelectedElement is null) return;
            Session.SelectedProject.Elements.Add(Session.SelectedElement.Copy());
            Session.OnNewElementAdded();
        }

        [RelayCommand]
        private void SelectElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            Session.SelectedElementId = selectedInternalId;
            Session.SelectedElement.Recalculate = true;
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void ElementDoubleClick()
        {
            SwitchPage(NavigationPage.LayerSetup);
        }

        [RelayCommand]
        private void CreateSingleElementPdf(int selectedInternalId)
        {
            Session.SelectedElementId = selectedInternalId;
            DocumentDesigner.CreateSingleElementDocument(Session.SelectedProject, Session.SelectedElement);
        }

        [RelayCommand]
        private void CreateFullPdf()
        {
            DocumentDesigner.FullCatalogueExport(Session.SelectedProject);
        }

        // This method will be called whenever SortingPropertyIndex changes
        // Workaround since Combobox has no Command or Click option
        partial void OnSortingPropertyIndexChanged(int value)
        {
            RefreshXamlBindings();
        }
        partial void OnGroupingPropertyIndexChanged(int value)
        {
            RefreshXamlBindings();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        private static int _sortingPropertyIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        private static int _groupingPropertyIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        [NotifyPropertyChangedFor(nameof(ElementInfoVisibility))]
        private List<Element> _elements = Session.SelectedProject?.Elements ?? new List<Element>(0);

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public Element? SelectedElement => Session.SelectedElement; // Cannot be directly mutated via binding like ListViewItems, since ints wrapped as button in a WrapPanel
        public bool ElementToolsAvailable => Session.SelectedElementId != -1;
        public bool ExportPdfCatalogueAvailable => Elements.Count > 0;

        // Returns False if Index is 0. Index 0 means without Grouping, since "Ohne" is first entry in Combobox
        public bool IsGroupingEnabled => GroupingPropertyIndex > 0;

        // For Grouping and Sorting of WrapPanel: Expose as Static for 'GroupingTypeToPropertyName' Converter
        public ElementSortingType SelectedSorting => (ElementSortingType)_sortingPropertyIndex;
        public ElementGroupingType SelectedGrouping => (ElementGroupingType)_groupingPropertyIndex;
        public Visibility ElementInfoVisibility => ElementToolsAvailable ? Visibility.Visible : Visibility.Collapsed;
        public List<string> SortingProperties => ElementComparer.SortingTypes; // Has to match ElementSortingType enum values (+Order)
        public List<string> GroupingProperties => ElementComparer.GroupingTypes; // Has to match ElementSortingType enum values (+Order)
        public ICollectionView? GroupedElements => IsGroupingEnabled && Session.SelectedProject?.Elements.Count > 0 ? GetGroupedItemsSource() : null;
        public Visibility NoElementsVisibility => Elements.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        private void UpdateOnNewElementAdded()
        {
            if (Session.SelectedProject is null) return;
            // Update InternalIds
            Session.SelectedProject.AssignInternalIdsToElements();
            // Set selected to newest element
            Session.SelectedElementId = Session.SelectedProject.Elements.Last().InternalId;
            // update UI
            RefreshXamlBindings();
        }

        private void UpdateNewProjectAdded()
        {
            if (Session.SelectedProject is null) return;

            // reset SelectedElement
            Session.SelectedElementId = -1;

            // Update InternalIds and render new images
            Session.SelectedProject.AssignInternalIdsToElements();
            Session.SelectedProject.RenderAllElementImages();
            // update UI
            RefreshXamlBindings();
        }
        private void UpdateOnElementRemoved()
        {
            if (Session.SelectedProject is null) return;

            // reset SelectedElement
            Session.SelectedElementId = -1;
            // set selected element to last
            if (Session.SelectedProject.Elements.Count > 0)
                Session.SelectedElementId = Session.SelectedProject.Elements.Last().InternalId;
            // update UI
            RefreshXamlBindings();
        }

        private void RefreshXamlBindings()
        {
            if (Session.SelectedProject is null) return;

            // For Updating MVVM Properties
            Elements = null;
            Session.SelectedProject.Elements.Sort(new ElementComparer(SelectedSorting));
            Elements = Session.SelectedProject.Elements;

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(IsGroupingEnabled));
            OnPropertyChanged(nameof(GroupedElements));
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(ExportPdfCatalogueAvailable));
            OnPropertyChanged(nameof(ElementToolsAvailable));
            OnPropertyChanged(nameof(ElementInfoVisibility));
            OnPropertyChanged(nameof(NoElementsVisibility));
        }

        private ICollectionView GetGroupedItemsSource()
        {
            var cvs = new CollectionViewSource { Source = Session.SelectedProject?.Elements };
            var pgd = new PropertyGroupDescription(".", new GroupingTypeToPropertyName(SelectedGrouping));
            cvs.GroupDescriptions.Add(pgd);
            return cvs.View;
        }
    }
}
