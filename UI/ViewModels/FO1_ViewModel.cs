using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public partial class FO1_ViewModel : ObservableObject
    {
        // Called by 'InitializeComponent()' from FO1_Setup.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "Setup";
        public string ElementName { get; set; } = FO0_LandingPage.SelectedElement.Name;
        public string ElementType { get; set; } = FO0_LandingPage.SelectedElement.Construction.Type;   
        public List<Layer> Layers { get; set; } = FO0_LandingPage.SelectedElement.Layers; // Initial List used by 'layers_ListView'

        /*
         * Static Class Properties:
         * 
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */
        private static List<string>? ti_Keys;
        public List<string> Ti_Keys
        {
            get
            {
                return ti_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Ti").Select(e => e.Comment).ToList();
            }
        }
        private static List<string>? te_Keys;
        public List<string> Te_Keys
        {
            get
            {
                return te_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Te").Select(e => e.Comment).ToList();
            }
        }
        private static List<string>? rsi_Keys;
        public List<string> Rsi_Keys
        {
            get
            {
                return rsi_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Select(e => e.Comment).ToList();
            }
        }
        private static List<string>? rse_Keys;
        public List<string> Rse_Keys
        {
            get
            {
                return rse_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rse").Select(e => e.Comment).ToList();
            }
        }
        private static List<string>? rel_Fi_Keys;
        public List<string> Rel_Fi_Keys
        {
            get
            {
                return rel_Fi_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Select(e => e.Comment).ToList();
            }
        }
        private static List<string>? rel_Fe_Keys;
        public List<string> Rel_Fe_Keys
        {
            get
            {
                return rel_Fe_Keys ??= DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Select(e => e.Comment).ToList();
            }
        }

        // Testing MVVM

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TiValue))] // Notify 'TiValue' when this property is changed!
        private static string ti_selection = ""; // As Static Class Variable to Save the Selection after Switching Pages!
        public string TiValue
        {
            get
            {
                return (ti_selection == "") ? "" : DatabaseAccess.QueryEnvVarsBySymbol("Ti").Find(e => e.Comment == ti_selection).Value.ToString();
            }
            set { }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TeValue))] // Notify 'TeValue' when this property is changed!
        private static string te_selection = ""; // As Static Class Variable to Save the Selection after Switching Pages!
        public string TeValue
        {
            get
            {
                return (te_selection == "") ? "" : DatabaseAccess.QueryEnvVarsBySymbol("Te").Find(e => e.Comment == te_selection).Value.ToString();
            }
            set { }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RsiValue))] // Notify 'TeValue' when this property is changed!
        private static string rsi_selection = ""; // As Static Class Variable to Save the Selection after Switching Pages!
        public string RsiValue
        {
            get
            {
                return (rsi_selection == "") ? "" : DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Find(e => e.Comment == rsi_selection).Value.ToString();
            }
            set { }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RseValue))] // Notify 'TeValue' when this property is changed!
        private static string rse_selection = ""; // As Static Class Variable to Save the Selection after Switching Pages!
        public string RseValue
        {
            get
            {
                return (rse_selection == "") ? "" : DatabaseAccess.QueryEnvVarsBySymbol("Rse").Find(e => e.Comment == rse_selection).Value.ToString();
            }
            set { }
        }
    }

}
