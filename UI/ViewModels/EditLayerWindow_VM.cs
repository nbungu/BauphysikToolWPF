using BauphysikToolWPF.SQLiteRepo;
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
        public string Title { get; } = "EditLayerWindow";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void ApplyChanges(Window? window)
        {
            // To be able to Close EditLayerWindow from within this ViewModel
            if (window is null || EditLayerWindow.SelectedLayer is null) return;

            // When Material values or Name has changed 
            if (IsCustomMaterial)
            {
                if (EditLayerWindow.SelectedLayer.Material.Category == MaterialCategory.UserDefined)
                {
                    // Update exisiting Material if already 'UserDefined'
                    EditLayerWindow.SelectedLayer.Material.Name = Name;
                    EditLayerWindow.SelectedLayer.Material.ThermalConductivity = Convert.ToDouble(ThermalConductivity);
                    EditLayerWindow.SelectedLayer.Material.BulkDensity = Convert.ToInt32(BulkDensity);
                    EditLayerWindow.SelectedLayer.Material.DiffusionResistance = Convert.ToDouble(DiffusionResistance);
                    EditLayerWindow.SelectedLayer.Material.SpecificHeatCapacity = Convert.ToInt32(HeatCapacity);

                    // Update in Database
                    DatabaseAccess.UpdateMaterial(EditLayerWindow.SelectedLayer.Material);
                }
                else
                {
                    // Create new Material
                    Material usedDefinedMaterial = new Material()
                    {
                        Name = (Name == EditLayerWindow.SelectedLayer.Material.Name) ? Name + "-Edited" : Name,
                        CategoryName = "Benutzerdefiniert",
                        ThermalConductivity = Convert.ToDouble(ThermalConductivity),
                        BulkDensity = Convert.ToInt32(BulkDensity),
                        DiffusionResistance = Convert.ToDouble(DiffusionResistance),
                        SpecificHeatCapacity = Convert.ToInt32(HeatCapacity),
                        //TODO auch anpassbar machen
                        ColorCode = EditLayerWindow.SelectedLayer.Material.ColorCode
                    };

                    // Create in Database
                    DatabaseAccess.CreateMaterial(usedDefinedMaterial);
                    // Bind to new Material via MaterialId as FK
                    EditLayerWindow.SelectedLayer.MaterialId = usedDefinedMaterial.MaterialId;
                }
            }

            // Update Layer thickness
            EditLayerWindow.SelectedLayer.LayerThickness = Convert.ToDouble(Thickness);
            // Update Layer in Database
            DatabaseAccess.UpdateLayer(EditLayerWindow.SelectedLayer);

            window.Close();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _name = EditLayerWindow.SelectedLayer is null ? "" : EditLayerWindow.SelectedLayer.Material.Name;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _thickness = EditLayerWindow.SelectedLayer is null ? "" : EditLayerWindow.SelectedLayer.LayerThickness.ToString(CultureInfo.InvariantCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _thermalConductivity = EditLayerWindow.SelectedLayer is null ? "" : EditLayerWindow.SelectedLayer.Material.ThermalConductivity.ToString(CultureInfo.InvariantCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _bulkDensity = EditLayerWindow.SelectedLayer is null ? "" : EditLayerWindow.SelectedLayer.Material.BulkDensity.ToString();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _diffusionResistance = EditLayerWindow.SelectedLayer is null ? "" : EditLayerWindow.SelectedLayer.Material.DiffusionResistance.ToString(CultureInfo.InvariantCulture);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDataValid))]
        private string _heatCapacity = EditLayerWindow.SelectedLayer is null ? "" : EditLayerWindow.SelectedLayer.Material.SpecificHeatCapacity.ToString();


        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         * 
         * Only 'IsDataValid' gets Notified due to usage in frontend (needs updated value at runtime) to toggle Button-State
         * 'IsCustomMaterial' only gets evaluated after Button Click (no runtime update for frontend needed)
         */

        public bool IsCustomMaterial =>
            Name != EditLayerWindow.SelectedLayer.Material.Name ||
            ThermalConductivity != EditLayerWindow.SelectedLayer.Material.ThermalConductivity.ToString() ||
            BulkDensity != EditLayerWindow.SelectedLayer.Material.BulkDensity.ToString() ||
            DiffusionResistance != EditLayerWindow.SelectedLayer.Material.DiffusionResistance.ToString() ||
            HeatCapacity != EditLayerWindow.SelectedLayer.Material.SpecificHeatCapacity.ToString();

        public bool IsDataValid => (Name != "" && Thickness != "" && ThermalConductivity != "" && BulkDensity != "" && DiffusionResistance != "" && HeatCapacity != ""); // Check if all TextBoxes have non-empty Values
    }
}
