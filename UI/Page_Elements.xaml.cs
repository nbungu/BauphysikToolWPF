using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using System.Windows.Controls;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI
{
    public partial class Page_Elements : UserControl  // publisher of 'ElementSelectionChanged' event
    {
        // Constructor
        public Page_Elements()
        {
            InitializeElements();
            
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)
        }

        private void InitializeElements()
        {
            if (Session.SelectedProject is null) return;
            Session.SelectedProject.AssignInternalIdsToElements();
            Session.SelectedProject.AssignAsParentToElements();
            Session.SelectedProject.SortElements(ElementSortingType.DateDescending);
            Session.SelectedProject.RenderMissingElementImages();
        }
    }
}
