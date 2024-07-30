using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using MeasurementChain = BauphysikToolWPF.UI.Drawing.MeasurementChain;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_LayerSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_LayerSetup_VM : ObservableObject
    {
        private readonly CanvasDrawingService _drawingService = new CanvasDrawingService(UserSaved.SelectedElement, new Rectangle(new Point(0, 0), 880, 400));

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM()
        {
            UserSaved.SelectedLayerId = -1;

            // Subscribe to Event and Handle
            // Allow child Windows to trigger UpdateBindingsAndRecalculateFlag of this Window
            UserSaved.SelectedElementChanged += UpdateBindingsAndRecalculateFlag;
            UserSaved.SelectedLayerChanged += UpdateBindingsAndRecalculateFlag;

            // For values changed in PropertyDataGrid TextBox
            PropertyItem<double>.PropertyChanged += UpdateBindingsAndRecalculateFlag;
            PropertyItem<SubConstructionDirection>.PropertyChanged += UpdateBindingsAndRecalculateFlag;
            PropertyItem<bool>.PropertyChanged += UpdateBindingsAndRecalculateFlag;
        }
        
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
            new AddElementWindow().ShowDialog();
        }

        [RelayCommand]
        private void AddLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerWindow(editExsiting: false).ShowDialog();
        }
        [RelayCommand]
        private void EditLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerWindow(editExsiting: IsLayerSelected).ShowDialog();
        }

        [RelayCommand]
        private void AddSubConstructionLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerSubConstructionWindow(editExisting: false).ShowDialog();
        }
        [RelayCommand]
        private void EditSubConstructionLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerSubConstructionWindow(editExisting: IsLayerSelected && SelectedListViewItem.HasSubConstructions).ShowDialog();
        }

        [RelayCommand]
        private void DeleteSubConstructionLayer()
        {
            UserSaved.SelectedLayer.RemoveSubConstruction();
            UserSaved.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            UserSaved.SelectedElement.RemoveLayer(UserSaved.SelectedLayerId);
            UserSaved.OnSelectedLayerChanged();
        }
        
        [RelayCommand]
        private void DuplicateLayer()
        {
            UserSaved.SelectedElement.DuplicateLayer(UserSaved.SelectedLayerId);
            UserSaved.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            UserSaved.SelectedElement.MoveLayerPositionToOutside(UserSaved.SelectedLayerId);
            UserSaved.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            UserSaved.SelectedElement.MoveLayerPositionToInside(UserSaved.SelectedLayerId);
            UserSaved.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void LayerDoubleClick()
        {
            if (SelectedListViewItem is null) return;
            EditLayer();
        }
        
        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Layer? value)
        {
            if (value is null) return;
            UserSaved.SelectedElement.Layers.ForEach(l => l.IsSelected = false);
            UserSaved.SelectedLayerId = value.InternalId;
            UserSaved.SelectedLayer.IsSelected = true;

            _drawingService.UpdateDrawings();
        }

        /*
         * MVVM Properties: Observable, if user can change these properties via frontend directly
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(LayerMeasurement))]
        [NotifyPropertyChangedFor(nameof(LayerMeasurementFull))]
        [NotifyPropertyChangedFor(nameof(SubConstructionMeasurement))]
        [NotifyPropertyChangedFor(nameof(CrossSectionDrawing))]
        [NotifyPropertyChangedFor(nameof(CanvasSize))]
        private List<Layer> _layerList = UserSaved.SelectedElement.Layers;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLayerSelected))]
        [NotifyPropertyChangedFor(nameof(ShowLayerExpander))]
        [NotifyPropertyChangedFor(nameof(ShowSubConstructionExpander))]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        [NotifyPropertyChangedFor(nameof(CrossSectionDrawing))]
        private Layer _selectedListViewItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TiValue))]
        private static int _tiIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TeValue))]
        private static int _teIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RsiValue))]
        private static int _rsiIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RseValue))]
        private static int _rseIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFiValue))]
        private static int _relFiIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFeValue))]
        private static int _relFeIndex;

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, No direct User Input involved
         */

        public bool IsLayerSelected => SelectedListViewItem != null;
        public bool ShowLayerExpander => IsLayerSelected;
        public bool ShowSubConstructionExpander => IsLayerSelected && SelectedListViewItem.HasSubConstructions;
        public List<IDrawingGeometry> CrossSectionDrawing => _drawingService.DrawingGeometries;
        public Rectangle CanvasSize => _drawingService.CanvasSize;
        public List<DrawingGeometry> LayerMeasurement => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers).ToList();
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementChain.GetMeasurementChain(_drawingService.DrawingGeometries.Where(g => g.ZIndex == 1), Axis.X).ToList();
        public List<DrawingGeometry> LayerMeasurementFull => UserSaved.SelectedElement.Layers.Count > 1 ? MeasurementChain.GetMeasurementChain(new[] {0, 400.0 }).ToList() : new List<DrawingGeometry>();

        public List<IPropertyItem> LayerProperties => new List<IPropertyItem>()
        {
            new PropertyItem<string>("Kategorie", () => UserSaved.SelectedLayer.Material.CategoryName),
            new PropertyItem<string>("Materialquelle", () => UserSaved.SelectedLayer.Material.IsUserDefined ? "Benutzerdefiniert" : "aus Materialdatenbank"),
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedLayer.Thickness, value => UserSaved.SelectedLayer.Thickness = value),
            new PropertyItem<double>(Symbol.ThermalConductivity, () => UserSaved.SelectedLayer.Material.ThermalConductivity),
            new PropertyItem<double>(Symbol.RValueLayer, () => UserSaved.SelectedLayer.R_Value)
            {
                SymbolSubscriptText = $"{UserSaved.SelectedLayer.LayerPosition}"
            },
            new PropertyItem<double>(Symbol.RawDensity, () => UserSaved.SelectedLayer.Material.BulkDensity),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => UserSaved.SelectedLayer.AreaMassDensity),
            new PropertyItem<double>(Symbol.SdThickness, () => UserSaved.SelectedLayer.Sd_Thickness),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => UserSaved.SelectedLayer.Material.DiffusionResistance),
            new PropertyItem<double>(Symbol.SpecificHeatCapacity, () => UserSaved.SelectedLayer.Material.SpecificHeatCapacity),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => UserSaved.SelectedLayer.ArealHeatCapacity)
            {
                SymbolSubscriptText = $"{UserSaved.SelectedLayer.LayerPosition}"
            },
            new PropertyItem<bool>("Wirksame Schicht", () => UserSaved.SelectedLayer.IsEffective, value => UserSaved.SelectedLayer.IsEffective = value)
        };

        public List<IPropertyItem> SubConstructionProperties => UserSaved.SelectedLayer.HasSubConstructions ? new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => UserSaved.SelectedLayer.SubConstruction.Material.Name),
            new PropertyItem<string>("Kategorie", () => UserSaved.SelectedLayer.SubConstruction.Material.CategoryName),
            new PropertyItem<SubConstructionDirection>("Ausrichtung", () => UserSaved.SelectedLayer.SubConstruction.SubConstructionDirection, value => UserSaved.SelectedLayer.SubConstruction.SubConstructionDirection = value)
            {
                PropertyValues = Enum.GetValues(typeof(SubConstructionDirection)).Cast<object>().ToArray(),
            },
            new PropertyItem<double>(Symbol.Thickness, () => UserSaved.SelectedLayer.SubConstruction.Thickness, value => UserSaved.SelectedLayer.SubConstruction.Thickness = value),
            new PropertyItem<double>(Symbol.Width, () => UserSaved.SelectedLayer.SubConstruction.Width, value => UserSaved.SelectedLayer.SubConstruction.Width = value),
            new PropertyItem<double>(Symbol.Distance, () => UserSaved.SelectedLayer.SubConstruction.Spacing, value => UserSaved.SelectedLayer.SubConstruction.Spacing = value),
            new PropertyItem<double>("Achsenabstand", Symbol.Distance, () => UserSaved.SelectedLayer.SubConstruction.AxisSpacing, value => UserSaved.SelectedLayer.SubConstruction.AxisSpacing = value),
            new PropertyItem<double>(Symbol.ThermalConductivity, () => UserSaved.SelectedLayer.SubConstruction.Material.ThermalConductivity),
            new PropertyItem<double>(Symbol.RValueLayer, () => UserSaved.SelectedLayer.SubConstruction.R_Value)
            {
                SymbolSubscriptText = $"{UserSaved.SelectedLayer.LayerPosition}b"
            },
            new PropertyItem<double>(Symbol.AreaMassDensity, () => UserSaved.SelectedLayer.SubConstruction.AreaMassDensity),
            new PropertyItem<double>(Symbol.SdThickness, () => UserSaved.SelectedLayer.SubConstruction.Sd_Thickness),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => UserSaved.SelectedLayer.SubConstruction.ArealHeatCapacity)
            {
                SymbolSubscriptText = $"{UserSaved.SelectedLayer.LayerPosition}b"
            },
            new PropertyItem<bool>("Wirksame Schicht", () => UserSaved.SelectedLayer.SubConstruction.IsEffective, value => UserSaved.SelectedLayer.SubConstruction.IsEffective = value)
        } : new List<IPropertyItem>();

        public List<string> TiKeys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureInterior).Select(e => e.Comment).ToList();
        public List<string> TeKeys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureExterior).Select(e => e.Comment).ToList();
        public List<string> RsiKeys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceInterior).Select(e => e.Comment).ToList();
        public List<string> RseKeys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceExterior).Select(e => e.Comment).ToList();
        public List<string> RelFiKeys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityInterior).Select(e => e.Comment).ToList();
        public List<string> RelFeKeys { get; } = DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityExterior).Select(e => e.Comment).ToList();

        public double TiValue
        {
            get
            {
                // Index is 0:
                // On Initial Startup (default value for not assigned int)
                // Index is -1:
                // On custom user input

                //Get corresp Value
                double? value = (_tiIndex == -1) ? UserSaved.Ti : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureInterior).Find(e => e.Comment == TiKeys[_tiIndex])?.Value;
                // Save SessionData
                UserSaved.Ti = value ?? 0.0;
                // Return value to UIElement
                return UserSaved.Ti;
            }
            set
            {
                // Save custom user input
                UserSaved.Ti = value;
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                TiIndex = -1;
            }
        }
        public double TeValue
        {
            get
            {
                double? value = (_teIndex == -1) ? UserSaved.Te : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureExterior).Find(e => e.Comment == TeKeys[_teIndex])?.Value;
                UserSaved.Te = value ?? 0.0;
                return UserSaved.Te;
            }
            set
            {
                UserSaved.Te = value;
                TeIndex = -1;
            }
        }
        public double RsiValue
        {
            get
            {
                double? value = (_rsiIndex == -1) ? UserSaved.Rsi : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceInterior).Find(e => e.Comment == RsiKeys[_rsiIndex])?.Value;
                UserSaved.Rsi = value ?? 0.0;
                return UserSaved.Rsi;
            }
            set
            {
                UserSaved.Rsi = value;
                RsiIndex = -1;
            }
        }
        public double RseValue
        {
            get
            {
                double? value = (_rseIndex == -1) ? UserSaved.Rse : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceExterior).Find(e => e.Comment == RseKeys[_rseIndex])?.Value;
                UserSaved.Rse = value ?? 0.0;
                return UserSaved.Rse;
            }
            set
            {
                UserSaved.Rse = value;
                RseIndex = -1;
            }
        }
        public double RelFiValue
        {
            get
            {
                double? value = (_relFiIndex == -1) ? UserSaved.Rel_Fi : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityInterior).Find(e => e.Comment == RelFiKeys[_relFiIndex])?.Value;
                UserSaved.Rel_Fi = value ?? 0.0;
                return UserSaved.Rel_Fi;
            }
            set
            {
                UserSaved.Rel_Fi = value;
                RelFiIndex = -1;
            }
        }
        public double RelFeValue
        {
            get
            {
                double? value = (_relFeIndex == -1) ? UserSaved.Rel_Fe : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityExterior).Find(e => e.Comment == RelFeKeys[_relFeIndex])?.Value;
                UserSaved.Rel_Fe = value ?? 0.0;
                return UserSaved.Rel_Fe;
            }
            set
            {
                UserSaved.Rel_Fe = value;
                RelFeIndex = -1;
            }
        }

        /*
         * Custom Methods
         */

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>
        private void UpdateBindingsAndRecalculateFlag()
        {
            UserSaved.Recalculate = true;

            _drawingService.UpdateDrawings();

            LayerList = null;
            LayerList = UserSaved.SelectedElement.Layers;
            SelectedElement = null;
            SelectedElement = UserSaved.SelectedElement;
            SelectedListViewItem = null;
            SelectedListViewItem = UserSaved.SelectedLayer;
        }
    }
}
