using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using SkiaSharp;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class FO1_ViewModel
    {
        public string Title { get; } = "Setup";
        public string ElementName { get; set; }

        private List<Layer> layers; //TODO remove new
        public List<Layer> Layers //for Validation
        {
            get { return layers; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("null layer list specified");
                layers = value;
            }
        }
        public List<string> Ti_Keys { get; set; }
        public List<string> Te_Keys { get; set; }
        public List<string> Rsi_Keys { get; set; }
        public List<string> Rse_Keys { get; set; }
        public List<string> Rel_Fi_Keys { get; set; }
        public List<string> Rel_Fe_Keys { get; set; }
        public FO1_ViewModel() // Called by 'InitializeComponent()' from FO1_Setup.cs due to Class-Binding in xaml via DataContext
        {
            //For the ListView
            Layers = FO1_Setup.Layers;

            //For the ComboBoxes - Get List entries and set selection 
            Ti_Keys = DatabaseAccess.QueryEnvVarsBySymbol("Ti").Select(e => e.Comment).ToList();
            Te_Keys = DatabaseAccess.QueryEnvVarsBySymbol("Te").Select(e => e.Comment).ToList();
            Rsi_Keys = DatabaseAccess.QueryEnvVarsBySymbol("Rsi").Select(e => e.Comment).ToList();
            Rse_Keys = DatabaseAccess.QueryEnvVarsBySymbol("Rse").Select(e => e.Comment).ToList();
            Rel_Fi_Keys = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fi").Select(e => e.Comment).ToList();
            Rel_Fe_Keys = DatabaseAccess.QueryEnvVarsBySymbol("Rel_Fe").Select(e => e.Comment).ToList();

            //For the Page title
            ElementName = FO0_LandingPage.SelectedElement.Name;

            //TODO: TextBox Viewmodel implementieren

            //TODO: Canvas Viewmodel implementieren
        }
    }
}
