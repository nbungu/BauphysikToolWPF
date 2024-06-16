using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public delegate void Notify(); // delegate with signature: return type void, no input parameters
    public partial class FO0_ProjectPage : UserControl
    {
        // Constructor
        public FO0_ProjectPage()
        {
            InitializeSessionData();

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }

        private void InitializeSessionData()
        {
            if (UserSaved.SelectedProject is null)
            {
                var selectedProject = DatabaseAccess.QueryProjectById(1);
                selectedProject.AssignInternalIdsToElements();
                UserSaved.SelectedProject = selectedProject;
            }
        }
    }
}
