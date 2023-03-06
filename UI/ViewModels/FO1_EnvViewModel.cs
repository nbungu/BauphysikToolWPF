using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for FO1_SetupEnv.xaml: Used in xaml as "DataContext"
    public partial class FO1_EnvViewModel : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO1_SetupLayer.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "SetupEnv";

        /*
         * Static Class Properties:
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */

        private static List<string?>? ti_Keys;
        public List<string?> Ti_Keys
        {
            get { return ti_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Ti").Select(e => e.Comment).ToList(); }
        }
        private static List<string?>? te_Keys;
        public List<string?> Te_Keys
        {
            get { return te_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Te").Select(e => e.Comment).ToList(); }
        }
        private static List<string?>? rsi_Keys;
        public List<string?> Rsi_Keys
        {
            get { return rsi_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Select(e => e.Comment).ToList(); }
        }
        private static List<string?>? rse_Keys;
        public List<string?> Rse_Keys
        {
            get { return rse_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rse").Select(e => e.Comment).ToList(); }
        }
        private static List<string?>? rel_Fi_Keys;
        public List<string?> Rel_Fi_Keys
        {
            get { return rel_Fi_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Select(e => e.Comment).ToList(); }
        }
        private static List<string?>? rel_Fe_Keys;
        public List<string?> Rel_Fe_Keys
        {
            get { return rel_Fe_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Select(e => e.Comment).ToList(); }
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void EditElement(Element? selectedElement) // Binding in XAML via 'EditElementCommand'
        {
            if (selectedElement is null)
                selectedElement = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId);

            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new EditElementWindow(selectedElement);
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            window.ShowDialog();

            // After Window closed:
            // Update XAML Binding Property by fetching from DB
            ElementName = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Name;
            ElementType = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Construction.Type;
        }

        /*
         * MVVM Properties
         */

        [ObservableProperty]
        private string elementName = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Name;

        [ObservableProperty]
        private string elementType = DatabaseAccess.QueryElementById(FO0_LandingPage.SelectedElementId).Construction.Type;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TiValue))] // Notifies 'TiValue' when this property is changed!
        private static string ti_selection = ""; // As Static Class Variable to Save the Selection after Switching Pages!

        // Add m:n realtion to Database when new selection is set
        //TODO implement again
        //UpdateElementEnvVars(ElementId, currentEnvVar);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TeValue))]
        private static string te_selection = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RsiValue))]
        private static string rsi_selection = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RseValue))]
        private static string rse_selection = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFiValue))]
        private static string rel_fi_selection = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFeValue))]
        private static string rel_fe_selection = "";

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public string TiValue
        {
            get
            {
                double? value = (ti_selection == "") ? 0 : DatabaseAccess.QueryEnvVarsBySymbol("Ti").Find(e => e.Comment == ti_selection)?.Value;
                UserSaved.Ti = value ?? 0;
                return UserSaved.Ti.ToString();
            }
            set
            {
                //TODO handle input from User
            }
        }
        public string TeValue
        {
            get
            {
                double? value = (te_selection == "") ? 0 : DatabaseAccess.QueryEnvVarsBySymbol("Te").Find(e => e.Comment == te_selection)?.Value;
                UserSaved.Te = value ?? 0;
                return UserSaved.Te.ToString();
            }
            set
            {
                //TODO handle input from User
            }
        }
        public string RsiValue
        {
            get
            {
                double? value = (rsi_selection == "") ? 0 : DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Find(e => e.Comment == rsi_selection)?.Value;
                UserSaved.Rsi = value ?? 0;
                return UserSaved.Rsi.ToString();
            }
            set
            {
                //TODO handle input from User
            }
        }
        public string RseValue
        {
            get
            {
                double? value = (rse_selection == "") ? 0 : DatabaseAccess.QueryEnvVarsBySymbol("Rse").Find(e => e.Comment == rse_selection)?.Value;
                UserSaved.Rse = value ?? 0;
                return UserSaved.Rse.ToString();
            }
            set
            {
                //TODO handle input from User
            }
        }
        public string RelFiValue
        {
            get
            {
                double? value = (rel_fi_selection == "") ? 0 : DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Find(e => e.Comment == rel_fi_selection)?.Value;
                UserSaved.Rel_Fi = value ?? 0;
                return UserSaved.Rel_Fi.ToString();
            }
            set
            {
                //TODO handle input from User
            }
        }
        public string RelFeValue
        {
            get
            {
                double? value = (rel_fe_selection == "") ? 0 : DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Find(e => e.Comment == rel_fe_selection)?.Value;
                UserSaved.Rel_Fe = value ?? 0;
                return UserSaved.Rel_Fe.ToString();
            }
            set
            {
                //TODO handle input from User
            }
        }
    }
}
