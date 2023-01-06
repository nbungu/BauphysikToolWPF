using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using BauphysikToolWPF.SQLiteRepo;
using SkiaSharp;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class FO1_ViewModel
    {
        public string Title { get; } = "Setup";

        private List<Layer> layers;
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

        public FO1_ViewModel() // Called by 'InitializeComponent()' from FO1_Setup.cs due to Class-Binding in xaml via DataContext
        {
            //For the ListView
            Layers = DatabaseAccess.GetLayers();

            //For the ComboBoxes
            Ti_Keys = DatabaseAccess.QueryEnvVarsByCategory("Ti").Select(e => e.Key).ToList();
            Te_Keys = DatabaseAccess.QueryEnvVarsByCategory("Te").Select(e => e.Key).ToList();
            Rsi_Keys = DatabaseAccess.QueryEnvVarsByCategory("Rsi").Select(e => e.Key).ToList();
            Rse_Keys = DatabaseAccess.QueryEnvVarsByCategory("Rse").Select(e => e.Key).ToList();

            //TODO: TextBox Viewmodel implementieren
        }
    }
}
