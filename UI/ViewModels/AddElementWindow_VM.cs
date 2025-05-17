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
        private readonly Element? _targetElement = Session.SelectedProject?.Elements.FirstOrDefault(e => e?.InternalId == AddElementWindow.TargetElementInternalId, null);

        // Called by 'InitializeComponent()' from AddElementWindow.cs due to Class-Binding in xaml via DataContext
        public AddElementWindow_VM()
        {
            SelectedElementName = _targetElement != null ? _targetElement.Name : "Neues Element";
            SelectedConstruction = _targetElement != null ? (int)_targetElement.Construction.ConstructionType : (int)ConstructionType.Aussenwand;
            SelectedOrientation = _targetElement != null ? (int)_targetElement.OrientationType : (int)OrientationType.North;
            TagList = _targetElement != null ? _targetElement.TagList : new List<string>(0);
            SelectedElementComment = _targetElement != null ? _targetElement.Comment : "";
            SelectedElementColor = _targetElement != null ? _targetElement.ColorCode : "#00FFFFFF";
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

            if (_targetElement != null)
            {
                _targetElement.Name = SelectedElementName;
                _targetElement.ConstructionId = SelectedConstruction;
                _targetElement.OrientationType = (OrientationType)SelectedOrientation;
                _targetElement.TagList = TagList;
                _targetElement.Comment = SelectedElementComment;
                _targetElement.ColorCode = SelectedElementColor;
                _targetElement.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
                Session.OnSelectedElementChanged();
            }
            else
            {
                Element newElem = new Element
                {
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
        private string _selectedElementName;
        [ObservableProperty]
        private int _selectedConstruction;
        [ObservableProperty]
        private int _selectedOrientation;
        [ObservableProperty]
        private Visibility _tagBtnVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _textBoxVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _enterBtnVisible = Visibility.Hidden;
        [ObservableProperty]
        private List<string> _tagList;
        [ObservableProperty]
        private string _selectedElementComment;
        [ObservableProperty]
        private string _selectedElementColor;

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */

        public string Title => _targetElement != null ? $"Ausgewähltes Element bearbeiten: {_targetElement.Name}" : "Neues Element erstellen";

        public List<string> OrientationList => OrientationTypeMapping.Values.ToList();

        // Database is Source
        public List<string> ConstructionTypeList => DatabaseAccess.GetConstructionsQuery().Select(e => e.TypeName).ToList();
    }
}
