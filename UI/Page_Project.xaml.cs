using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Project : UserControl
    {
        // Constructor
        public Page_Project()
        {
            if (UserSaved.SelectedProject != null) UserSaved.SelectedProject.AssignInternalIdsToElements();

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }
    }
}
