using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for FO1_Setup.xaml: Used in xaml as "DataContext"
    public class NewElementWindow_ViewModel
    {
        // Called by 'InitializeComponent()' from FO0_LandingPage.cs due to Class-Binding in xaml via DataContext
        public string Title { get; } = "NewElement";

        /*
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */

        private static List<string>? types;
        public List<string> Types
        {
            get
            {
                return types ??= DatabaseAccess.GetConstructions().Select(e => e.Type).ToList();
            }
        }
    }
}
