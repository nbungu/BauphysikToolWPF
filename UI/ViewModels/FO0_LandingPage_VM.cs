using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

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

        private static List<string?>? sortingListItems;
        public List<string?> SortingListItems
        {
            // Has to match ElementSortingType enum values (+Order)
            get { return sortingListItems ??= new List<string?>() { "Änderungsdatum", "Name", "Typ", "Ausrichtung", "R-Wert", "sd-Wert" }; }
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            if (FO0_LandingPage.SelectedElementId != -1)
                MainWindow.SetPage(desiredPage);
        }

        // Create New Element / Edit Existing Element
        [RelayCommand]
        private void EditElement(int? selectedElementId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            var window = (selectedElementId is null) ? new EditElementWindow() : new EditElementWindow(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            if (selectedElementId is null)
                return;

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId, (ElementSortingType)selectedSortingIndex, isAscending);
        }

        [RelayCommand]
        private void DeleteElement(int? selectedElementId) // CommandParameter is the 'Content' Property of the Button which holds the ElementId as string
        {
            if (selectedElementId is null)
                return;

            // Delete selected Layer
            DatabaseAccess.DeleteElement(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // When deleting the Element which was currently SelectedElement
            if (selectedElementId == FO0_LandingPage.SelectedElementId)
            {
                // Reset Selected Layer
                FO0_LandingPage.SelectedElementId = -1;
                // Update XAML Binding Property
                SelectedElementId = FO0_LandingPage.SelectedElementId;
            }

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId, (ElementSortingType)selectedSortingIndex, isAscending);
        }

        [RelayCommand]
        private void DeleteAllElements()
        {
            // Delete selected Layer
            DatabaseAccess.DeleteAllElements();

            // Reset Selected Layer
            FO0_LandingPage.SelectedElementId = -1;

            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId);
            SelectedElementId = FO0_LandingPage.SelectedElementId;
        }

        [RelayCommand]
        private void SelectElement(int? selectedElementId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId is null)
                return;

            // Set the currently selected Element
            FO0_LandingPage.SelectedElementId = Convert.ToInt32(selectedElementId);

            // Update XAML Binding Property
            SelectedElementId = FO0_LandingPage.SelectedElementId;
        }

        [RelayCommand]
        private void CopyElement(int? selectedElementId) // CommandParameter is the Binding 'ElementId' of the Button inside the ItemsControl
        {
            if (selectedElementId is null)
                return;

            // Create copy of selected Element and add to DB
            Element elementCopy = DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId));
            elementCopy.Name += "-Copy"; 
            DatabaseAccess.CreateElement(elementCopy, withChildren: true);

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId, (ElementSortingType)selectedSortingIndex, isAscending);
        }

        [RelayCommand]
        private void ChangeSortingOrder()
        {
            // Change sorting order
            IsAscending = !IsAscending;

            // Update XAML Binding Property
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId, (ElementSortingType)selectedSortingIndex, isAscending);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private static int selectedSortingIndex = 0; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        private static bool isAscending = true; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedElementName))] // Notifies 'SelectedElementName' when this property is changed!
        [NotifyPropertyChangedFor(nameof(SelectedElementType))]
        [NotifyPropertyChangedFor(nameof(SelectedElementOrientation))]
        private List<Element> elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId, (ElementSortingType)selectedSortingIndex, isAscending) ?? new List<Element>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedElementName))]
        [NotifyPropertyChangedFor(nameof(SelectedElementImage))]
        [NotifyPropertyChangedFor(nameof(SelectedElementType))]
        [NotifyPropertyChangedFor(nameof(SelectedElementOrientation))]
        [NotifyPropertyChangedFor(nameof(SelectedElementRValue))]
        [NotifyPropertyChangedFor(nameof(SelectedElementSdThickness))]
        [NotifyPropertyChangedFor(nameof(SelectedElementAreaMassDens))]
        [NotifyPropertyChangedFor(nameof(ElementToolsAvailable))]
        private int selectedElementId = FO0_LandingPage.SelectedElementId;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        //TODO call Element from DB only once!

        public bool ElementToolsAvailable
        {
            get
            {
                return SelectedElementId != -1;
            }
        }
        public string SelectedElementName
        {
            get
            {
                return (SelectedElementId == -1) ? String.Empty : DatabaseAccess.QueryElementById(SelectedElementId).Name;
            }
        }
        public BitmapImage? SelectedElementImage
        {
            get
            {
                return (SelectedElementId == -1) ? new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/placeholder_256px_light.png")) : DatabaseAccess.QueryElementById(SelectedElementId).ElementImage;
            }
        }
        public string SelectedElementType
        {
            get
            {
                return (SelectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(SelectedElementId).Construction.Type;
            }
        }
        public string SelectedElementOrientation
        {
            get
            {
                return (SelectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(SelectedElementId).Orientation.TypeName;
            }
        }
        public string SelectedElementRValue
        {
            get
            {
                return (SelectedElementId == -1) ? "0" : DatabaseAccess.QueryElementById(SelectedElementId).RValue.ToString();
            }
        }
        public string SelectedElementSdThickness
        {
            get
            {
                return (SelectedElementId == -1) ? "0" : DatabaseAccess.QueryElementById(SelectedElementId).SdThickness.ToString();
            }
        }
        public string SelectedElementAreaMassDens
        {
            get
            {
                return (SelectedElementId == -1) ? "0" : DatabaseAccess.QueryElementById(SelectedElementId).AreaMassDens.ToString();
            }
        }
    }
}
