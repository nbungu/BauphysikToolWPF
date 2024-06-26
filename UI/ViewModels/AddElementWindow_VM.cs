using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddElementWindow.xaml: Used in xaml as "DataContext"
    public partial class AddElementWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddElementWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => "AddElementWindow";

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
            TagList = TagList.Append(newTag).ToList();
            ToggleTagInput();
        }

        [RelayCommand]
        private void RemoveTag(string? tag)
        {
            if (tag is null || TagList.Count == 0) return;
            TagList.Remove(tag);
            // Assign a new instance to force Update or trigger OnPropertyChanged
            TagList = new List<string>(TagList);
            //OnPropertyChanged(nameof(TagList));
        }

        [RelayCommand]
        private void ChangeColor(SolidColorBrush? color)
        {
            if (color is null) return;
            SelectedElementColor = color.Color.ToString();
        }


        [RelayCommand]
        private void ApplyChanges(Window? window)
        {
            // To be able to Close EditElementWindow from within this ViewModel
            if (window is null) return;
            // Avoid empty Input fields

            Construction construction = DatabaseAccess.GetConstructions().FirstOrDefault(e => e.TypeName == SelectedConstruction, new Construction());

            Element newElem = new Element
            {
                // ElementId gets set by SQLite DB (AutoIncrement)
                Name = SelectedElementName,
                ConstructionId = construction.ConstructionId,
                Construction = construction,
                OrientationType = SelectedOrientation,
                ProjectId = UserSaved.SelectedProject.Id,
                TagList = TagList,
                Comment = SelectedElementComment,
                ColorCode = SelectedElementColor
            };
            UserSaved.SelectedProject.Elements.Add(newElem);
            UserSaved.OnSelectedElementChanged();
            window.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _selectedElementName = UserSaved.SelectedElement.Name;
        [ObservableProperty]
        private string _selectedConstruction = UserSaved.SelectedElement.Construction.TypeName;
        [ObservableProperty]
        private OrientationType _selectedOrientation = UserSaved.SelectedElement.OrientationType;
        [ObservableProperty]
        private Visibility _tagBtnVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _textBoxVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _enterBtnVisible = Visibility.Hidden;
        [ObservableProperty]
        private List<string> _tagList = UserSaved.SelectedElement.TagList;
        [ObservableProperty]
        private string _selectedElementComment = UserSaved.SelectedElement.Comment;
        [ObservableProperty]
        private string _selectedElementColor = UserSaved.SelectedElement.ColorCode;

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public List<string> ConstructionTypeList => DatabaseAccess.GetConstructions().Select(e => e.TypeName).ToList();
    }
}
