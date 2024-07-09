using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class FO0_ProjectPage : UserControl
    {
        // Constructor
        public FO0_ProjectPage()
        {
            if (UserSaved.SelectedProject != null) UserSaved.SelectedProject.AssignInternalIdsToElements();

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }
    }
}
