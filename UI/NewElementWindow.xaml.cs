using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

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
            if (constructionType_Picker.SelectedIndex != -1 && elementName_TextBox.Text != "")
            {
                string elementName = elementName_TextBox.Text;
                string constrType = constructionType_Picker.SelectedItem.ToString();
                int constrId = DatabaseAccess.GetConstructions().Find(e => e.Type == constrType).ConstructionId;

                // If no Element in Parameter -> Create New
                if (this.selectedElement == null)
                {
                    Element newElem = new Element()
                    {
                        // ElementId gets set by SQLite DB (AutoIncrement)
                        Name = elementName,
                        ConstructionId = constrId,
                        ProjectId = FO0_LandingPage.ProjectId,
                    };
                    // TODO: set directly as selectedElement via ID
                    // TODO: Link with FO0_ViewModel to updateElements

                    // Update in Database
                    DatabaseAccess.CreateElement(newElem);
                    // Go to Setup Page (Editor) after creating new Element
                    this.Close();
                    //MainWindow.SetPage("Setup");
                }
                // If Element in Parameter -> Edit existing Element (SelectedElement from FO0_LandingPage)
                else
                {
                    this.selectedElement.Name = elementName;
                    this.selectedElement.ConstructionId = constrId;
                    this.selectedElement.ProjectId = FO0_LandingPage.ProjectId;

                    // Update in Database
                    DatabaseAccess.UpdateElement(this.selectedElement);
                    // Just Close this after editing existing Element
                    this.Close();
                }
            }
        }
    }
}
