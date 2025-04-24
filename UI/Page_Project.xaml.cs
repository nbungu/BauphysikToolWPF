using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.ViewModels;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Project : UserControl
    {
        // Constructor
        public Page_Project()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }
        
        private void FileDropArea_OnSourceUpdated(object? sender, DataTransferEventArgs e)
        {
            var viewModel = DataContext as Page_Project_VM;
            if (viewModel != null)
            {
                Session.SelectedProject.LinkedFilesList = viewModel.DroppedFilePaths.ToList();
            }
        }
    }
}
