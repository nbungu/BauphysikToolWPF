using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_ThermalBridges_VM : ObservableObject
    {
        public Page_ThermalBridges_VM()
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

        public string Title => "Wärmebrückeneingabe";

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
