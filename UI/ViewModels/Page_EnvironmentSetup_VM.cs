using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Models.Helper;
using Geometry;

namespace BauphysikToolWPF.UI.ViewModels
{
    // ViewModel for Page_EnvironmentSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_EnvironmentSetup_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public string Title = "SetupEnv";

        private readonly CanvasDrawingService _drawingService = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 400, 880), DrawingType.VerticalCut);


        /*
         * Static Class Properties:
         * If List<string> is null, then get List from Database. If List is already loaded, use existing List.
         * To only load Propery once. Every other getter request then uses the static class variable.
         */

        public List<string> Ti_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureInterior).Select(e => e.Comment).ToList();
        public List<string> Te_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureExterior).Select(e => e.Comment).ToList();
        public List<string> Rsi_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceInterior).Select(e => e.Comment).ToList();
        public List<string> Rse_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceExterior).Select(e => e.Comment).ToList();
        public List<string> Rel_Fi_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityInterior).Select(e => e.Comment).ToList();
        public List<string> Rel_Fe_Keys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityExterior).Select(e => e.Comment).ToList();

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void EditElement() // Binding in XAML via 'EditElementCommand'
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditElementWindow().ShowDialog();

            // Update XAML Binding Property by fetching from DB
            OnPropertyChanged(nameof(SelectedElement));
        }

        // Manually Trigger this method whenever SortingPropertyIndex changes
        partial void OnLayersChanged(List<Layer> value)
        {
            // Updates the Layer Geometry
            //UserSaved.SelectedElement.GetCrossSectionDrawing();

            // Update XAML Binding Property by fetching from DB
            OnPropertyChanged(nameof(SelectedElement));
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values (int default = 0)
         */

        // Add m:n realtion to Database when new selection is set
        //TODO implement again
        //UpdateElementEnvVars(Id, currentEnvVar);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TiValue))]
        private static int ti_Index; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TeValue))]
        private static int te_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RsiValue))]
        private static int rsi_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RseValue))]
        private static int rse_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFiValue))]
        private static int rel_fi_Index;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFeValue))]
        private static int rel_fe_Index;

        [ObservableProperty]
        private List<Layer> _layers = UserSaved.SelectedElement.Layers;
        
        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the _selection Values above
         */

        public List<IDrawingGeometry> DrawingGeometries => _drawingService.DrawingGeometries;
        public Rectangle CanvasSize => _drawingService.CanvasSize;

        public List<DrawingGeometry> LayerMeasurement => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers, Axis.X).ToList();
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementChain.GetMeasurementChain(_drawingService.DrawingGeometries.Where(g => g.ZIndex == 1), Axis.Z).ToList();
        public List<DrawingGeometry> LayerMeasurementFull => UserSaved.SelectedElement.Layers.Count > 1 ? MeasurementChain.GetMeasurementChain(new[] { 0, 400.0 }, Axis.X).ToList() : new List<DrawingGeometry>();

        public string TiValue
        {
            get
            {
                // Index is 0:
                // On Initial Startup (default value for not assigned int)
                // Index is -1:
                // On custom user input

                //Get corresp Value
                double? value = (ti_Index == -1) ? UserSaved.Ti : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureInterior).Find(e => e.Comment == Ti_Keys[ti_Index])?.Value;
                // Save SessionData
                UserSaved.Ti = value ?? 0;
                // Return value to UIElement
                return value.ToString() ?? string.Empty;
            }
            set
            {
                // Save custom user input
                UserSaved.Ti = Convert.ToDouble(value);
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                Ti_Index = -1;
            }
        }
        public string TeValue
        {
            get
            {
                double? value = (te_Index == -1) ? UserSaved.Te : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureExterior).Find(e => e.Comment == Te_Keys[te_Index])?.Value;
                UserSaved.Te = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Te = Convert.ToDouble(value);
                Te_Index = -1;
            }
        }
        public string RsiValue
        {
            get
            {
                double? value = (rsi_Index == -1) ? UserSaved.Rsi : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceInterior).Find(e => e.Comment == Rsi_Keys[rsi_Index])?.Value;
                UserSaved.Rsi = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rsi = Convert.ToDouble(value);
                Rsi_Index = -1;
            }
        }
        public string RseValue
        {
            get
            {
                double? value = (rse_Index == -1) ? UserSaved.Rse : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceExterior).Find(e => e.Comment == Rse_Keys[rse_Index])?.Value;
                UserSaved.Rse = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rse = Convert.ToDouble(value);
                Rse_Index = -1;
            }
        }
        public string RelFiValue
        {
            get
            {
                double? value = (rel_fi_Index == -1) ? UserSaved.Rel_Fi : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityInterior).Find(e => e.Comment == Rel_Fi_Keys[rel_fi_Index])?.Value;
                UserSaved.Rel_Fi = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rel_Fi = Convert.ToDouble(value);
                Rel_fi_Index = -1;
            }
        }
        public string RelFeValue
        {
            get
            {
                double? value = (rel_fe_Index == -1) ? UserSaved.Rel_Fe : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityExterior).Find(e => e.Comment == Rel_Fe_Keys[rel_fe_Index])?.Value;
                UserSaved.Rel_Fe = value ?? 0;
                return value.ToString() ?? string.Empty;
            }
            set
            {
                UserSaved.Rel_Fe = Convert.ToDouble(value);
                Rel_fe_Index = -1;
            }
        }
    }
}
