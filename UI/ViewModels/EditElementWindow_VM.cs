using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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

        private static List<string>? _constructionTypeList;
        public List<string> ConstructionType_List
        {
            get { return _constructionTypeList ??= DatabaseAccess.GetConstructions().Select(e => e.TypeName).ToList(); }
        }

        private static List<string>? _orientationList;
        public List<string> Orientation_List
        {
            get { return _orientationList ??= DatabaseAccess.GetOrientations().Select(e => e.TypeName).ToList(); }
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void ToggleTagInput()
        {
            if (TagBtnVisible == Visibility.Hidden)
            {
                TagBtnVisible = Visibility.Visible;
                TextBoxVisible = Visibility.Hidden;
                EnterBtnVisible = Visibility.Hidden;
            }
            else
            {
                TagBtnVisible = Visibility.Hidden;
                TextBoxVisible = Visibility.Visible;
                EnterBtnVisible = Visibility.Visible;
            }
        }

        [RelayCommand]
        private void EnterTag(string? newTag)
        {
            if (newTag is null || newTag == string.Empty) return;
            // When null, create one first
            TagList ??= new List<string>();
            TagList = TagList.Append(newTag).ToList();
            ToggleTagInput();
        }

        [RelayCommand]
        private void RemoveTag(string? tag)
        {
            if (tag is null || TagList is null) return;
            // create a copy to update Tag_List, since .Remove is void Method
            var list = TagList;
            list.Remove(tag);
            TagList = (list.Count == 0) ? null : list.ToList();
        }

        [RelayCommand]
        private void ChangeColor(SolidColorBrush? color)
        {
            if (color is null) return;
            string colorCode = color.Color.ToString();
            SelectedElementColor = colorCode;
        }


        [RelayCommand]
        private void ApplyChanges(Window? window)
        {
            // To be able to Close EditElementWindow from within this ViewModel
            if (window is null) return;
            // Avoid empty Input fields

            string constrType = SelectedConstruction;
            int constrId = DatabaseAccess.GetConstructions().Find(e => e.TypeName == constrType)?.ConstructionId ?? -1;

            string orientationType = SelectedOrientation;
            int orientationId = DatabaseAccess.GetOrientations().Find(e => e.TypeName == orientationType)?.OrientationId ?? -1;

            // If no Element in Parameter -> Create New
            if (EditElementWindow.SelectedElement is null)
            {
                Element newElem = new Element
                {
                    // ElementId gets set by SQLite DB (AutoIncrement)
                    Name = SelectedElementName,
                    ConstructionId = constrId,
                    OrientationId = orientationId,
                    ProjectId = FO0_ProjectPage.SelectedProjectId,
                    TagList = TagList,
                    Comment = (SelectedElementComment == string.Empty) ? null : SelectedElementComment,
                    ColorCode = SelectedElementColor
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
                EditElementWindow.SelectedElement.ProjectId = FO0_ProjectPage.SelectedProjectId;
                EditElementWindow.SelectedElement.TagList = TagList;
                EditElementWindow.SelectedElement.Comment = (SelectedElementComment == string.Empty) ? null : SelectedElementComment;
                EditElementWindow.SelectedElement.ColorCode = SelectedElementColor;

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
        private string _selectedElementName = EditElementWindow.SelectedElement?.Name ?? "";
        [ObservableProperty]
        private string _selectedConstruction = EditElementWindow.SelectedElement?.Construction.TypeName ?? "";
        [ObservableProperty]
        private string _selectedOrientation = EditElementWindow.SelectedElement?.Orientation.TypeName ?? "";
        [ObservableProperty]
        private Visibility _tagBtnVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _textBoxVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _enterBtnVisible = Visibility.Hidden;
        [ObservableProperty]
        private List<string>? _tagList = EditElementWindow.SelectedElement?.TagList;
        [ObservableProperty]
        private string _selectedElementComment = EditElementWindow.SelectedElement?.Comment ?? "";
        [ObservableProperty]
        private string _selectedElementColor = EditElementWindow.SelectedElement?.ColorCode ?? "#00FFFFFF";

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */
    }
}
