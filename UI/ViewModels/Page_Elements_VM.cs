using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI.Converter;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BauphysikToolWPF.Services.Application.DocumentOutput;
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
            _dialogService = new DialogService();
            _elements = new ObservableCollection<Element>(Session.SelectedProject?.Elements ?? new List<Element>());
            _groupedElements = IsGroupingEnabled && HasItems ? GetGroupedItemsSource() : null;

            // Subscribe to Event and Handle
            // Allow child Windows to trigger RefreshXamlBindings of this Window
            Session.NewProjectAdded += UpdateOnNewProjectAdded;
            Session.NewElementAdded += UpdateOnNewElementAdded;
            Session.ElementRemoved += UpdateOnElementRemoved;
            Session.SelectedElementChanged += UpdateXamlBindings;

            Logger.LogInfo("[VM] Success");
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
            if (Session.SelectedProject is null || Session.SelectedProject.Elements.Count == 0) return;

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
        private void ShowExtendedProperties()
        {
            if (Session.SelectedElement is null) return;

            var props = new List<IPropertyItem>()
            {
                new PropertyItem<double>(Symbol.UValue, () => Session.SelectedElement.UValueUserDef, value => Session.SelectedElement.UValueUserDef = value) { DecimalPlaces = 3 },
                new PropertyItem<double>(Symbol.RValueElement, () => Session.SelectedElement.RGesValueUserDef, value => Session.SelectedElement.RGesValueUserDef = value),
                new PropertyItem<double>(Symbol.RValueTotal, () => Session.SelectedElement.RTotValueUserDef, value => Session.SelectedElement.RTotValueUserDef = value),
                new PropertyItem<double>(Symbol.AreaMassDensity, () => Session.SelectedElement.AreaMassDensUserDef, value => Session.SelectedElement.AreaMassDensUserDef = value),
                new PropertyItem<double>(Symbol.SdThickness, () => Session.SelectedElement.SdThicknessUserDef, value => Session.SelectedElement.SdThicknessUserDef = value) { DecimalPlaces = 1 },
            };
            var propertyTitle = "Bauphysikalische Daten";
            var windowTitle = Session.SelectedElement.Name + " - bauphysikalische Daten überschreiben";
            _dialogService.ShowPropertyBagDialog(props, propertyTitle, windowTitle);
        }
        
        [RelayCommand]
        private void CopyElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            Session.SelectedElementId = selectedInternalId;
            Session.SelectedProject.DuplicateElement(Session.SelectedElement);
            Session.OnNewElementAdded();
        }

        [RelayCommand]
        private void SelectElement(int selectedInternalId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            Session.SelectedElementId = selectedInternalId;
            Session.SelectedElement.RefreshResults();
            UpdateXamlBindingsOnElementSelected();
        }

        [RelayCommand]
        private void NextElement()
        {
            if (SelectedElement == null || Elements.Count == 0)
                return;

            int index = Elements.IndexOf(SelectedElement);
            if (index == -1)
                return; // safety: element not found

            int nextIndex = index + 1;
            if (nextIndex < Elements.Count)
            {
                SelectElement(Elements[nextIndex].InternalId);
            }
        }

        [RelayCommand]
        private void PreviousElement()
        {
            if (SelectedElement == null || Elements.Count == 0)
                return;

            int index = Elements.IndexOf(SelectedElement);
            if (index <= 0)
                return; // either not found (-1) or already at first

            int prevIndex = index - 1;
            SelectElement(Elements[prevIndex].InternalId);
        }

        [RelayCommand]
        private void OpenElement()
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
            _dialogService.ShowLoadingDialog("Lädt, bitte warten...", minDurationMs: 400);

            DocumentDesigner.FullCatalogueExport(Session.SelectedProject);

            _dialogService.CloseLoadingDialog();
        }

        // This method will be called whenever SortingPropertyIndex changes
        // Workaround since Combobox has no Command or Click option
        partial void OnSortingPropertyIndexChanged(int value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.LastUsedSortingType = value;
            UpdateXamlBindings();
        }

        partial void OnGroupingPropertyIndexChanged(int value)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.LastUsedGroupingType = value;
            UpdateXamlBindings();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        private int _sortingPropertyIndex = Session.SelectedProject?.LastUsedSortingType ?? 0; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        private int _groupingPropertyIndex = Session.SelectedProject?.LastUsedGroupingType ?? 0; // As Static Class Variable to Save the Selection after Switching Pages!

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public Element? SelectedElement => Session.SelectedElement; // Cannot be directly mutated via binding like ListViewItems, since ints wrapped as button in a WrapPanel
        public bool ElementToolsAvailable => Session.SelectedElementId != -1;
        public bool HasItems => Elements.Count > 0;

        // Returns False if Index is 0. Index 0 means without Grouping, since "Ohne" is first entry in Combobox
        public bool IsGroupingEnabled => GroupingPropertyIndex > 0;

        // For Grouping and Sorting of WrapPanel: Expose as Static for 'GroupingTypeToPropertyName' Converter
        public ElementSortingType SelectedSorting => (ElementSortingType)SortingPropertyIndex;
        public ElementGroupingType SelectedGrouping => (ElementGroupingType)GroupingPropertyIndex;
        public Visibility ElementInfoVisibility => ElementToolsAvailable ? Visibility.Visible : Visibility.Collapsed;
        public IEnumerable<string> SortingProperties => ElementSortingTypeMapping.Values; // Has to match ElementSortingType enum values (+Order)
        public IEnumerable<string> GroupingProperties => ElementGroupingTypeMapping.Values; // Has to match ElementSortingType enum values (+Order)

        // Cached 'Elements' collection
        private ObservableCollection<Element> _elements;
        public ObservableCollection<Element> Elements
        {
            get => _elements;
            private set => SetProperty(ref _elements, value);
        }

        // Cached 'GroupedElements' collection
        private ICollectionView? _groupedElements;
        public ICollectionView? GroupedElements
        {
            get => _groupedElements;
            private set => SetProperty(ref _groupedElements, value);
        }

        public Visibility NoElementsVisibility => HasItems ? Visibility.Collapsed : Visibility.Visible;
        public List<IPropertyItem> ElementProperties => Session.SelectedElement != null ? new List<IPropertyItem>()
        {
            new PropertyItem<double>(Symbol.UValue, () => Session.SelectedElement.UValueUserDef, value => Session.SelectedElement.UValueUserDef = value) { DecimalPlaces = 3 },
            new PropertyItem<double>(Symbol.RValueElement, () => Session.SelectedElement.RGesValueUserDef, value => Session.SelectedElement.RGesValueUserDef = value),
            new PropertyItem<double>(Symbol.RValueTotal, () => Session.SelectedElement.RTotValueUserDef, value => Session.SelectedElement.RTotValueUserDef = value),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => Session.SelectedElement.AreaMassDensUserDef, value => Session.SelectedElement.AreaMassDensUserDef = value),
            new PropertyItem<double>(Symbol.SdThickness, () => Session.SelectedElement.SdThicknessUserDef, value => Session.SelectedElement.SdThicknessUserDef = value) { DecimalPlaces = 1 },
            new PropertyItem<string>("Erstellt:", () => Session.SelectedElement.CreatedAtString),
            new PropertyItem<string>("Geändert:", () => Session.SelectedElement.UpdatedAtString),
        } : new List<IPropertyItem>();

        private void UpdateOnNewElementAdded()
        {
            Session.SelectedElementId = Session.SelectedProject?.Elements.Last().InternalId ?? -1;
            UpdateXamlBindings();
        }
        private void UpdateOnElementRemoved()
        {
            // set selected element to last
            if (Session.SelectedProject?.Elements.Count > 0)
                Session.SelectedElementId = Session.SelectedProject.Elements.Last().InternalId;
            else
                Session.SelectedElementId = -1;

            // update UI
            UpdateXamlBindings();
        }

        private void UpdateXamlBindingsOnElementSelected()
        {
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(ElementToolsAvailable));
            OnPropertyChanged(nameof(ElementInfoVisibility));
            OnPropertyChanged(nameof(NoElementsVisibility));
            OnPropertyChanged(nameof(ElementProperties));
        }

        private void UpdateXamlBindings()
        {
            if (Session.SelectedProject is null) return;

            // For Updating MVVM Properties
            Session.SelectedProject.SortElements(SelectedSorting);

            Elements = new ObservableCollection<Element>(Session.SelectedProject.Elements);
            GroupedElements = IsGroupingEnabled && HasItems ? GetGroupedItemsSource() : null;

            // TODO: NotifyPropertyChangedFor einarbeiten,statt OnPropertyChanged(nameof(Elements));
            OnPropertyChanged(nameof(Elements));

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(IsGroupingEnabled));
            OnPropertyChanged(nameof(GroupedElements));
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(ElementToolsAvailable));
            OnPropertyChanged(nameof(ElementInfoVisibility));
            OnPropertyChanged(nameof(NoElementsVisibility));
            OnPropertyChanged(nameof(ElementProperties));
        }

        private void UpdateOnNewProjectAdded()
        {
            SortingPropertyIndex = Session.SelectedProject?.LastUsedSortingType ?? 0;
            GroupingPropertyIndex = Session.SelectedProject?.LastUsedGroupingType ?? 0;

            UpdateXamlBindings();
        }

        private ICollectionView GetGroupedItemsSource()
        {
            var cvs = new CollectionViewSource { Source = _elements };
            var pgd = new PropertyGroupDescription(".", new GroupingTypeToPropertyName(SelectedGrouping));
            cvs.GroupDescriptions.Add(pgd);
            return cvs.View;
        }
    }
}
