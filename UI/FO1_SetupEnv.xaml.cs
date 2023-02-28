using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für FO1_SetupEnv.xaml
    /// </summary>
    public partial class FO1_SetupEnv : UserControl
    {
        // Class Variables - Belongs to the Class-Type itself and stay the same


        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call.


        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_SetupEnv()
        {   
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

        }

        // event handlers - subscribers

        // Custom Methods        

        // UI Methods

        // When User sets Custom Value for EnvVars
        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Handle the input
            string userInput = e.Text;
            Regex regex = new Regex("[^0-9,-]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);

            // only allow one decimal point
            if (userInput == "." && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            //set new value as UserSaved Data
            /*switch ((sender as TextBox).Name)
            {
                case "Ti_Input":
                    UserSaved.Ti = Convert.ToDouble(Ti_Input.Text + userInput);
                    Ti_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Te_Input":
                    UserSaved.Te = Convert.ToDouble(Te_Input.Text + userInput);
                    Te_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rsi_Input":
                    UserSaved.Rsi = Convert.ToDouble(Rsi_Input.Text + userInput);
                    Rsi_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rse_Input":
                    UserSaved.Rse = Convert.ToDouble(Rse_Input.Text + userInput);
                    Rse_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rel_Fi_Input":
                    UserSaved.Rel_Fi = Convert.ToDouble(Rel_Fi_Input.Text + userInput);
                    Rel_Fi_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                case "Rel_Fe_Input":
                    UserSaved.Rel_Fe = Convert.ToDouble(Rel_Fe_Input.Text + userInput);
                    Rel_Fe_ComboBox.SelectedIndex = -1; // set empty selection
                    return;
                default: throw new ArgumentException("Could not assign value");
            }*/
        }

    }
}
