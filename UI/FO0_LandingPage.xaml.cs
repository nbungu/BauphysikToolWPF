using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters
    public partial class FO0_LandingPage : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Class Variables
        // Initialize + Assign Default Values to Avoid null Values.s
        public static int ProjectId { get; private set; } = 1; // TODO change Hardcoded value

        private static int selectedElementId = -1;
        public static int SelectedElementId
        {
            get { return selectedElementId; }
            set
            {
                if (value != selectedElementId)
                    OnSelectedElementChanged();
                selectedElementId = value;
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
    }
}
