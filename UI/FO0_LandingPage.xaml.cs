using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class FO0_LandingPage : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Constructor
        public FO0_LandingPage()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }

        // Not in ViewModel due to the lack of Command Bindings to ButtonEvents
        private void Button_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.SetPage(NavigationContent.SetupLayer);
        }
    }
}
