using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für NewElementWindow.xaml
    /// </summary>
    public partial class NewElementWindow : Window
    {
        private Project project = FO0_LandingPage.Project;
        private Element element;

        public NewElementWindow(Element element = null)
        {
            this.element = element;

            InitializeComponent();

            // Pre set TextBox and ComboBox to edit existing Element
            if (element != null)
            {
                elementName_TextBox.Text = element.Name;
                construcitonType_Picker.SelectedItem = element.Construction.Type;
            }
        }

        private void apply_Button_Click(object sender, RoutedEventArgs e)
        {
            // check if name and construction type is set
            if (construcitonType_Picker.SelectedIndex != -1 && elementName_TextBox.Text != "")
            {
                string elementName = elementName_TextBox.Text;
                string constrType = construcitonType_Picker.SelectedItem.ToString();
                int constrId = DatabaseAccess.GetConstructions().Find(e => e.Type == constrType).ConstructionId;

                // If no Element in Parameter -> Create New
                if (this.element == null)
                {
                    Element newElem = new Element()
                    {
                        //ElementId gets set by SQLite DB (AutoIncrement)
                        Name = elementName,
                        ConstructionId = constrId,
                        Construction = new Construction() { ConstructionId = constrId, Type = constrType },
                        ProjectId = project.ProjectId,
                        Project = project,
                        Layers = new List<Layer>(),
                        EnvVars = new List<EnvVars>()
                    };
                    FO0_LandingPage.SelectedElement = newElem;
                    DatabaseAccess.CreateElement(newElem);
                    this.Close();
                    MainWindow.SetPage("Setup");
                }
                // If Element in Parameter -> Edit existing Element (SelectedElement from FO0_LandingPage)
                else
                {
                    this.element.Name = elementName;
                    this.element.ConstructionId = constrId;
                    this.element.ProjectId = project.ProjectId;  
                    DatabaseAccess.UpdateElement(this.element);
                    this.Close();
                }
            }
        }
    }
}
