using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    public partial class Page_LayerSetup : UserControl
    {
        // Instance Variables - only for "MainPage" Instances. Variables get re-assigned on every 'new' Instance call of this Class.

        // Class Variables - Belongs to the Class-Type itself and stay the same
        public static bool Recalculate { get; set; } = true;

        // (Instance-) Contructor - when 'new' Keyword is used to create class (e.g. when toggling pages via menu navigation)
        public Page_LayerSetup()
        {
            if (UserSaved.SelectedElement != null)
            {
                UserSaved.SelectedElement.SortLayers();
                UserSaved.SelectedElement.AssignEffectiveLayers();
                UserSaved.SelectedElement.AssignInternalIdsToLayers();
            }

            // UI Elements in backend only accessible AFTER InitializeComponent() was executed
            InitializeComponent(); // Initializes xaml objects -> Calls constructors for all referenced Class Bindings in the xaml (from DataContext, ItemsSource etc.)                                                    

            // Event Subscription - Register with Events
            DatabaseAccess.LayersChanged += DB_LayersChanged;
            UserSaved.EnvVarsChanged += Session_EnvVarsChanged;
            UserSaved.SelectedElementChanged += Session_SelectedElementChanged;
        }

        // event handlers - subscribers
        public void DB_LayersChanged() // has to match the signature of the delegate (return type void, no input parameters)
        {
            // Update Recalculate Flag
            Recalculate = true;
        }
        public void Session_EnvVarsChanged()
        {
            // Update Recalculate Flag
            Recalculate = true;
        }

        public void Session_SelectedElementChanged()
        {
            // Update Recalculate Flag
            Recalculate = true;
        }

        // UI Methods

        // Save current canvas as image, just before closing Page_LayerSetup Page
        // 'Unloaded' event was called after FO0 Initialize();
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only save if leaving this page
            if (IsVisible) return;
            UserSaved.SelectedElement.Layers.ForEach(l => l.IsSelected = false);
            UserSaved.SelectedElement.Image = (UserSaved.SelectedElement.Layers.Count != 0) ? SaveCanvas.SaveAsBLOB(LayersCanvas) : Array.Empty<byte>();
        }
    }
}
