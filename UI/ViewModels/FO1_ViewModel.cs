using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class FO1_ViewModel
    {
        // Called by 'InitializeComponent()' from FO1_Setup.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "Setup";
        public string ElementName { get; set; } = FO0_LandingPage.SelectedElement.Name;
        public string ElementType { get; set; } = FO0_LandingPage.SelectedElement.Construction.Type;
        
        // Initial List used by 'layers_ListView'
        public List<Layer> Layers { get; set; } = FO0_LandingPage.SelectedElement.Layers;

        /*
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
    }

}
