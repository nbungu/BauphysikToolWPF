using System.Windows.Controls;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.ViewModels;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Elements : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Constructor
        public Page_Elements()
        {
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)

            // View Model
            this.DataContext = new Page_Elements_VM(); // View Model

            Session.SelectedProject.RenderMissingElementImages();
        }
    }
}
