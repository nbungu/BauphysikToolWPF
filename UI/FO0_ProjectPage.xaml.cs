using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class FO0_ProjectPage : UserControl
    {
        // Class Variables
        // Initialize + Assign Default Values to Avoid null Values.s
       


        // Constructor
        public FO0_ProjectPage()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }
    }
}
