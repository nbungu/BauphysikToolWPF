using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters
    public partial class FO0_LandingPage : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Class Variables

        private static int _selectedElementId = -1; // Initialize + Assign Default Values to Avoid null Values
        public static int SelectedElementId
        {
            get => _selectedElementId;
            set
            {
                if (value != _selectedElementId) OnSelectedElementChanged();
                _selectedElementId = value;

            }
        }

        //The subscriber class must register to SelectedElementChanged event and handle it with the method whose signature matches Notify delegate
        public static event Notify? SelectedElementChanged; // event

        // event handlers - publisher
        public static void OnSelectedElementChanged() //protected virtual method
        {
            SelectedElementChanged?.Invoke(); //if LayersChanged is not null then call delegate
        }

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
