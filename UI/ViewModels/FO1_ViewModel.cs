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
        public string NeighbouringEnv { get; set; } = FO0_LandingPage.SelectedElement.Construction.Type;
        public List<Layer> Layers { get; set; } = FO0_LandingPage.SelectedElement.Layers;
        public List<string> Ti_Keys { get; set; } = DatabaseAccess.QueryEnvVarsBySymbol("Ti").Select(e => e.Comment).ToList();
        public List<string> Te_Keys { get; set; } = DatabaseAccess.QueryEnvVarsBySymbol("Te").Select(e => e.Comment).ToList();
        public List<string> Rsi_Keys { get; set; } = DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Select(e => e.Comment).ToList();
        public List<string> Rse_Keys { get; set; } = DatabaseAccess.QueryEnvVarsBySymbol("Rse").Select(e => e.Comment).ToList();
        public List<string> Rel_Fi_Keys { get; set; } = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Select(e => e.Comment).ToList();
        public List<string> Rel_Fe_Keys { get; set; } = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Select(e => e.Comment).ToList();
    }
}
