using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            if (elementName_TextBox.Text == String.Empty)
                this.Close();

            Element element = new Element()
            {
                //ElementId gets set by SQLite DB (AutoIncrement)
                Name = elementName_TextBox.Text
            };
            DatabaseAccess.CreateElement(element);
            FO1_Setup.ElementId = DatabaseAccess.GetElements().Last().ElementId;
            this.Close();
            MainWindow.SetPage("Setup");
        }
    }
}
