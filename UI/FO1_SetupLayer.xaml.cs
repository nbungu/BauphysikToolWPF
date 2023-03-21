using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.UI.Helper;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class FO1_SetupLayer : UserControl
    {
        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call.

        private List<Layer>? layers;

        // Class Variables - Belongs to the Class-Type itself and stay the same

        // Recalculate Flags - Save computation time by avoiding unnecessary new instances
        public static bool RecalculateTemp { get; set; } = true;
        public static bool RecalculateGlaser { get; set; } = true;

        // For use in current Instance only
        private int currentElementId { get; } = FO0_LandingPage.SelectedElementId;

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public FO1_SetupLayer()
        {   
            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            // Drawing
            layers = DatabaseAccess.QueryLayersByElementId(currentElementId);
            new DrawLayerCanvas(layers_Canvas, layers);         // Initial Draw of the Canvas
            new DrawMeasurementLine(measurement_Grid, layers);  // Initial Draw of the measurement line

            // Event Subscription - Register with Events
            DatabaseAccess.LayersChanged += DB_LayersChanged;
            UserSaved.EnvVarsChanged += Session_EnvVarsChanged;
            FO0_LandingPage.SelectedElementChanged += Session_SelectedElementChanged;
        }

        // event handlers - subscribers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Update UI
            layers = DatabaseAccess.QueryLayersByElementId(currentElementId);
            new DrawLayerCanvas(layers_Canvas, layers);         // Redraw Canvas
            new DrawMeasurementLine(measurement_Grid, layers);  // Redraw measurement line

            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateGlaser = true;
        }
        public void Session_EnvVarsChanged()
        {
            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateGlaser = true;
        }

        public void Session_SelectedElementChanged()
        {
            // Update Recalculate Flag
            RecalculateTemp = true;
            RecalculateGlaser = true;
        }

        // custom Methods

        public void UpdateElementEnvVars(int elementID, EnvVars envVar)
        {
            // Add m:n realtion to Database
            ElementEnvVars elemEnvVars = new ElementEnvVars()
            {
                //Id gets set by SQLite (Autoincrement)
                ElementId = elementID,
                EnvVarId = envVar.EnvVarId,
            };
            // Only insert every envVar once! 
            if (DatabaseAccess.UpdateElementEnvVars(elemEnvVars) == 0)
                // If no row is updated ( == 0), create a new one
                DatabaseAccess.CreateElementEnvVars(elemEnvVars);
        }

        // UI Methods

        // Redraw on every Click on Canvas to show current selected Layer with blue border
        private void layers_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (layers_ListView.SelectedItem is null)
                return;

            var layer = layers_ListView.SelectedItem as Layer;
            if (layer is null)
                return;

            // deselect every other layer
            foreach (Layer l in layers_ListView.ItemsSource)
            {
                l.IsSelected = false;
            }
            // select only one at a time
            layer.IsSelected = true;

            // Redraw to show selected layer, but take Layers from ItemsSource to keep the IsSelected Property
            new DrawLayerCanvas(layers_Canvas, layers_ListView.ItemsSource as List<Layer>);
        }

        // Save current canvas as image, just before closing FO1_SetupLayer Page
        // 'Unloaded' event was called after FO0 Initialize();
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only save if leaving this page
            if (this.IsVisible)
                return;

            //TODO: Deselect every Layer to remove blue Border around Layer

            Element currentElement = DatabaseAccess.QueryElementById(currentElementId);
            currentElement.Image = DrawLayerCanvas.SaveAsBLOB(layers_Canvas);

            // Update in Database
            DatabaseAccess.UpdateElement(currentElement);
        }
    }
}
