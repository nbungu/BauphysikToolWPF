using BauphysikToolWPF.SQLiteRepo;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    public partial class EditLayerWindow : Window
    {
        // Class Variable, make SelectedLayer from parent Window (FO1_Layer) accessible for ViewModel
        public static Layer? SelectedLayer { get; private set; }

        public EditLayerWindow(Layer layer)
        {
            if (layer is null)
                return;

            SelectedLayer = layer;

            InitializeComponent();
        }
    }
}
