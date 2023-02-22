using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters
    public partial class FO0_LandingPage : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Class Variables
        // Initialize + Assign empty List to avoid null value
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

        // Custom Methods

        // Right click only on Panel Button(!) opens Context Menu
        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu contextMenu = element_ItemsControl.FindResource("WrapPanel_ContextMenu") as ContextMenu;
            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }
        /*
        // Context Menu - Delete
        private void delete_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            Button button = contextMenu.PlacementTarget as Button;
            int elementId = Convert.ToInt16(button.Content);
            DatabaseAccess.DeleteElementById(elementId);
        }
        // Context Menu - Edit
        private void edit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            Button button = contextMenu.PlacementTarget as Button;
            int elementId = Convert.ToInt16(button.Content);
            Element editElement = DatabaseAccess.QueryElementById(elementId);
            // Once a window is closed, the same object instance can't be used to reopen the window.
            var window = new NewElementWindow(editElement);

            window.ShowDialog(); // Open as modal (Parent window pauses, waiting for the window to be closed)
        }
        // Context Menu - Lock
        private void lock_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }*/
    }
}
