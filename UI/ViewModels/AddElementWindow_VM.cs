using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddElementWindow.xaml: Used in xaml as "DataContext"
    public partial class AddElementWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddElementWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => EditSelectedElement && Session.SelectedElement != null ? $"Ausgewähltes Element bearbeiten: {Session.SelectedElement.Name}" : "Neues Element erstellen";

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
            if (Session.SelectedProject == null) return;
            // Avoid empty Input fields

            //Construction construction = DatabaseAccess.GetConstructionsQuery().FirstOrDefault(e => e.Type == (ConstructionType)SelectedConstruction, new Construction());
            
            if (EditSelectedElement && Session.SelectedElement != null)
            {
                Session.SelectedElement.Name = SelectedElementName;
                Session.SelectedElement.ConstructionId = SelectedConstruction;
                Session.SelectedElement.OrientationType = (OrientationType)SelectedOrientation;
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
                    ConstructionId = SelectedConstruction,
                    OrientationType = (OrientationType)SelectedOrientation,
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
        private string _selectedElementName = Session.SelectedElement != null && Session.SelectedElement.Name != "" ? Session.SelectedElement.Name : "Neues Element";
        [ObservableProperty]
        private int _selectedConstruction = Session.SelectedElement != null ? (int)Session.SelectedElement.Construction.Type : (int)ConstructionType.Aussenwand;
        [ObservableProperty]
        private int _selectedOrientation = Session.SelectedElement != null ? (int)Session.SelectedElement.OrientationType : (int)OrientationType.North;
        [ObservableProperty]
        private Visibility _tagBtnVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _textBoxVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _enterBtnVisible = Visibility.Hidden;
        [ObservableProperty]
        private List<string> _tagList = Session.SelectedElement != null ? Session.SelectedElement.TagList : new List<string>(0);
        [ObservableProperty]
        private string _selectedElementComment = Session.SelectedElement != null ? Session.SelectedElement.Comment : "";
        [ObservableProperty]
        private string _selectedElementColor = Session.SelectedElement != null ? Session.SelectedElement.ColorCode : "#00FFFFFF";

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public bool EditSelectedElement => AddElementWindow.EditExistingElement;
        public List<string> OrientationList => OrientationTypeMapping.Values.ToList();

        // Database is Source
        public List<string> ConstructionTypeList => DatabaseAccess.GetConstructionsQuery().Select(e => e.TypeName).ToList();
    }
}
