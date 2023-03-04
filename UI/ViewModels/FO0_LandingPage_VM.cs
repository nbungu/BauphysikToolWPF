using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO0_LandingPage.xaml: Used in xaml as "DataContext"
    public partial class FO0_LandingPage_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "LandingPage";

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
        private void OpenNewElementWindow(int? selectedElementId) // CommandParameter is the Content Property of the Button which holds the ElementId
        {
            var window = (selectedElementId is null) ? new NewElementWindow() : new NewElementWindow(DatabaseAccess.QueryElementById(Convert.ToInt32(selectedElementId)));

            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId);
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

            // Update XAML Binding Property by fetching from DB
            Elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId);
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

        /*
         * MVVM Properties
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private List<Element> elements = DatabaseAccess.QueryElementsByProjectId(FO0_ProjectPage.ProjectId) ?? new List<Element>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedElementName))] // Notifies 'SelectedElementName' when this property is changed!
        [NotifyPropertyChangedFor(nameof(SelectedElementImage))]
        [NotifyPropertyChangedFor(nameof(SelectedElementType))]
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

        public bool ElementToolsAvailable
        {
            get
            {
                return selectedElementId != -1;
            }
        }
        public string SelectedElementName
        {
            get
            {
                return (selectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(selectedElementId).Name;
            }
        }
        public BitmapImage? SelectedElementImage
        {
            get
            {
                return (selectedElementId == -1) ? new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/placeholder_256px_light.png")) : DatabaseAccess.QueryElementById(selectedElementId).ElementImage;
            }
        }
        public string SelectedElementType
        {
            get
            {
                return (selectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(selectedElementId).Construction.Type;
            }
        }
        public string SelectedElementRValue
        {
            get
            {
                return (selectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(selectedElementId).RValue.ToString();
            }
        }
        public string SelectedElementSdThickness
        {
            get
            {
                return (selectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(selectedElementId).SdThickness.ToString();
            }
        }
        public string SelectedElementAreaMassDens
        {
            get
            {
                return (selectedElementId == -1) ? "-" : DatabaseAccess.QueryElementById(selectedElementId).AreaMassDens.ToString();
            }
        }
    }
}
