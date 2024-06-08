using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class FO0_ProjectPage : UserControl
    {
        // Class Variables
        // Initialize + Assign Default Values to Avoid null Values.s

        //public static int ProjectId { get; private set; } = 1; // TODO change Hardcoded value

        private static int _selectedProjectId = 1; // Initialize + Assign Default Values to Avoid null Values
        public static int SelectedProjectId
        {
            get { return _selectedProjectId; }
            set
            {
                if (value != _selectedProjectId)
                    OnSelectedProjectChanged();
                _selectedProjectId = value;
            }
        }

        //The subscriber class must register to SelectedElementChanged event and handle it with the method whose signature matches Notify delegate
        public static event Notify? SelectedProjectChanged; // event

        // event handlers - publisher
        public static void OnSelectedProjectChanged() //protected virtual method
        {
            SelectedProjectChanged?.Invoke(); //if SelectedProjectChanged is not null then call delegate
        }

        // Constructor
        public FO0_ProjectPage()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }
    }
}
