using BauphysikToolWPF.SQLiteRepo;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für EditLayerWindow.xaml
    /// </summary>
    public partial class EditLayerWindow : Window
    {
        // Class Variable, make SelectedLayer accessible for ViewModel
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
