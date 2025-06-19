using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_Elements.xaml: Used in xaml as "DataContext"
    public partial class Page_Elements_VM : ObservableObject
    {
        private readonly IDialogService _dialogService;

        // Called by 'InitializeComponent()' from Page_Elements.cs due to Class-Binding in xaml via DataContext
        public Page_Elements_VM()
        {
            if (Session.SelectedProject is null) return;

            _dialogService = new DialogService();
            
            Session.SelectedProject.AssignInternalIdsToElements();
            Session.SelectedProject.AssignAsParentToElements();
            Session.SelectedProject.Elements.Sort(new ElementComparer(SelectedSorting));
            Session.SelectedProject.RenderMissingElementImages(); // If Images are not rendered yet

            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            Session.NewProjectAdded += UpdateNewProjectAdded;
            Session.NewElementAdded += UpdateOnNewElementAdded;
            Session.ElementRemoved += UpdateOnElementRemoved;
            Session.SelectedElementChanged += RefreshXamlBindings;
        }
        
        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void AddNewElement()
        {
            Session.SelectedElementId = -1;
            _dialogService.ShowAddNewElementDialog();
        }

        [RelayCommand]
        private void EditElement(int selectedInternalId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            Session.SelectedElementId = selectedInternalId;
            _dialogService.ShowEditElementDialog(Session.SelectedElementId);
        }

        [RelayCommand]
        private void DeleteElement(int selectedInternalId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            // Delete selected Element
            Session.SelectedProject.RemoveElementById(selectedInternalId);
            Session.OnElementRemoved();
        }

        [RelayCommand]
        private void DeleteAllElements()
        {
            MessageBoxResult result = _dialogService.ShowDeleteConfirmationDialog();

            switch (result)
            {
                case MessageBoxResult.Yes:
                    Session.SelectedProject.Elements.Clear();
                    Session.OnElementRemoved();
                    break;
                case MessageBoxResult.Cancel:
                    // Do nothing, user cancelled the action
                    break;
            }
        }

        [RelayCommand]
        private void CopyElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            Session.SelectedElementId = selectedInternalId;

            if (Session.SelectedElement is null) return;
            Session.SelectedProject.DuplicateElement(Session.SelectedElement);
            Session.OnNewElementAdded();
        }

        [RelayCommand]
        private void SelectElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            Session.SelectedElementId = selectedInternalId;
            if (Session.SelectedElement != null) Session.SelectedElement.RefreshResults();
            RefreshXamlBindings();
        }

        [RelayCommand]
        private void ElementDoubleClick()
        {
            if (SelectedElement is null) return;
            MainWindow.SetPage(NavigationPage.LayerSetup, NavigationPage.ElementCatalogue);
        }

        [RelayCommand]
        private void SchichtaufbauClick()
        {
            if (SelectedElement is null) return;
            MainWindow.SetPage(NavigationPage.LayerSetup, NavigationPage.ElementCatalogue);
        }

        [RelayCommand]
        private void CreateSingleElementPdf(int selectedInternalId)
        {
            Session.SelectedElementId = selectedInternalId;
            DocumentDesigner.CreateSingleElementDocument(Session.SelectedElement);
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

        // TODO NotifyPropertyChangedFor einarbeiten
        //[ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        //[NotifyPropertyChangedFor(nameof(ElementInfoVisibility))]
        //[NotifyPropertyChangedFor(nameof(HasItems))]
        //private List<Element>? _elements;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public Element? SelectedElement => Session.SelectedElement; // Cannot be directly mutated via binding like ListViewItems, since ints wrapped as button in a WrapPanel
        public bool ElementToolsAvailable => Session.SelectedElementId != -1;
        public bool HasItems => Elements.Count > 0;

        public ObservableCollection<Element> Elements => new ObservableCollection<Element>(Session.SelectedProject.Elements);

        // Returns False if Index is 0. Index 0 means without Grouping, since "Ohne" is first entry in Combobox
        public bool IsGroupingEnabled => GroupingPropertyIndex > 0;

        // For Grouping and Sorting of WrapPanel: Expose as Static for 'GroupingTypeToPropertyName' Converter
        public ElementSortingType SelectedSorting => (ElementSortingType)_sortingPropertyIndex;
        public ElementGroupingType SelectedGrouping => (ElementGroupingType)_groupingPropertyIndex;
        public Visibility ElementInfoVisibility => ElementToolsAvailable ? Visibility.Visible : Visibility.Collapsed;
        public IEnumerable<string> SortingProperties => ElementSortingTypeMapping.Values; // Has to match ElementSortingType enum values (+Order)
        public IEnumerable<string> GroupingProperties => ElementGroupingTypeMapping.Values; // Has to match ElementSortingType enum values (+Order)
        public ICollectionView? GroupedElements => IsGroupingEnabled && HasItems ? GetGroupedItemsSource() : null;
        public Visibility NoElementsVisibility => HasItems ? Visibility.Collapsed : Visibility.Visible;

        private void UpdateOnNewElementAdded()
        {
            Session.SelectedElementId = Session.SelectedProject.Elements.Last().InternalId;
            RefreshXamlBindings();
        }

        private void UpdateNewProjectAdded()
        {
            // reset SelectedElement
            Session.SelectedElementId = -1;

            // Update InternalIds and render new images
            Session.SelectedProject.AssignInternalIdsToElements();
            Session.SelectedProject.AssignAsParentToElements();
            Session.SelectedProject.RenderAllElementImages();
            // update UI
            RefreshXamlBindings();
        }
        private void UpdateOnElementRemoved()
        {
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
            // For Updating MVVM Properties
            Session.SelectedProject.Elements.Sort(new ElementComparer(SelectedSorting));
            OnPropertyChanged(nameof(Elements));

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(IsGroupingEnabled));
            OnPropertyChanged(nameof(GroupedElements));
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(ElementToolsAvailable));
            OnPropertyChanged(nameof(ElementInfoVisibility));
            OnPropertyChanged(nameof(NoElementsVisibility));
        }

        private ICollectionView GetGroupedItemsSource()
        {
            var cvs = new CollectionViewSource { Source = Session.SelectedProject.Elements };
            var pgd = new PropertyGroupDescription(".", new GroupingTypeToPropertyName(SelectedGrouping));
            cvs.GroupDescriptions.Add(pgd);
            return cvs.View;
        }
    }
}
