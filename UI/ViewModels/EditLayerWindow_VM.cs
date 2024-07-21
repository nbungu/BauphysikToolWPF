using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Windows;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for EditLayerWindow.xaml: Used in xaml as "DataContext"
    public partial class EditLayerWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from EditLayerWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => "EditLayerWindow";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void ApplyChanges(Window? window)
        {
            // To be able to Close EditLayerWindow from within this ViewModel
            if (window is null) return;

            // When Material values or Name has changed 
            if (IsCustomMaterial)
            {
                if (UserSaved.SelectedLayer.Material.IsUserDefined)
                {
                    // Update exisiting Material if already 'UserDefined'
                    UserSaved.SelectedLayer.Material.Name = Name;
                    UserSaved.SelectedLayer.Material.ThermalConductivity = Convert.ToDouble(ThermalConductivity, CultureInfo.CurrentCulture);
                    UserSaved.SelectedLayer.Material.BulkDensity = Convert.ToInt32(BulkDensity, CultureInfo.CurrentCulture);
                    UserSaved.SelectedLayer.Material.DiffusionResistance = Convert.ToDouble(DiffusionResistance, CultureInfo.CurrentCulture);
                    UserSaved.SelectedLayer.Material.SpecificHeatCapacity = Convert.ToInt32(HeatCapacity, CultureInfo.CurrentCulture);
                }
                else
                {
                    // Create new Material
                    var customMaterial = UserSaved.SelectedLayer.Material.Copy();
                    customMaterial.Name = Name + " (Edited)";
                    customMaterial.IsUserDefined = true;
                    customMaterial.ThermalConductivity = Convert.ToDouble(ThermalConductivity, CultureInfo.CurrentCulture);
                    customMaterial.BulkDensity = Convert.ToInt32(BulkDensity, CultureInfo.CurrentCulture);
                    customMaterial.DiffusionResistance = Convert.ToDouble(DiffusionResistance, CultureInfo.CurrentCulture);
                    customMaterial.SpecificHeatCapacity = Convert.ToInt32(HeatCapacity, CultureInfo.CurrentCulture);

                    // Create in Database
                    DatabaseAccess.CreateMaterial(customMaterial);
                    // Bind to new Material via Id as FK
                    UserSaved.SelectedLayer.Material = customMaterial;
                    UserSaved.SelectedLayer.MaterialId = customMaterial.Id;
                }
            }
            // Update Layer thickness
            UserSaved.SelectedLayer.Thickness = Convert.ToDouble(Thickness, CultureInfo.CurrentCulture);
            // Trigger Event to Update Layer Window
            UserSaved.OnSelectedLayerChanged();
            window.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _name = UserSaved.SelectedLayer.Material.Name;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _thickness = UserSaved.SelectedLayer.Thickness.ToString(CultureInfo.CurrentCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _thermalConductivity = UserSaved.SelectedLayer.Material.ThermalConductivity.ToString(CultureInfo.CurrentCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _bulkDensity = UserSaved.SelectedLayer.Material.BulkDensity.ToString(CultureInfo.CurrentCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _diffusionResistance = UserSaved.SelectedLayer.Material.DiffusionResistance.ToString(CultureInfo.CurrentCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _heatCapacity = UserSaved.SelectedLayer.Material.SpecificHeatCapacity.ToString(CultureInfo.CurrentCulture);


        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         * 
         * Only 'IsDataValid' gets Notified due to usage in frontend (needs updated value at runtime) to toggle Button-State
         * 'IsCustomMaterial' only gets evaluated after Button Click (no runtime update for frontend needed)
         */

        public bool IsCustomMaterial
        {
            get
            {
                if (!UserSaved.SelectedLayer.IsValid) return false;
                return 
                    Name != UserSaved.SelectedLayer.Material.Name ||
                    ThermalConductivity != UserSaved.SelectedLayer.Material.ThermalConductivity.ToString(CultureInfo.CurrentCulture) ||
                    BulkDensity != UserSaved.SelectedLayer.Material.BulkDensity.ToString(CultureInfo.CurrentCulture) ||
                    DiffusionResistance != UserSaved.SelectedLayer.Material.DiffusionResistance.ToString(CultureInfo.CurrentCulture) ||
                    HeatCapacity != UserSaved.SelectedLayer.Material.SpecificHeatCapacity.ToString(CultureInfo.CurrentCulture);
            }
        }
        
        public bool IsDataValid => (Name != "" && Thickness != "" && ThermalConductivity != "" && BulkDensity != "" && DiffusionResistance != "" && HeatCapacity != ""); // Check if all TextBoxes have non-empty Values
    }
}
