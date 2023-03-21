using BauphysikToolWPF.SQLiteRepo;
using System.Linq;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    public partial class EditElementWindow : Window
    {
        // Instance Variable, when existing Elemenet is being edited and passed as Parameter
        private Element? selectedElement;

        public EditElementWindow(Element? selectedElement = null)
        {
            this.selectedElement = selectedElement;

            InitializeComponent();

            construction_Picker.ItemsSource = DatabaseAccess.GetConstructions().Select(e => e.Type).ToList();
            orientation_Picker.ItemsSource = DatabaseAccess.GetOrientations().Select(e => e.TypeName).ToList();

            // Pre set TextBox and ComboBox to edit existing Element
            if (selectedElement != null)
            {
                elementName_TextBox.Text = selectedElement.Name;
                construction_Picker.SelectedItem = selectedElement.Construction.Type;
                orientation_Picker.SelectedItem = selectedElement.Orientation.TypeName;
            }
        }

        private void apply_Button_Click(object sender, RoutedEventArgs e)
        {
            // Avoid empty Input fields
            if (construction_Picker.SelectedIndex != -1 && orientation_Picker.SelectedIndex != -1 && elementName_TextBox.Text != string.Empty)
            {
                string constrType = construction_Picker.SelectedItem.ToString() ?? "";
                int constrId = DatabaseAccess.GetConstructions().Find(e => e.Type == constrType)?.ConstructionId ?? -1;

                string orientationType = orientation_Picker.SelectedItem.ToString() ?? "";
                int orientationId = DatabaseAccess.GetOrientations().Find(e => e.TypeName == orientationType)?.OrientationId ?? -1;
                
                if (constrId == -1 || orientationId == -1)
                    return;

                // If no Element in Parameter -> Create New
                if (selectedElement is null)
                {
                    Element newElem = new Element()
                    {
                        // ElementId gets set by SQLite DB (AutoIncrement)
                        Name = elementName_TextBox.Text,
                        ConstructionId = constrId,
                        OrientationId = orientationId,
                        ProjectId = FO0_ProjectPage.ProjectId,
                    };
                    // Update in Database
                    DatabaseAccess.CreateElement(newElem);

                    //Set as selected Element
                    FO0_LandingPage.SelectedElementId = newElem.ElementId;

                    // Go to Setup Page (Editor) after creating new Element
                    this.Close();
                    MainWindow.SetPage(NavigationContent.SetupLayer);
                }
                // If Element in Parameter -> Edit existing Element (SelectedElement from FO0_LandingPage)
                else
                {
                    selectedElement.Name = elementName_TextBox.Text;
                    selectedElement.ConstructionId = constrId;
                    selectedElement.OrientationId = orientationId;
                    selectedElement.ProjectId = FO0_ProjectPage.ProjectId;

                    // Update in Database
                    DatabaseAccess.UpdateElement(selectedElement);
                    // Just Close this after editing existing Element
                    this.Close();
                }
            }
        }
    }
}
