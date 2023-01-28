using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ListBox navigationMenuListBox;

        public static Window Main;
        
        public MainWindow()
        {
            InitializeComponent();
            navigationMenuListBox = this.NavigationMenuListBox;
            Main = this;
        }
        public static void SetPage(string page)
        {
            navigationMenuListBox.SelectedItem = page; // MainWindow.xaml Binding changes the ContentPage based on the SelectedItem string
        }
    }
}
