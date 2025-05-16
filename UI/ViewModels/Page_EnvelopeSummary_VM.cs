using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_EnvelopeSummary_VM : ObservableObject
    {


        public Page_EnvelopeSummary_VM()
        {
            //Session.EnvelopeItemsChanged += UpdateXamlBindings;

            //PropertyItem<double>.PropertyChanged += UpdatePresets;
            //PropertyItem<int>.PropertyChanged += UpdatePresets;
            //PropertyItem<string>.PropertyChanged += UpdatePresets;
        }

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */



        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public string Title => "Ergebnisse und Randbedingungen";

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>

        //private void UpdateXamlBindings()
        //{
        //    // For updating MVVM Capsulated Properties
        //    OnPropertyChanged(nameof(EnvelopeItems));
        //    OnPropertyChanged(nameof(IsRowSelected));
        //    OnPropertyChanged(nameof(AnyPresetActive));
        //    OnPropertyChanged(nameof(ItemsCountString));
        //    OnPropertyChanged(nameof(NoEntriesVisibility));
        //}
    }
}
