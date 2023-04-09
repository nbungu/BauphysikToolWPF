using BauphysikToolWPF.SQLiteRepo;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    public partial class EditElementWindow : Window
    {
        // Instance Variable, when existing Elemenet is being edited and passed as Parameter
        public static Element? SelectedElement { get; private set; }  

        public EditElementWindow(Element? selectedElement = null)
        {
            SelectedElement = selectedElement;

            InitializeComponent();
        }
    }
}
