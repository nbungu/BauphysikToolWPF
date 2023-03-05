using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für EditLayerWindow.xaml
    /// </summary>
    public partial class EditLayerWindow : Window
    {
        // Instance Variable, when existing Elemenet is being edited and passed as Parameter
        private Layer layer;

        public EditLayerWindow(Layer layer)
        {
            this.layer = layer;

            InitializeComponent();

            if (layer is null)
                return;

            // Pre set TextBoxes and Header Label
            // Header Label
            headerLabel_MaterialName.Content = layer.Material.Name;
            headerLabel_MaterialCategory.Content = layer.Material.Category;

            // TextBoxes
            layerThickness_TextBox.Text = layer.LayerThickness.ToString();
            layerLambda_TextBox.Text = layer.Material.ThermalConductivity.ToString();
            layerDensity_TextBox.Text = layer.Material.BulkDensity.ToString();
            layerDiffResistance_TextBox.Text = layer.Material.DiffusionResistance.ToString();
        }

        private void apply_Button_Click(object sender, RoutedEventArgs e)
        {
            // check if every value is set
            if (layerThickness_TextBox.Text != "" && layerLambda_TextBox.Text != "" && layerDensity_TextBox.Text != "" && layerDiffResistance_TextBox.Text != "")
            {
                // TODO: validate values
                layer.LayerThickness = Convert.ToDouble(layerThickness_TextBox.Text);
                layer.Material.ThermalConductivity = Convert.ToDouble(layerLambda_TextBox.Text);
                layer.Material.BulkDensity = Convert.ToInt32(layerDensity_TextBox.Text);
                layer.Material.DiffusionResistance = Convert.ToDouble(layerDiffResistance_TextBox.Text);
                // TODO: Flag in Layer Mowdel hen Material was modded and add new "custom" MaterialId

                // Update in Database
                DatabaseAccess.UpdateLayer(layer);
                // Just Close this after editing existing Element
                this.Close();
            }
        }
    }
}
