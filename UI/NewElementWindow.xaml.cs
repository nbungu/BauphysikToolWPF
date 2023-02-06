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
        private Element element;
        private Project project;
        public NewElementWindow(Project project, Element element = null)
        {
            this.element = element;
            this.project = project; 
            InitializeComponent();

            // Pre set TextBox and ComboBox to edit existing Element
            if (element != null)
            {
                elementName_TextBox.Text = element.Name;
                construcitonType_Picker.SelectedItem = element.ConstructionType.Name;
            }
        }

        private void apply_Button_Click(object sender, RoutedEventArgs e)
        {
            // check if name and construction type is set
            if (construcitonType_Picker.SelectedIndex != -1 && elementName_TextBox.Text != "")
            {
                string elementName = elementName_TextBox.Text;
                string constrName = construcitonType_Picker.SelectedItem.ToString();
                int constrId = DatabaseAccess.GetConstructionTypes().Find(e => e.Name == constrName).ConstructionTypeId;

                // If no Element in Parameter -> Create New
                if (this.element == null)
                {
                    Element newElem = new Element()
                    {
                        //ElementId gets set by SQLite DB (AutoIncrement)
                        Name = elementName,
                        ConstructionTypeId = constrId,
                        ConstructionType = new ConstructionType() { ConstructionTypeId = constrId, Name = constrName },
                        ProjectId = project.ProjectId,
                        Project = project,
                        Layers = new List<Layer>(),
                        EnvVars = new List<EnvVars>()
                    };
                    DatabaseAccess.CreateElement(newElem);
                    FO0_LandingPage.SelectedElement = newElem;
                    this.Close();
                    MainWindow.SetPage("Setup");
                }
                // If Element in Parameter -> Edit existing Element
                else
                {
                    this.element.Name = elementName;
                    this.element.ConstructionTypeId = constrId;
                    this.element.ProjectId = project.ProjectId;  
                    DatabaseAccess.UpdateElement(this.element);
                    this.Close();
                }
            }
        }
    }
}
