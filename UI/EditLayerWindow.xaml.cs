using System.Windows;
using BauphysikToolWPF.Models;

namespace BauphysikToolWPF.UI
{
    public partial class EditLayerWindow : Window
    {
        // Class Variable, make SelectedLayer from parent Window (FO1_Layer) accessible for ViewModel
        public static Layer? SelectedLayer { get; private set; }

        public EditLayerWindow(Layer? layer = null)
        {
            SelectedLayer = layer;
            InitializeComponent();
        }
    }
}
