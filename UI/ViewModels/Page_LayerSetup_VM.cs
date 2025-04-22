using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using Point = BT.Geometry.Point;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_LayerSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_LayerSetup_VM : ObservableObject
    {
        private readonly CrossSectionDrawing _crossSection = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM()
        {
            if (Session.SelectedProject is null) return;
            if (Session.SelectedElement is null) return;

            Session.SelectedLayerId = -1;
            Session.SelectedElement.SortLayers();
            Session.SelectedElement.AssignEffectiveLayers();
            Session.SelectedElement.AssignInternalIdsToLayers();

            // Allow child Windows to trigger UpdateAll of this Window
            Session.SelectedElementChanged += UpdateElement;
            Session.SelectedLayerChanged += UpdateAll;
            Session.EnvVarsChanged += UpdateRecalculateFlag;

            // For values changed in PropertyDataGrid TextBox
            PropertyItem<double>.PropertyChanged += TriggerSelectedLayerChanged;
            PropertyItem<int>.PropertyChanged += TriggerSelectedLayerChanged;
            PropertyItem<bool>.PropertyChanged += TriggerSelectedLayerChanged;
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
            new AddElementWindow(editExsiting: true).ShowDialog();
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
        private void AddSubConstructionLayer(int targetLayerInternalId = -1)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            if (targetLayerInternalId == -1) targetLayerInternalId = Session.SelectedLayerId;
            new AddLayerSubConstructionWindow(targetLayerInternalId: targetLayerInternalId).ShowDialog();
        }
        [RelayCommand]
        private void EditSubConstructionLayer(int targetLayerInternalId = -1)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            if (targetLayerInternalId == -1) targetLayerInternalId = Session.SelectedLayerId;
            new AddLayerSubConstructionWindow(targetLayerInternalId: targetLayerInternalId).ShowDialog();
        }

        [RelayCommand]
        private void DeleteSubConstructionLayer(int targetLayerInternalId = -1)
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = Session.SelectedLayerId;
            var targetLayer = Session.SelectedElement?.Layers.FirstOrDefault(l => l?.InternalId == targetLayerInternalId, null);
            targetLayer?.RemoveSubConstruction();
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            if (Session.SelectedElement is null) return;
            if (Session.SelectedLayer is null) return;

            Session.SelectedElement.RemoveLayer(Session.SelectedLayerId);
            Session.OnSelectedLayerChanged();
            SelectedListViewItem = null;
        }

        [RelayCommand]
        private void DuplicateLayer()
        {
            if (Session.SelectedElement is null) return;
            if (Session.SelectedLayer is null) return;

            Session.SelectedElement.DuplicateLayer(Session.SelectedLayerId);
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            if (Session.SelectedElement is null) return;
            if (Session.SelectedLayer is null) return;

            Session.SelectedElement.MoveLayerPositionToOutside(Session.SelectedLayerId);
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            if (Session.SelectedElement is null) return;
            if (Session.SelectedLayer is null) return;

            Session.SelectedElement.MoveLayerPositionToInside(Session.SelectedLayerId);
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void LayerDoubleClick()
        {
            EditLayer();
        }

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Layer? value)
        {
            if (value is null) return;
            if (Session.SelectedElement is null) return;

            Session.SelectedElement.Layers.ForEach(l => l.IsSelected = false);
            Session.SelectedLayerId = value.InternalId;

            if (Session.SelectedLayer is null) return;
            Session.SelectedLayer.IsSelected = true;

            _crossSection.UpdateDrawings();
            //_crossSection.UpdateSingleDrawing(Session.SelectedLayer);
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLayerSelected))]
        [NotifyPropertyChangedFor(nameof(SubConstructionExpanderVisibility))]
        [NotifyPropertyChangedFor(nameof(LayerPropertiesExpanderVisibility))]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        [NotifyPropertyChangedFor(nameof(CrossSectionDrawing))]
        private Layer? _selectedListViewItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LayerMeasurement))]
        [NotifyPropertyChangedFor(nameof(LayerMeasurementFull))]
        [NotifyPropertyChangedFor(nameof(SubConstructionMeasurement))]
        [NotifyPropertyChangedFor(nameof(CrossSectionDrawing))]
        [NotifyPropertyChangedFor(nameof(CanvasSize))]
        private List<Layer>? _layerList = Session.SelectedElement?.Layers;

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
        * Not Observable, not directly mutated by user input
        */

        public Element? SelectedElement => Session.SelectedElement;
        public bool IsLayerSelected => SelectedListViewItem != null;
        public Visibility SubConstructionExpanderVisibility => IsLayerSelected && SelectedListViewItem.HasSubConstructions ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LayerPropertiesExpanderVisibility => IsLayerSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoLayersVisibility => LayerList?.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public List<IDrawingGeometry> CrossSectionDrawing => _crossSection.DrawingGeometries;
        public Rectangle CanvasSize => _crossSection.CanvasSize;
        public List<DrawingGeometry> LayerMeasurement => MeasurementDrawing.GetLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementDrawing.GetSubConstructionMeasurementChain(_crossSection);
        public List<DrawingGeometry> LayerMeasurementFull => MeasurementDrawing.GetFullLayerMeasurementChain(_crossSection);

        // TODO: On Layer->IsActive change long loading times
        public List<IPropertyItem> LayerProperties => Session.SelectedLayer != null ? new List<IPropertyItem>()
        {
            new PropertyItem<string>("Kategorie", () => Session.SelectedLayer.Material.CategoryName),
            new PropertyItem<string>("Materialquelle", () => Session.SelectedLayer.Material.IsUserDefined ? "Benutzerdefiniert" : "aus Materialdatenbank"),
            new PropertyItem<double>(Symbol.Thickness, () => Session.SelectedLayer.Thickness, value => Session.SelectedLayer.Thickness = value),
            new PropertyItem<double>(Symbol.ThermalConductivity, () => Session.SelectedLayer.Material.ThermalConductivity) { DecimalPlaces = 3},
            new PropertyItem<double>(Symbol.RValueLayer, () => Session.SelectedLayer.R_Value)
            {
                SymbolSubscriptText = $"{Session.SelectedLayer.LayerNumber}"
            },
            new PropertyItem<int>(Symbol.RawDensity, () => Session.SelectedLayer.Material.BulkDensity),
            new PropertyItem<double>(Symbol.AreaMassDensity, () => Session.SelectedLayer.AreaMassDensity),
            new PropertyItem<double>(Symbol.SdThickness, () => Session.SelectedLayer.Sd_Thickness),
            new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => Session.SelectedLayer.Material.DiffusionResistance),
            new PropertyItem<int>(Symbol.SpecificHeatCapacity, () => Session.SelectedLayer.Material.SpecificHeatCapacity),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => Session.SelectedLayer.ArealHeatCapacity)
            {
                SymbolSubscriptText = $"{Session.SelectedLayer.LayerNumber}"
            },
            new PropertyItem<bool>("Wirksame Schicht", () => Session.SelectedLayer.IsEffective, value => Session.SelectedLayer.IsEffective = value)
        } : new List<IPropertyItem>(0);

        public List<IPropertyItem> SubConstructionProperties => Session.SelectedLayer != null && Session.SelectedLayer.HasSubConstructions ? new List<IPropertyItem>()
        {
            new PropertyItem<string>("Material", () => Session.SelectedLayer.SubConstruction.Material.Name),
            new PropertyItem<string>("Kategorie", () => Session.SelectedLayer.SubConstruction.Material.CategoryName),
            new PropertyItem<int>("Ausrichtung", () => (int)Session.SelectedLayer.SubConstruction.Direction, value => Session.SelectedLayer.SubConstruction.Direction = (SubConstructionDirection)value)
            {
                PropertyValues = SubConstructionDirectionMapping.Values.Cast<object>().ToArray()
            },
            new PropertyItem<double>(Symbol.Thickness, () => Session.SelectedLayer.SubConstruction.Thickness, value => Session.SelectedLayer.SubConstruction.Thickness = value),
            new PropertyItem<double>(Symbol.Width, () => Session.SelectedLayer.SubConstruction.Width, value => Session.SelectedLayer.SubConstruction.Width = value),
            new PropertyItem<double>(Symbol.Distance, () => Session.SelectedLayer.SubConstruction.Spacing, value => Session.SelectedLayer.SubConstruction.Spacing = value),
            new PropertyItem<double>("Achsenabstand", Symbol.Distance, () => Session.SelectedLayer.SubConstruction.AxisSpacing, value => Session.SelectedLayer.SubConstruction.AxisSpacing = value),
            new PropertyItem<double>(Symbol.ThermalConductivity, () => Session.SelectedLayer.SubConstruction.Material.ThermalConductivity) { DecimalPlaces = 3},
            new PropertyItem<double>(Symbol.RValueLayer, () => Session.SelectedLayer.SubConstruction.R_Value)
            {
                SymbolSubscriptText = $"{Session.SelectedLayer.LayerNumber}b"
            },
            new PropertyItem<double>(Symbol.AreaMassDensity, () => Session.SelectedLayer.SubConstruction.AreaMassDensity),
            new PropertyItem<double>(Symbol.SdThickness, () => Session.SelectedLayer.SubConstruction.Sd_Thickness),
            new PropertyItem<double>(Symbol.ArealHeatCapacity, () => Session.SelectedLayer.SubConstruction.ArealHeatCapacity)
            {
                SymbolSubscriptText = $"{Session.SelectedLayer.LayerNumber}b"
            },
            new PropertyItem<bool>("Wirksame Schicht", () => Session.SelectedLayer.SubConstruction.IsEffective, value => Session.SelectedLayer.SubConstruction.IsEffective = value)
        } : new List<IPropertyItem>(0);

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
                double? value = (_tiIndex == -1) ? Session.Ti : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureInterior).Find(e => e.Comment == TiKeys[_tiIndex])?.Value;
                // Save SessionData
                Session.Ti = value ?? 0.0;
                // Return value to UIElement
                return Session.Ti;
            }
            set
            {
                // Save custom user input
                Session.Ti = value;
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                TiIndex = -1;
            }
        }
        public double TeValue
        {
            get
            {
                double? value = (_teIndex == -1) ? Session.Te : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TemperatureExterior).Find(e => e.Comment == TeKeys[_teIndex])?.Value;
                Session.Te = value ?? 0.0;
                return Session.Te;
            }
            set
            {
                Session.Te = value;
                TeIndex = -1;
            }
        }
        public double RsiValue
        {
            get
            {
                double? value = (_rsiIndex == -1) ? Session.Rsi : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceInterior).Find(e => e.Comment == RsiKeys[_rsiIndex])?.Value;
                Session.Rsi = value ?? 0.0;
                return Session.Rsi;
            }
            set
            {
                Session.Rsi = value;
                RsiIndex = -1;
            }
        }
        public double RseValue
        {
            get
            {
                double? value = (_rseIndex == -1) ? Session.Rse : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.TransferResistanceSurfaceExterior).Find(e => e.Comment == RseKeys[_rseIndex])?.Value;
                Session.Rse = value ?? 0.0;
                return Session.Rse;
            }
            set
            {
                Session.Rse = value;
                RseIndex = -1;
            }
        }
        public double RelFiValue
        {
            get
            {
                double? value = (_relFiIndex == -1) ? Session.RelFi : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityInterior).Find(e => e.Comment == RelFiKeys[_relFiIndex])?.Value;
                Session.RelFi = value ?? 0.0;
                return Session.RelFi;
            }
            set
            {
                Session.RelFi = value;
                RelFiIndex = -1;
            }
        }
        public double RelFeValue
        {
            get
            {
                double? value = (_relFeIndex == -1) ? Session.RelFe : DatabaseAccess.QueryEnvVarsBySymbol(Symbol.RelativeHumidityExterior).Find(e => e.Comment == RelFeKeys[_relFeIndex])?.Value;
                Session.RelFe = value ?? 0.0;
                return Session.RelFe;
            }
            set
            {
                Session.RelFe = value;
                RelFeIndex = -1;
            }
        }

        /*
         * Trigger Hooks
         */

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>
        private void UpdateAll()
        {
            UpdateRecalculateFlag();

            _crossSection.UpdateDrawings();

            LayerList = null;
            LayerList = Session.SelectedElement?.Layers;
            SelectedListViewItem = null;
            SelectedListViewItem = Session.SelectedLayer;

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(LayerProperties));
            OnPropertyChanged(nameof(LayerMeasurement));
            OnPropertyChanged(nameof(LayerMeasurementFull));
            OnPropertyChanged(nameof(SubConstructionMeasurement));
            OnPropertyChanged(nameof(CrossSectionDrawing));
            OnPropertyChanged(nameof(CanvasSize));
            OnPropertyChanged(nameof(NoLayersVisibility));
        }
        private void UpdateElement()
        {
            UpdateRecalculateFlag();

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(SelectedElement));
        }
        private void UpdateRecalculateFlag()
        {
            Session.Recalculate = true;
        }
        private void TriggerSelectedLayerChanged()
        {
            Session.OnSelectedLayerChanged();
        }
    }
}
