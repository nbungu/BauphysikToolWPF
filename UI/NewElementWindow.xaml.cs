using BauphysikToolWPF.SQLiteRepo;
using System.Collections.Generic;
using System.Windows;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für NewElementWindow.xaml
    /// </summary>
    public partial class NewElementWindow : Window
    {
        public NewElementWindow()
        {
            InitializeComponent();
        }

        private void createElement_Button_Click(object sender, RoutedEventArgs e)
        {
            // check if name and construction type is set
            if (construcitonType_Picker.SelectedIndex != -1 && elementName_TextBox.Text != "")
            {
                string constrName = construcitonType_Picker.SelectedItem.ToString();
                int constrId = DatabaseAccess.GetConstructionTypes().Find(e => e.Name == constrName).ConstructionTypeId;

                Element element = new Element()
                {
                    //ElementId gets set by SQLite DB (AutoIncrement)
                    Name = elementName_TextBox.Text,
                    ConstructionTypeId = constrId,
                    ConstructionType = new ConstructionType() { ConstructionTypeId = constrId, Name = constrName },
                    Layers = new List<Layer>(),
                    EnvVars = new List<EnvVars>()
                };
                DatabaseAccess.CreateElement(element);
                FO0_LandingPage.SelectedElement = element;
                this.Close();
                MainWindow.SetPage("Setup");
            }
        }
    }
}
