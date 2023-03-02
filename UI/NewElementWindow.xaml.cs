using BauphysikToolWPF.SQLiteRepo;
using System.Linq;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für NewElementWindow.xaml
    /// </summary>
    public partial class NewElementWindow : Window
    {
        // Instance Variable, when existing Elemenet is being edited and passed as Parameter
        private Element? selectedElement;

        public NewElementWindow(Element selectedElement = null)
        {
            this.selectedElement = selectedElement;

            InitializeComponent();

            constructionType_Picker.ItemsSource = DatabaseAccess.GetConstructions().Select(e => e.Type).ToList();

            // Pre set TextBox and ComboBox to edit existing Element
            if (selectedElement != null)
            {
                elementName_TextBox.Text = selectedElement.Name;
                constructionType_Picker.SelectedItem = selectedElement.Construction.Type;
            }
        }

        private void apply_Button_Click(object sender, RoutedEventArgs e)
        {
            // check if name and construction type is set
            if (constructionType_Picker.SelectedIndex != -1 && elementName_TextBox.Text != string.Empty)
            {
                string constrType = constructionType_Picker.SelectedItem.ToString();
                int constrId = DatabaseAccess.GetConstructions().Find(e => e.Type == constrType).ConstructionId;

                // If no Element in Parameter -> Create New
                if (this.selectedElement == null)
                {
                    Element newElem = new Element()
                    {
                        // ElementId gets set by SQLite DB (AutoIncrement)
                        Name = elementName_TextBox.Text,
                        ConstructionId = constrId,
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
