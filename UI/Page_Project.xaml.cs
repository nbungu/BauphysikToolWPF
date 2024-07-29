using System.Linq;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;

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
        
        private void FileDropArea_OnSourceUpdated(object? sender, DataTransferEventArgs e)
        {
            var viewModel = DataContext as Page_Project_VM;
            if (viewModel != null)
            {
                UserSaved.SelectedProject.LinkedFilesList = viewModel.DroppedFilePaths.ToList();
            }
        }
    }
}
