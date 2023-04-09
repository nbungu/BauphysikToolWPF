using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for EditElementWindow.xaml: Used in xaml as "DataContext"
    public partial class EditElementWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from EditElementWindow.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "EditElementWindow";

        /*
         * Static Class Properties:
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */

        private static List<string>? constructionType_List;
        public List<string> ConstructionType_List
        {
            get { return constructionType_List ??= DatabaseAccess.GetConstructions().Select(e => e.TypeName).ToList(); }
        }

        private static List<string>? orientation_List;
        public List<string> Orientation_List
        {
            get { return orientation_List ??= DatabaseAccess.GetOrientations().Select(e => e.TypeName).ToList(); }
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void ToggleInput()
        {
            if(TagBtnVisible == Visibility.Hidden)
            {
                TagBtnVisible = Visibility.Visible;
                TextBoxVisible = Visibility.Hidden;
                EnterBtnVisible = Visibility.Hidden;
            } else
            {
                TagBtnVisible = Visibility.Hidden;
                TextBoxVisible = Visibility.Visible;
                EnterBtnVisible = Visibility.Visible;
            }
        }

        [RelayCommand]
        private void EnterTag(string newTag)
        {
            if (newTag == string.Empty || newTag is null)
                return;
            Tag_List = Tag_List.Append(newTag).ToList();
            ToggleInput();
        }

        [RelayCommand]
        private void RemoveTag(string tag)
        {
            if (tag is null)
                return;
            // create a copy to update Tag_List, since .Remove is void Method
            var list = Tag_List;
            list.Remove(tag);
            Tag_List = list.ToList();
        }

        [RelayCommand]
        private void ApplyChanges(Window window)
        {
            // To be able to Close EditElementWindow from within this ViewModel
            if (window is null)
                return;
            // Avoid empty Input fields

            string constrType = SelectedConstruction;
            int constrId = DatabaseAccess.GetConstructions().Find(e => e.TypeName == constrType)?.ConstructionId ?? -1;

            string orientationType = SelectedOrientation;
            int orientationId = DatabaseAccess.GetOrientations().Find(e => e.TypeName == orientationType)?.OrientationId ?? -1;

            // If no Element in Parameter -> Create New
            if (EditElementWindow.SelectedElement is null)
            {
                Element newElem = new Element()
                {
                    // ElementId gets set by SQLite DB (AutoIncrement)
                    Name = SelectedElementName,
                    ConstructionId = constrId,
                    OrientationId = orientationId,
                    ProjectId = FO0_ProjectPage.ProjectId,
                    TagList = Tag_List
                };
                // Update in Database
                DatabaseAccess.CreateElement(newElem);

                //Set as selected Element
                FO0_LandingPage.SelectedElementId = newElem.ElementId;

                // Go to Setup Page (Editor) after creating new Element
                window.Close();
                MainWindow.SetPage(NavigationContent.SetupLayer);
            }
            // If Element in Parameter -> Edit existing Element (SelectedElement from FO0_LandingPage)
            else
            {
                EditElementWindow.SelectedElement.Name = SelectedElementName;
                EditElementWindow.SelectedElement.ConstructionId = constrId;
                EditElementWindow.SelectedElement.OrientationId = orientationId;
                EditElementWindow.SelectedElement.ProjectId = FO0_ProjectPage.ProjectId;
                EditElementWindow.SelectedElement.TagList = Tag_List;

                // Update in Database
                DatabaseAccess.UpdateElement(EditElementWindow.SelectedElement);
                // Just Close this after editing existing Element
                window.Close();
            }
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string selectedElementName = EditElementWindow.SelectedElement?.Name ?? "";

        [ObservableProperty]
        private string selectedConstruction = EditElementWindow.SelectedElement?.Construction.TypeName ?? "";

        [ObservableProperty]
        private string selectedOrientation = EditElementWindow.SelectedElement?.Orientation.TypeName ?? "";

        [ObservableProperty]
        private Visibility tagBtnVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility textBoxVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility enterBtnVisible = Visibility.Hidden;

        [ObservableProperty]
        private List<string>? tag_List = EditElementWindow.SelectedElement?.TagList;

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

    }
}
