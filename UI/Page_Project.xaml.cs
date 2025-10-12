using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.ViewModels;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Project : UserControl
    {
        #region private Fields

        private readonly Page_Project_VM _vm;

        #endregion

        // Constructor
        public Page_Project()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)

            // View Model
            _vm = new Page_Project_VM();
            DataContext = _vm;
        }
        
        private void FileDropArea_OnSourceUpdated(object? sender, DataTransferEventArgs e)
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.LinkedFilesList = _vm.DroppedFilePaths.ToList();
        }
    }
}
