using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for FO1_SetupEnv.xaml: Used in xaml as "DataContext"
    public partial class FO1_EnvViewModel : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO1_SetupLayer.cs due to Class-Binding in xaml via DataContext
        public string Title => "SetupEnv";

        /*
         * Static Class Properties:
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */

        public List<string> Ti_Keys => DatabaseAccess.QueryEnvVarsBySymbol("Ti").Select(e => e.Comment).ToList();

        public List<string> Te_Keys => DatabaseAccess.QueryEnvVarsBySymbol("Te").Select(e => e.Comment).ToList();

        public List<string> Rsi_Keys => DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Select(e => e.Comment).ToList();

        public List<string> Rse_Keys => DatabaseAccess.QueryEnvVarsBySymbol("Rse").Select(e => e.Comment).ToList();

        public List<string> Rel_Fi_Keys => DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Select(e => e.Comment).ToList();

        public List<string> Rel_Fe_Keys => DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Select(e => e.Comment).ToList();

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
        private void EditElement() // Binding in XAML via 'EditElementCommand'
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditElementWindow().ShowDialog();

            // Update XAML Binding Property by fetching from DB
            OnPropertyChanged(nameof(CurrentElement));
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values (int default = 0)
         */

        [ObservableProperty]
        private Element _currentElement = UserSaved.SelectedElement;

        // Add m:n realtion to Database when new selection is set
        //TODO implement again
        //UpdateElementEnvVars(ElementId, currentEnvVar);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TiValue))]
        private static int ti_Index; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TeValue))]
        private static int te_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RsiValue))]
        private static int rsi_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RseValue))]
        private static int rse_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFiValue))]
        private static int rel_fi_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFeValue))]
        private static int rel_fe_Index;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public string TiValue
        {
            get
            {
                // Index is 0:
                // On Initial Startup (default value for not assigned int)
                // Index is -1:
                // On custom user input

                //Get corresp Value
                double? value = (ti_Index == -1) ? UserSaved.Ti : DatabaseAccess.QueryEnvVarsBySymbol("Ti").Find(e => e.Comment == Ti_Keys[ti_Index])?.Value;
                // Save SessionData
                UserSaved.Ti = value ?? 0;
                // Return value to UIElement
                return value.ToString() ?? String.Empty;
            }
            set
            {
                // Save custom user input
                UserSaved.Ti = Convert.ToDouble(value);
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                Ti_Index = -1;
            }
        }
        public string TeValue
        {
            get
            {
                double? value = (te_Index == -1) ? UserSaved.Te : DatabaseAccess.QueryEnvVarsBySymbol("Te").Find(e => e.Comment == Te_Keys[te_Index])?.Value;
                UserSaved.Te = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Te = Convert.ToDouble(value);
                Te_Index = -1;
            }
        }
        public string RsiValue
        {
            get
            {
                double? value = (rsi_Index == -1) ? UserSaved.Rsi : DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Find(e => e.Comment == Rsi_Keys[rsi_Index])?.Value;
                UserSaved.Rsi = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rsi = Convert.ToDouble(value);
                Rsi_Index = -1;
            }
        }
        public string RseValue
        {
            get
            {
                double? value = (rse_Index == -1) ? UserSaved.Rse : DatabaseAccess.QueryEnvVarsBySymbol("Rse").Find(e => e.Comment == Rse_Keys[rse_Index])?.Value;
                UserSaved.Rse = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rse = Convert.ToDouble(value);
                Rse_Index = -1;
            }
        }
        public string RelFiValue
        {
            get
            {
                double? value = (rel_fi_Index == -1) ? UserSaved.Rel_Fi : DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Find(e => e.Comment == Rel_Fi_Keys[rel_fi_Index])?.Value;
                UserSaved.Rel_Fi = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rel_Fi = Convert.ToDouble(value);
                Rel_fi_Index = -1;
            }
        }
        public string RelFeValue
        {
            get
            {
                double? value = (rel_fe_Index == -1) ? UserSaved.Rel_Fe : DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Find(e => e.Comment == Rel_Fe_Keys[rel_fe_Index])?.Value;
                UserSaved.Rel_Fe = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rel_Fe = Convert.ToDouble(value);
                Rel_fe_Index = -1;
            }
        }
    }
}
