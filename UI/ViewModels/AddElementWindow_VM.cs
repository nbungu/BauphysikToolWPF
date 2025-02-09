using BauphysikToolWPF.Repository;
using BauphysikToolWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using BauphysikToolWPF.Repository.Models;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddElementWindow.xaml: Used in xaml as "DataContext"
    public partial class AddElementWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddElementWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => EditSelectedElement ? $"Ausgewähltes Element bearbeiten: {Session.SelectedElement.Name}" : "Neues Element erstellen";

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
            
            if (EditSelectedElement)
            {
                Session.SelectedElement.Name = SelectedElementName;
                Session.SelectedElement.ConstructionId = construction.ConstructionId;
                Session.SelectedElement.Construction = construction;
                Session.SelectedElement.OrientationType = SelectedOrientation;
                Session.SelectedElement.ProjectId = Session.SelectedProject.Id;
                Session.SelectedElement.TagList = TagList;
                Session.SelectedElement.Comment = SelectedElementComment;
                Session.SelectedElement.ColorCode = SelectedElementColor;
                Session.SelectedElement.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
                Session.OnSelectedElementChanged();
            }
            else
            {
                Element newElem = new Element
                {
                    // ElementId gets set by SQLite DB (AutoIncrement)
                    Name = SelectedElementName,
                    ConstructionId = construction.ConstructionId,
                    Construction = construction,
                    OrientationType = SelectedOrientation,
                    ProjectId = Session.SelectedProject.Id,
                    TagList = TagList,
                    Comment = SelectedElementComment,
                    ColorCode = SelectedElementColor
                };
                Session.SelectedProject.Elements.Add(newElem);
                Session.OnNewElementAdded();
            }

            window.Close();
        }

        [RelayCommand]
        private void Cancel(Window? window)
        {
            // To be able to Close EditElementWindow from within this ViewModel
            if (window is null) return;
            window.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _selectedElementName = Session.SelectedElement.Name != "" ? Session.SelectedElement.Name : "Neues Element";
        [ObservableProperty]
        private string _selectedConstruction = Session.SelectedElement.Name != "" ? Session.SelectedElement.Construction.TypeName : "Außenwand";
        [ObservableProperty]
        private OrientationType _selectedOrientation = Session.SelectedElement.Name != "" ? Session.SelectedElement.OrientationType : OrientationType.Norden;
        [ObservableProperty]
        private Visibility _tagBtnVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _textBoxVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _enterBtnVisible = Visibility.Hidden;
        [ObservableProperty]
        private List<string> _tagList = Session.SelectedElement.TagList;
        [ObservableProperty]
        private string _selectedElementComment = Session.SelectedElement.Comment;
        [ObservableProperty]
        private string _selectedElementColor = Session.SelectedElement.ColorCode;

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public bool EditSelectedElement => AddElementWindow.EditExistingElement;

        public List<string> ConstructionTypeList => DatabaseAccess.GetConstructions().Select(e => e.TypeName).ToList();
    }
}
