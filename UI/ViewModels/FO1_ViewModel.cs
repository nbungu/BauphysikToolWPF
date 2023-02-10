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
        public string ElementName { get; } = FO0_LandingPage.SelectedElement.Name;
        public string ElementType { get; } = FO0_LandingPage.SelectedElement.Construction.Type;
        public string NeighbouringEnv { get; } = FO0_LandingPage.SelectedElement.Construction.Type;
        public List<Layer> Layers { get; } = FO1_Setup.Layers;
        public List<string> Ti_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol("Ti").Select(e => e.Comment).ToList();
        public List<string> Te_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol("Te").Select(e => e.Comment).ToList();
        public List<string> Rsi_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Select(e => e.Comment).ToList();
        public List<string> Rse_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol("Rse").Select(e => e.Comment).ToList();
        public List<string> Rel_Fi_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Select(e => e.Comment).ToList();
        public List<string> Rel_Fe_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Select(e => e.Comment).ToList();
    }
}
